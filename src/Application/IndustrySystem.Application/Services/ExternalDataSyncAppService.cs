using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Application.Services;

public sealed class ExternalDataSyncAppService : IExternalDataSyncAppService, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ExternalSyncOptions _options;
    private readonly IExternalSyncChannelFactory _channelFactory;
    private readonly Func<IMaterialAppService> _materialServiceFactory;
    private readonly Func<IShelfAppService> _shelfServiceFactory;
    private readonly Func<IExperimentAppService> _experimentServiceFactory;
    // 串行化生命周期切换（Start/Stop），避免状态重叠。
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    // 单次仅允许一个同步周期执行，保证顺序稳定并避免重入。
    private readonly SemaphoreSlim _syncGate = new(1, 1);
    // 死信文件单写门闩，防止并发追加导致行内容交错。
    private readonly SemaphoreSlim _deadLetterFileGate = new(1, 1);
    // 按端点复用通道实例，保持 Socket/SignalR 长连接。
    private readonly ConcurrentDictionary<string, IExternalSyncChannel> _channels = new(StringComparer.OrdinalIgnoreCase);
    // endpoint::messageId -> 处理时间，用于幂等去重窗口。
    private readonly ConcurrentDictionary<string, DateTimeOffset> _dedupStore = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<ExternalSyncDeadLetterItem> _deadLetters = new();

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private long _totalProcessed;
    private long _totalFailed;
    private long _totalRetried;
    private long _deadLetterCount;
    private long _lastBatchLatencyMilliseconds;
    private long _lastDedupCleanupTicks;
    private DateTimeOffset? _lastSyncedAt;
    private string? _lastError;

    public ExternalDataSyncAppService(
        ExternalSyncOptions options,
        IExternalSyncChannelFactory channelFactory,
        Func<IMaterialAppService> materialServiceFactory,
        Func<IShelfAppService> shelfServiceFactory,
        Func<IExperimentAppService> experimentServiceFactory)
    {
        _options = options;
        _channelFactory = channelFactory;
        _materialServiceFactory = materialServiceFactory;
        _shelfServiceFactory = shelfServiceFactory;
        _experimentServiceFactory = experimentServiceFactory;
    }

    public Task<ExternalSyncRuntimeStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        var status = new ExternalSyncRuntimeStatusDto(
            Enabled: _options.Enabled,
            IsRunning: _loopTask is { IsCompleted: false },
            EndpointCount: _options.Endpoints.Count(x => x.Enabled),
            TotalProcessedCount: Interlocked.Read(ref _totalProcessed),
            TotalFailedCount: Interlocked.Read(ref _totalFailed),
            TotalRetriedCount: Interlocked.Read(ref _totalRetried),
            DeadLetterCount: Interlocked.Read(ref _deadLetterCount),
            LastBatchLatencyMilliseconds: Interlocked.Read(ref _lastBatchLatencyMilliseconds),
            LastSyncedAt: _lastSyncedAt,
            LastError: _lastError);

        return Task.FromResult(status);
    }

    public async Task<int> SyncOnceAsync(CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            return 0;
        }

        // 使用异步门闩而非 lock，避免 await 场景下死锁。
        await _syncGate.WaitAsync(ct).ConfigureAwait(false);
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var processed = 0;
            var endpoints = _options.Endpoints.Where(x => x.Enabled).ToList();

            foreach (var endpoint in endpoints)
            {
                ct.ThrowIfCancellationRequested();

                try
                {
                    processed += await ExecuteWithRetryAsync(
                        endpoint,
                        token => SyncEndpointAsync(endpoint, token),
                        ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _totalFailed);
                    _lastError = ex.Message;
                    WriteStructuredLog("Error", "endpoint_sync_failed", new
                    {
                        endpoint = endpoint.Name,
                        protocol = endpoint.Protocol.ToString(),
                        error = ex.Message
                    });
                }
            }

            _lastSyncedAt = DateTimeOffset.UtcNow;
            Interlocked.Add(ref _totalProcessed, processed);
            return processed;
        }
        finally
        {
            stopwatch.Stop();
            Interlocked.Exchange(ref _lastBatchLatencyMilliseconds, stopwatch.ElapsedMilliseconds);
            _syncGate.Release();
        }
    }

    public async Task StartAsync(CancellationToken ct = default)
    {
        await _lifecycleGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (!_options.Enabled)
            {
                return;
            }

            if (_loopTask is { IsCompleted: false })
            {
                return;
            }

            // 后台启动同步循环，不阻塞调用线程。
            _loopCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token), CancellationToken.None);
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken ct = default)
    {
        CancellationTokenSource? loopCts;
        Task? loopTask;

        await _lifecycleGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            loopCts = _loopCts;
            loopTask = _loopTask;
            _loopCts = null;
            _loopTask = null;
        }
        finally
        {
            _lifecycleGate.Release();
        }

        if (loopTask is null)
        {
            await DisposeChannelsAsync(ct).ConfigureAwait(false);
            return;
        }

        loopCts?.Cancel();

        try
        {
            // 在生命周期门闩外等待任务完成，避免“持锁 + await”死锁模式。
            await loopTask.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // 停止过程中的取消可忽略。
        }
        finally
        {
            loopCts?.Dispose();
        }

        await DisposeChannelsAsync(ct).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        _lifecycleGate.Dispose();
        _syncGate.Dispose();
        _deadLetterFileGate.Dispose();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await SyncOnceAsync(ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref _totalFailed);
                    _lastError = ex.Message;
                    WriteStructuredLog("Error", "sync_loop_failed", new { error = ex.Message });
                }

                var delaySeconds = ResolvePollingSeconds();
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Stop 触发的取消属于预期行为。
        }
        finally
        {
            await DisposeChannelsAsync(CancellationToken.None).ConfigureAwait(false);
        }
    }

    private int ResolvePollingSeconds()
    {
        var endpointMin = _options.Endpoints
            .Where(x => x.Enabled)
            .Select(x => x.PollingIntervalSeconds)
            .Where(x => x > 0)
            .DefaultIfEmpty(_options.DefaultPollingIntervalSeconds)
            .Min();

        return endpointMin <= 0 ? 5 : endpointMin;
    }

    private async Task<int> SyncEndpointAsync(ExternalSyncEndpointOptions endpoint, CancellationToken ct)
    {
        // 端点键保证每个配置端点只对应一个通道实例。
        var channel = _channels.GetOrAdd(BuildEndpointKey(endpoint), _ => _channelFactory.Create(endpoint));
        if (!channel.IsConnected)
        {
            await channel.ConnectAsync(ct).ConfigureAwait(false);
        }

        var messages = await channel.ReceiveBatchAsync(ct).ConfigureAwait(false);
        if (messages.Count == 0)
        {
            return 0;
        }

        var processed = 0;
        // 仅对已接收或已跳过的消息发送 ACK，便于上游安全推进游标。
        var ackCandidates = new List<ExternalSyncMessage>(messages.Count);

        foreach (var message in messages)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(message.MessageId))
            {
                await RecordDeadLetterAsync(
                    endpoint,
                    message,
                    "missing_message_id",
                    "MessageId is empty.",
                    ct).ConfigureAwait(false);
                ackCandidates.Add(message);
                continue;
            }

            var dedupKey = BuildDedupKey(endpoint.Name, message.MessageId);
            if (IsDuplicate(dedupKey, DateTimeOffset.UtcNow))
            {
                WriteStructuredLog("Information", "dedup_skip", new
                {
                    endpoint = endpoint.Name,
                    messageId = message.MessageId
                });
                ackCandidates.Add(message);
                continue;
            }

            try
            {
                var applied = await ExecuteWithRetryAsync(
                    endpoint,
                    token => ApplyMessageAsync(endpoint, message, token),
                    ct).ConfigureAwait(false);

                processed += applied;
                MarkProcessed(dedupKey, DateTimeOffset.UtcNow);
                ackCandidates.Add(message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalFailed);
                _lastError = ex.Message;
                WriteStructuredLog("Error", "message_apply_failed", new
                {
                    endpoint = endpoint.Name,
                    messageId = message.MessageId,
                    error = ex.Message
                });
            }
        }

        if (ackCandidates.Count > 0)
        {
            try
            {
                await ExecuteWithRetryAsync(
                    endpoint,
                    token => AcknowledgeAsync(channel, ackCandidates, token),
                    ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _totalFailed);
                _lastError = ex.Message;
                WriteStructuredLog("Warning", "ack_failed", new
                {
                    endpoint = endpoint.Name,
                    count = ackCandidates.Count,
                    error = ex.Message
                });
            }
        }

        return processed;
    }

    private async Task<int> ApplyMessageAsync(ExternalSyncEndpointOptions endpoint, ExternalSyncMessage message, CancellationToken ct)
    {
        return message.EntityType switch
        {
            ExternalSyncEntity.Material => await ApplyMaterialAsync(endpoint, message, ct).ConfigureAwait(false),
            ExternalSyncEntity.Shelf => await ApplyShelfAsync(endpoint, message, ct).ConfigureAwait(false),
            ExternalSyncEntity.Experiment => await ApplyExperimentAsync(endpoint, message, ct).ConfigureAwait(false),
            _ => 0
        };
    }

    private async Task<int> ApplyMaterialAsync(ExternalSyncEndpointOptions endpoint, ExternalSyncMessage message, CancellationToken ct)
    {
        var service = _materialServiceFactory();

        if (message.OperationType == ExternalSyncOperation.Delete)
        {
            if (!TryReadId(message.PayloadJson, out var id))
            {
                await RecordDeadLetterAsync(
                    endpoint,
                    message,
                    "material_delete_missing_id",
                    "Delete payload does not contain a valid id.",
                    ct).ConfigureAwait(false);
                return 0;
            }

            await service.DeleteAsync(id).ConfigureAwait(false);
            return 1;
        }

        if (!TryDeserializePayload<MaterialDto>(message.PayloadJson, out var payload))
        {
            await RecordDeadLetterAsync(
                endpoint,
                message,
                "material_payload_invalid",
                "Cannot deserialize MaterialDto payload.",
                ct).ConfigureAwait(false);
            return 0;
        }

        var existing = await service.GetAsync(payload.Id).ConfigureAwait(false);
        if (existing is not null && !ShouldApplyUpsert(message.PayloadJson, existing))
        {
            WriteStructuredLog("Information", "material_conflict_skip", new
            {
                endpoint = endpoint.Name,
                messageId = message.MessageId,
                policy = _options.Conflict.Policy.ToString()
            });
            return 0;
        }

        if (existing is null)
        {
            await service.CreateAsync(payload).ConfigureAwait(false);
        }
        else
        {
            await service.UpdateAsync(payload).ConfigureAwait(false);
        }

        return 1;
    }

    private async Task<int> ApplyShelfAsync(ExternalSyncEndpointOptions endpoint, ExternalSyncMessage message, CancellationToken ct)
    {
        var service = _shelfServiceFactory();

        if (message.OperationType == ExternalSyncOperation.Delete)
        {
            if (!TryReadId(message.PayloadJson, out var id))
            {
                await RecordDeadLetterAsync(
                    endpoint,
                    message,
                    "shelf_delete_missing_id",
                    "Delete payload does not contain a valid id.",
                    ct).ConfigureAwait(false);
                return 0;
            }

            await service.DeleteShelfAsync(id).ConfigureAwait(false);
            return 1;
        }

        if (!TryDeserializePayload<ShelfConfigDto>(message.PayloadJson, out var payload))
        {
            await RecordDeadLetterAsync(
                endpoint,
                message,
                "shelf_payload_invalid",
                "Cannot deserialize ShelfConfigDto payload.",
                ct).ConfigureAwait(false);
            return 0;
        }

        var existing = await service.GetShelfAsync(payload.Id).ConfigureAwait(false);
        if (existing is not null && !ShouldApplyUpsert(message.PayloadJson, existing))
        {
            WriteStructuredLog("Information", "shelf_conflict_skip", new
            {
                endpoint = endpoint.Name,
                messageId = message.MessageId,
                policy = _options.Conflict.Policy.ToString()
            });
            return 0;
        }

        if (existing is null)
        {
            await service.CreateShelfAsync(payload).ConfigureAwait(false);
        }
        else
        {
            await service.UpdateShelfAsync(payload).ConfigureAwait(false);
        }

        return 1;
    }

    private async Task<int> ApplyExperimentAsync(ExternalSyncEndpointOptions endpoint, ExternalSyncMessage message, CancellationToken ct)
    {
        var service = _experimentServiceFactory();

        if (message.OperationType == ExternalSyncOperation.Delete)
        {
            if (!TryReadId(message.PayloadJson, out var id))
            {
                await RecordDeadLetterAsync(
                    endpoint,
                    message,
                    "experiment_delete_missing_id",
                    "Delete payload does not contain a valid id.",
                    ct).ConfigureAwait(false);
                return 0;
            }

            await service.DeleteAsync(id).ConfigureAwait(false);
            return 1;
        }

        if (!TryDeserializePayload<ExperimentConfigUpsertDto>(message.PayloadJson, out var payload))
        {
            await RecordDeadLetterAsync(
                endpoint,
                message,
                "experiment_payload_invalid",
                "Cannot deserialize ExperimentConfigUpsertDto payload.",
                ct).ConfigureAwait(false);
            return 0;
        }

        var existing = await service.GetAsync(payload.Id).ConfigureAwait(false);
        if (existing is not null && !ShouldApplyUpsert(message.PayloadJson, existing))
        {
            WriteStructuredLog("Information", "experiment_conflict_skip", new
            {
                endpoint = endpoint.Name,
                messageId = message.MessageId,
                policy = _options.Conflict.Policy.ToString()
            });
            return 0;
        }

        if (existing is null)
        {
            await service.CreateAsync(payload).ConfigureAwait(false);
        }
        else
        {
            await service.UpdateAsync(payload).ConfigureAwait(false);
        }

        return 1;
    }

    private async Task<int> AcknowledgeAsync(
        IExternalSyncChannel channel,
        IReadOnlyList<ExternalSyncMessage> messages,
        CancellationToken ct)
    {
        await channel.AcknowledgeAsync(messages, ct).ConfigureAwait(false);
        return messages.Count;
    }

    private async Task<T> ExecuteWithRetryAsync<T>(
        ExternalSyncEndpointOptions endpoint,
        Func<CancellationToken, Task<T>> operation,
        CancellationToken ct)
    {
        var retry = endpoint.Retry ?? _options.Retry;

        var maxAttempts = retry.MaxAttempts <= 0 ? 1 : retry.MaxAttempts;
        var baseDelay = retry.BaseDelayMilliseconds <= 0 ? 250 : retry.BaseDelayMilliseconds;
        var maxDelay = retry.MaxDelayMilliseconds <= 0 ? 5000 : retry.MaxDelayMilliseconds;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return await operation(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                Interlocked.Increment(ref _totalRetried);
                _lastError = ex.Message;

                // 指数退避（可选抖动）用于降低重试风暴。
                var delayMs = ComputeBackoffDelay(attempt, baseDelay, maxDelay, retry.UseJitter);
                WriteStructuredLog("Warning", "retry_scheduled", new
                {
                    endpoint = endpoint.Name,
                    attempt,
                    maxAttempts,
                    delayMs,
                    error = ex.Message
                });

                await Task.Delay(TimeSpan.FromMilliseconds(delayMs), ct).ConfigureAwait(false);
            }
        }

        return await operation(ct).ConfigureAwait(false);
    }

    private static int ComputeBackoffDelay(int attempt, int baseDelay, int maxDelay, bool useJitter)
    {
        var scale = Math.Pow(2, Math.Max(0, attempt - 1));
        var delay = (int)Math.Min(maxDelay, baseDelay * scale);
        if (useJitter)
        {
            delay = Math.Min(maxDelay, delay + Random.Shared.Next(0, Math.Max(1, baseDelay)));
        }

        return Math.Max(1, delay);
    }

    private static string BuildEndpointKey(ExternalSyncEndpointOptions endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.Name))
        {
            return endpoint.Name;
        }

        return $"{endpoint.Protocol}:{endpoint.Url}";
    }

    private static string BuildDedupKey(string endpointName, string messageId) => $"{endpointName}::{messageId}";

    private bool IsDuplicate(string dedupKey, DateTimeOffset now)
    {
        if (!_options.Dedup.Enabled)
        {
            return false;
        }

        if (_dedupStore.TryGetValue(dedupKey, out var processedAt))
        {
            var ttl = TimeSpan.FromMinutes(Math.Max(1, _options.Dedup.WindowMinutes));
            if (now - processedAt <= ttl)
            {
                return true;
            }

            _dedupStore.TryRemove(dedupKey, out _);
        }

        return false;
    }

    private void MarkProcessed(string dedupKey, DateTimeOffset now)
    {
        if (!_options.Dedup.Enabled)
        {
            return;
        }

        _dedupStore[dedupKey] = now;
        CleanupDedupStore(now);
    }

    private void CleanupDedupStore(DateTimeOffset now)
    {
        var nowTicks = now.UtcTicks;
        var previousCleanupTicks = Interlocked.Read(ref _lastDedupCleanupTicks);
        if (nowTicks - previousCleanupTicks < TimeSpan.FromMinutes(5).Ticks)
        {
            return;
        }

        // 使用 CAS 避免并发调用重复执行清理逻辑。
        if (Interlocked.CompareExchange(ref _lastDedupCleanupTicks, nowTicks, previousCleanupTicks) != previousCleanupTicks)
        {
            return;
        }

        var ttl = TimeSpan.FromMinutes(Math.Max(1, _options.Dedup.WindowMinutes));
        foreach (var pair in _dedupStore)
        {
            if (now - pair.Value > ttl)
            {
                _dedupStore.TryRemove(pair.Key, out _);
            }
        }

        var maxEntries = Math.Max(1000, _options.Dedup.MaxEntries);
        if (_dedupStore.Count <= maxEntries)
        {
            return;
        }

        foreach (var pair in _dedupStore.OrderBy(x => x.Value).Take(_dedupStore.Count - maxEntries))
        {
            _dedupStore.TryRemove(pair.Key, out _);
        }
    }

    private bool ShouldApplyUpsert(string incomingPayloadJson, object existingEntity)
    {
        // 冲突策略在此统一决策，确保各实体处理行为一致。
        return _options.Conflict.Policy switch
        {
            ExternalSyncConflictPolicy.ExternalWins => true,
            ExternalSyncConflictPolicy.LocalWins => false,
            ExternalSyncConflictPolicy.NewerWins => IsIncomingNewerOrEqual(incomingPayloadJson, existingEntity),
            _ => true
        };
    }

    private bool IsIncomingNewerOrEqual(string incomingPayloadJson, object existingEntity)
    {
        if (string.IsNullOrWhiteSpace(incomingPayloadJson))
        {
            return true;
        }

        var versionField = string.IsNullOrWhiteSpace(_options.Conflict.VersionFieldName)
            ? "version"
            : _options.Conflict.VersionFieldName;
        var timestampField = string.IsNullOrWhiteSpace(_options.Conflict.TimestampFieldName)
            ? "updatedAt"
            : _options.Conflict.TimestampFieldName;

        if (!TryGetVersionAndTimestamp(incomingPayloadJson, versionField, timestampField, out var incomingVersion, out var incomingTimestamp))
        {
            return true;
        }

        var existingJson = JsonSerializer.Serialize(existingEntity, JsonOptions);
        if (!TryGetVersionAndTimestamp(existingJson, versionField, timestampField, out var localVersion, out var localTimestamp))
        {
            return true;
        }

        if (incomingVersion.HasValue && localVersion.HasValue)
        {
            return incomingVersion.Value >= localVersion.Value;
        }

        if (incomingTimestamp.HasValue && localTimestamp.HasValue)
        {
            return incomingTimestamp.Value >= localTimestamp.Value;
        }

        return true;
    }

    private static bool TryGetVersionAndTimestamp(
        string json,
        string versionField,
        string timestampField,
        out long? version,
        out DateTimeOffset? timestamp)
    {
        version = null;
        timestamp = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (TryGetPropertyIgnoreCase(doc.RootElement, versionField, out var versionElement))
            {
                version = ParseInt64(versionElement);
            }

            if (TryGetPropertyIgnoreCase(doc.RootElement, timestampField, out var timestampElement))
            {
                timestamp = ParseDateTimeOffset(timestampElement);
            }

            return version.HasValue || timestamp.HasValue;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement obj, string propertyName, out JsonElement value)
    {
        foreach (var property in obj.EnumerateObject())
        {
            if (property.NameEquals(propertyName) || property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static long? ParseInt64(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Number when element.TryGetInt64(out var number) => number,
            JsonValueKind.String when long.TryParse(element.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    private static DateTimeOffset? ParseDateTimeOffset(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String when DateTimeOffset.TryParse(element.GetString(), out var parsed) => parsed,
            _ => null
        };
    }

    private static bool TryDeserializePayload<TPayload>(string payloadJson, out TPayload payload)
    {
        payload = default!;

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return false;
        }

        try
        {
            var result = JsonSerializer.Deserialize<TPayload>(payloadJson, JsonOptions);
            if (result is null)
            {
                return false;
            }

            payload = result;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadId(string payloadJson, out Guid id)
    {
        id = Guid.Empty;

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!doc.RootElement.TryGetProperty("id", out var idElement)
                && !doc.RootElement.TryGetProperty("Id", out idElement))
            {
                return false;
            }

            return idElement.ValueKind switch
            {
                JsonValueKind.String => Guid.TryParse(idElement.GetString(), out id),
                _ => Guid.TryParse(idElement.GetRawText().Trim('"'), out id)
            };
        }
        catch
        {
            return false;
        }
    }

    private async Task RecordDeadLetterAsync(
        ExternalSyncEndpointOptions endpoint,
        ExternalSyncMessage message,
        string reason,
        string details,
        CancellationToken ct)
    {
        if (!_options.DeadLetter.Enabled)
        {
            return;
        }

        var item = new ExternalSyncDeadLetterItem(
            endpoint.Name,
            endpoint.Protocol,
            message.MessageId,
            message.EntityType,
            message.OperationType,
            reason,
            details,
            message.PayloadJson,
            DateTimeOffset.UtcNow);

        _deadLetters.Enqueue(item);
        TrimDeadLetterQueue();

        Interlocked.Increment(ref _deadLetterCount);
        Interlocked.Increment(ref _totalFailed);

        WriteStructuredLog("Warning", "dead_letter_added", new
        {
            endpoint = endpoint.Name,
            messageId = message.MessageId,
            reason,
            details
        });

        await PersistDeadLetterAsync(item, ct).ConfigureAwait(false);
    }

    private void TrimDeadLetterQueue()
    {
        var max = Math.Max(10, _options.DeadLetter.MaxInMemoryItems);
        while (_deadLetters.Count > max && _deadLetters.TryDequeue(out _))
        {
            // 移除最旧条目以控制内存上限。
        }
    }

    private async Task PersistDeadLetterAsync(ExternalSyncDeadLetterItem item, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.DeadLetter.FilePath))
        {
            return;
        }

        var outputPath = _options.DeadLetter.FilePath;
        if (!Path.IsPathRooted(outputPath))
        {
            outputPath = Path.Combine(AppContext.BaseDirectory, outputPath);
        }

        var folder = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var line = JsonSerializer.Serialize(item, JsonOptions) + Environment.NewLine;
        await _deadLetterFileGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            // 逐行追加，保持死信文件可读且便于流式处理。
            await File.AppendAllTextAsync(outputPath, line, ct).ConfigureAwait(false);
        }
        finally
        {
            _deadLetterFileGate.Release();
        }
    }

    private async Task DisposeChannelsAsync(CancellationToken ct)
    {
        var channels = _channels.ToArray();
        foreach (var entry in channels)
        {
            if (!_channels.TryRemove(entry.Key, out var channel))
            {
                continue;
            }

            try
            {
                await channel.DisconnectAsync(ct).ConfigureAwait(false);
            }
            catch
            {
                // 关闭阶段忽略通道停止异常。
            }

            try
            {
                await channel.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // 关闭阶段忽略通道释放异常。
            }
        }
    }

    private void WriteStructuredLog(string level, string eventName, object payload)
    {
        if (!_options.EnableStructuredLog)
        {
            return;
        }

        try
        {
            var envelope = JsonSerializer.Serialize(new
            {
                timestamp = DateTimeOffset.UtcNow,
                level,
                eventName,
                payload
            });

            Trace.WriteLine(envelope, "ExternalSync");
        }
        catch
        {
            // 日志异常不得影响同步主流程。
        }
    }

    private sealed record ExternalSyncDeadLetterItem(
        string EndpointName,
        ExternalSyncProtocol Protocol,
        string MessageId,
        ExternalSyncEntity EntityType,
        ExternalSyncOperation OperationType,
        string Reason,
        string Details,
        string PayloadJson,
        DateTimeOffset CreatedAt);
}

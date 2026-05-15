using IndustrySystem.Infrastructure.Communication.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;
using System.Threading.Channels;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public sealed class SignalRExternalSyncChannel : IExternalSyncChannel
{
    private readonly ExternalSyncEndpointOptions _endpoint;
    // 推送通道缓冲区，由应用服务按轮询方式批量消费。
    private readonly Channel<ExternalSyncMessage> _buffer = Channel.CreateUnbounded<ExternalSyncMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    // 防止并发 Connect/Disconnect 产生竞态。
    private readonly SemaphoreSlim _connectionGate = new(1, 1);

    private HubConnection? _connection;
    private bool _isConnected;

    public SignalRExternalSyncChannel(ExternalSyncEndpointOptions endpoint)
    {
        _endpoint = endpoint;
    }

    public string Name => _endpoint.Name;

    public ExternalSyncProtocol Protocol => ExternalSyncProtocol.SignalR;

    public int PollingIntervalSeconds => _endpoint.PollingIntervalSeconds;

    public bool IsConnected => _isConnected;

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _connectionGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_connection is not null)
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    _isConnected = true;
                    return;
                }

                await _connection.DisposeAsync().ConfigureAwait(false);
                _connection = null;
            }

            if (string.IsNullOrWhiteSpace(_endpoint.Url))
            {
                return;
            }

            var builder = new HubConnectionBuilder();
            builder.WithUrl(_endpoint.Url, options =>
            {
                if (!string.IsNullOrWhiteSpace(_endpoint.AccessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(_endpoint.AccessToken);
                }
            });

            builder.WithAutomaticReconnect(
            [
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ]);

            var connection = builder.Build();
            RegisterSubscriptions(connection);

            connection.Reconnecting += _ =>
            {
                _isConnected = false;
                return Task.CompletedTask;
            };

            connection.Reconnected += _ =>
            {
                _isConnected = true;
                return Task.CompletedTask;
            };

            connection.Closed += _ =>
            {
                _isConnected = false;
                return Task.CompletedTask;
            };

            await connection.StartAsync(ct).ConfigureAwait(false);
            _connection = connection;
            _isConnected = true;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _connectionGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            _isConnected = false;
            if (_connection is null)
            {
                return;
            }

            await _connection.StopAsync(ct).ConfigureAwait(false);
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
        }
        finally
        {
            _connectionGate.Release();
        }
    }

    public Task<IReadOnlyList<ExternalSyncMessage>> ReceiveBatchAsync(CancellationToken ct = default)
    {
        var batchSize = _endpoint.BatchSize > 0 ? _endpoint.BatchSize : 200;
        var result = new List<ExternalSyncMessage>(batchSize);

        // 保持方法非阻塞，由调用方控制调用节奏与超时。
        while (result.Count < batchSize && _buffer.Reader.TryRead(out var message))
        {
            result.Add(message);
        }

        return Task.FromResult<IReadOnlyList<ExternalSyncMessage>>(result);
    }

    public async Task AcknowledgeAsync(IReadOnlyList<ExternalSyncMessage> messages, CancellationToken ct = default)
    {
        if (!_endpoint.EnableAck || messages.Count == 0 || _connection is null || _connection.State != HubConnectionState.Connected)
        {
            return;
        }

        var ackMethod = string.IsNullOrWhiteSpace(_endpoint.AckMethodName)
            ? "AckSyncMessages"
            : _endpoint.AckMethodName;

        var payload = messages.Select(x => x.MessageId).ToArray();
        await _connection.InvokeCoreAsync(ackMethod, [payload], ct).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        _connectionGate.Dispose();
    }

    private void RegisterSubscriptions(HubConnection connection)
    {
        var messageMethod = string.IsNullOrWhiteSpace(_endpoint.MessageMethodName)
            ? "ReceiveSyncMessage"
            : _endpoint.MessageMethodName;

        // 仅保留一条订阅路径，避免不同负载形态导致重复入队。
        connection.On<JsonElement>(messageMethod, payload =>
        {
            var raw = payload.ValueKind == JsonValueKind.String
                ? payload.GetString()
                : payload.GetRawText();
            EnqueueRaw(raw);
        });
    }

    private void EnqueueRaw(string? raw)
    {
        foreach (var message in ExternalSyncMessageParser.ParseLine(raw))
        {
            _buffer.Writer.TryWrite(message);
        }
    }
}

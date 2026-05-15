using IndustrySystem.Infrastructure.Communication.Abstractions;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public sealed class SocketExternalSyncChannel : IExternalSyncChannel
{
    private readonly ExternalSyncEndpointOptions _endpoint;
    // 生产者-消费者缓冲：读循环写入，应用服务按批次消费。
    private readonly Channel<ExternalSyncMessage> _buffer = Channel.CreateUnbounded<ExternalSyncMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });
    // 串行化写入，避免 ACK 在网络流中交错。
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    private CancellationTokenSource? _readLoopCts;
    private Task? _readLoopTask;
    private TcpClient? _tcpClient;
    private NetworkStream? _networkStream;
    private bool _isConnected;

    public SocketExternalSyncChannel(ExternalSyncEndpointOptions endpoint)
    {
        _endpoint = endpoint;
    }

    public string Name => _endpoint.Name;

    public ExternalSyncProtocol Protocol => ExternalSyncProtocol.Socket;

    public int PollingIntervalSeconds => _endpoint.PollingIntervalSeconds;

    public bool IsConnected => _isConnected;

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (_readLoopTask is { IsCompleted: false })
        {
            return;
        }

        _readLoopCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        await EnsureConnectedAsync(_readLoopCts.Token).ConfigureAwait(false);
        // 在后台持续接收，避免调用线程被 Socket 读阻塞。
        _readLoopTask = Task.Run(() => ReadLoopAsync(_readLoopCts.Token), CancellationToken.None);
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        if (_readLoopCts is null)
        {
            return;
        }

        _readLoopCts.Cancel();
        DisposeConnection();

        if (_readLoopTask is not null)
        {
            try
            {
                await _readLoopTask.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // 停止流程中出现取消属于预期行为。
            }
        }

        _readLoopCts.Dispose();
        _readLoopCts = null;
        _readLoopTask = null;
    }

    public Task<IReadOnlyList<ExternalSyncMessage>> ReceiveBatchAsync(CancellationToken ct = default)
    {
        var batchSize = _endpoint.BatchSize > 0 ? _endpoint.BatchSize : 200;
        var result = new List<ExternalSyncMessage>(batchSize);

        // 非阻塞批量读取，保持轮询节奏稳定。
        while (result.Count < batchSize && _buffer.Reader.TryRead(out var message))
        {
            result.Add(message);
        }

        return Task.FromResult<IReadOnlyList<ExternalSyncMessage>>(result);
    }

    public async Task AcknowledgeAsync(IReadOnlyList<ExternalSyncMessage> messages, CancellationToken ct = default)
    {
        if (!_endpoint.EnableAck || messages.Count == 0 || _networkStream is null)
        {
            return;
        }

        var ackPayload = JsonSerializer.Serialize(new
        {
            type = "ack",
            endpoint = Name,
            messageIds = messages.Select(x => x.MessageId).ToArray(),
            ackedAt = DateTimeOffset.UtcNow
        });

        var delimiter = string.IsNullOrEmpty(_endpoint.MessageDelimiter) ? "\n" : _endpoint.MessageDelimiter;
        var bytes = Encoding.UTF8.GetBytes(ackPayload + delimiter);

        await _writeGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await _networkStream.WriteAsync(bytes, ct).ConfigureAwait(false);
            await _networkStream.FlushAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);
        _writeGate.Dispose();
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        var delimiter = string.IsNullOrEmpty(_endpoint.MessageDelimiter) ? "\n" : _endpoint.MessageDelimiter;
        var receiveBuffer = new byte[4096];
        var textBuffer = new StringBuilder();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await EnsureConnectedAsync(ct).ConfigureAwait(false);
                if (_networkStream is null)
                {
                    await DelayReconnectAsync(ct).ConfigureAwait(false);
                    continue;
                }

                var read = await _networkStream.ReadAsync(receiveBuffer.AsMemory(0, receiveBuffer.Length), ct).ConfigureAwait(false);
                if (read == 0)
                {
                    throw new IOException("Socket closed by remote endpoint.");
                }

                // TCP 是字节流，需要按分隔符切分恢复逻辑消息。
                textBuffer.Append(Encoding.UTF8.GetString(receiveBuffer, 0, read));
                DrainMessages(textBuffer, delimiter);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // 任意 IO 异常都会进入重连路径。
                DisposeConnection();
                await DelayReconnectAsync(ct).ConfigureAwait(false);
            }
        }
    }

    private void DrainMessages(StringBuilder textBuffer, string delimiter)
    {
        var payload = textBuffer.ToString();
        var index = payload.IndexOf(delimiter, StringComparison.Ordinal);

        while (index >= 0)
        {
            var line = payload[..index].Trim();
            if (!string.IsNullOrWhiteSpace(line))
            {
                foreach (var message in ExternalSyncMessageParser.ParseLine(line))
                {
                    _buffer.Writer.TryWrite(message);
                }
            }

            payload = payload[(index + delimiter.Length)..];
            index = payload.IndexOf(delimiter, StringComparison.Ordinal);
        }

        textBuffer.Clear();
        textBuffer.Append(payload);
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (_networkStream is not null && _tcpClient is { Connected: true })
        {
            _isConnected = true;
            return;
        }

        // 读循环发现断连时按需重建连接。
        var endpoint = ParseSocketEndpoint(_endpoint.Url);
        var client = new TcpClient();

        var timeoutSeconds = _endpoint.TimeoutSeconds > 0 ? _endpoint.TimeoutSeconds : 15;
        await client.ConnectAsync(endpoint.Host, endpoint.Port).WaitAsync(TimeSpan.FromSeconds(timeoutSeconds), ct).ConfigureAwait(false);

        _tcpClient = client;
        _networkStream = client.GetStream();
        _isConnected = true;
    }

    private static (string Host, int Port) ParseSocketEndpoint(string raw)
    {
        if (Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            return (uri.Host, uri.Port);
        }

        var segments = raw.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length != 2 || !int.TryParse(segments[1], out var port))
        {
            throw new InvalidOperationException($"Invalid socket endpoint: {raw}");
        }

        return (segments[0], port);
    }

    private async Task DelayReconnectAsync(CancellationToken ct)
    {
        var delaySeconds = _endpoint.PollingIntervalSeconds > 0 ? _endpoint.PollingIntervalSeconds : 2;
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct).ConfigureAwait(false);
    }

    private void DisposeConnection()
    {
        _isConnected = false;

        try
        {
            _networkStream?.Dispose();
        }
        catch
        {
            // 忽略关闭阶段异常。
        }

        try
        {
            _tcpClient?.Dispose();
        }
        catch
        {
            // 忽略关闭阶段异常。
        }

        _networkStream = null;
        _tcpClient = null;
    }
}

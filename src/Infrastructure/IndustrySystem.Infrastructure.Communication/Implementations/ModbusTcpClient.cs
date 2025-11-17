using System.Net.Sockets;
using IndustrySystem.Infrastructure.Communication.Abstractions;
using Modbus.Device;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class ModbusTcpClient : IModbusTcpClient
{
    private string? _host;
    private int _port;
    private byte _unitId = 1;
    private int _connectTimeoutMs = 3000;
    private int _receiveTimeoutMs = 3000;
    private int _sendTimeoutMs = 3000;

    private TcpClient? _tcpClient;
    private ModbusIpMaster? _master;

    public void Configure(string host, int port, byte unitId = 1, int connectTimeoutMs = 3000, int receiveTimeoutMs = 3000, int sendTimeoutMs = 3000)
    {
        _host = host;
        _port = port;
        _unitId = unitId;
        _connectTimeoutMs = connectTimeoutMs;
        _receiveTimeoutMs = receiveTimeoutMs;
        _sendTimeoutMs = sendTimeoutMs;
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_host) || _port <= 0)
            throw new InvalidOperationException("Modbus 参数未配置，请先调用 Configure");

        if (_tcpClient != null)
            await DisconnectAsync(ct).ConfigureAwait(false);

        var tcp = new TcpClient()
        {
            ReceiveTimeout = _receiveTimeoutMs,
            SendTimeout = _sendTimeoutMs,
            NoDelay = true
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(_connectTimeoutMs);
        try
        {
            await tcp.ConnectAsync(_host!, _port, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            tcp.Dispose();
            throw new TimeoutException($"连接 Modbus TCP({_host}:{_port}) 超时({_connectTimeoutMs}ms)");
        }

        _tcpClient = tcp;
    _master = ModbusIpMaster.CreateIp(_tcpClient);
    }

    public Task DisconnectAsync(CancellationToken ct = default)
    {
        _master?.Dispose();
        _master = null;
        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpClient = null;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _master?.Dispose();
        _tcpClient?.Dispose();
        _master = null;
        _tcpClient = null;
        return ValueTask.CompletedTask;
    }

    public async Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfPoints, CancellationToken ct = default)
    {
        if (_master is null)
            throw new InvalidOperationException("Modbus 未连接，请先调用 ConnectAsync");

        // 使用 NModbus4 的异步 API
        using var _ = ct.Register(() => { try { _tcpClient?.Close(); } catch { } });
        return await _master.ReadHoldingRegistersAsync(startAddress, numberOfPoints).ConfigureAwait(false);
    }

    public async Task WriteSingleRegisterAsync(ushort registerAddress, ushort value, CancellationToken ct = default)
    {
        if (_master is null)
            throw new InvalidOperationException("Modbus 未连接，请先调用 ConnectAsync");
        using var _ = ct.Register(() => { try { _tcpClient?.Close(); } catch { } });
        await _master.WriteSingleRegisterAsync(registerAddress, value).ConfigureAwait(false);
    }

    public async Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] data, CancellationToken ct = default)
    {
        if (_master is null)
            throw new InvalidOperationException("Modbus 未连接，请先调用 ConnectAsync");
        using var _ = ct.Register(() => { try { _tcpClient?.Close(); } catch { } });
        await _master.WriteMultipleRegistersAsync(startAddress, data).ConfigureAwait(false);
    }
}

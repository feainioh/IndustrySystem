using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Application.Services;

public class CommunicationAppService : ICommunicationAppService
{
    private readonly Func<IModbusTcpClient> _modbusFactory;
    private IModbusTcpClient? _modbus;
    private string? _host;
    private int _port;

    public CommunicationAppService(Func<IModbusTcpClient> modbusFactory)
    {
        _modbusFactory = modbusFactory;
    }

    public async Task ConnectModbusAsync(string host, int port, CancellationToken ct = default)
    {
        _host = host; _port = port;
        _modbus = _modbusFactory();
        _modbus.Configure(host, port);
        await _modbus.ConnectAsync(ct);
    }

    public async Task DisconnectModbusAsync(CancellationToken ct = default)
    {
        if (_modbus != null)
        {
            await _modbus.DisconnectAsync(ct);
            await _modbus.DisposeAsync();
            _modbus = null;
        }
    }

    public Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfPoints, CancellationToken ct = default)
    {
        if (_modbus == null) throw new InvalidOperationException("Modbus 未连接");
        return _modbus.ReadHoldingRegistersAsync(startAddress, numberOfPoints, ct);
    }

    public Task WriteSingleRegisterAsync(ushort registerAddress, ushort value, CancellationToken ct = default)
    {
        if (_modbus == null) throw new InvalidOperationException("Modbus 未连接");
        return _modbus.WriteSingleRegisterAsync(registerAddress, value, ct);
    }

    public Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] data, CancellationToken ct = default)
    {
        if (_modbus == null) throw new InvalidOperationException("Modbus 未连接");
        return _modbus.WriteMultipleRegistersAsync(startAddress, data, ct);
    }
}

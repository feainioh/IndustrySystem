using System;
using System.Threading;
using System.Threading.Tasks;

namespace IndustrySystem.Application.Contracts.Services;

public interface ICommunicationAppService
{
    Task ConnectModbusAsync(string host, int port, CancellationToken ct = default);
    Task DisconnectModbusAsync(CancellationToken ct = default);
    Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfPoints, CancellationToken ct = default);
    Task WriteSingleRegisterAsync(ushort registerAddress, ushort value, CancellationToken ct = default);
    Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] data, CancellationToken ct = default);
}

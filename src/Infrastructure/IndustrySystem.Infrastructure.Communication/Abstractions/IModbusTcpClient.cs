namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface IModbusTcpClient : ICommunicationClient
{
    /// <summary>
    /// 配置连接参数。
    /// </summary>
    /// <param name="host">目标主机</param>
    /// <param name="port">端口</param>
    /// <param name="unitId">Modbus 单元标识(Unit Id)，默认为 1</param>
    /// <param name="connectTimeoutMs">连接超时时间(ms)</param>
    /// <param name="receiveTimeoutMs">接收超时时间(ms)</param>
    /// <param name="sendTimeoutMs">发送超时时间(ms)</param>
    void Configure(string host, int port, byte unitId = 1, int connectTimeoutMs = 3000, int receiveTimeoutMs = 3000, int sendTimeoutMs = 3000);

    Task<ushort[]> ReadHoldingRegistersAsync(ushort startAddress, ushort numberOfPoints, CancellationToken ct = default);

    Task WriteSingleRegisterAsync(ushort registerAddress, ushort value, CancellationToken ct = default);
    Task WriteMultipleRegistersAsync(ushort startAddress, ushort[] data, CancellationToken ct = default);
}

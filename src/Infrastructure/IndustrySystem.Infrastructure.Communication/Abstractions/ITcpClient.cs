namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface ITcpClient : ICommunicationClient
{
    Task SendAsync(byte[] payload, CancellationToken ct = default);
    Task<byte[]> ReceiveAsync(int maxBytes, CancellationToken ct = default);
}

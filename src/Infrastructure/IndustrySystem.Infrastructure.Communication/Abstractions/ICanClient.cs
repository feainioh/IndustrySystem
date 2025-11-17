namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface ICanClient : ICommunicationClient
{
    Task SendFrameAsync(byte[] data, CancellationToken ct = default);
}

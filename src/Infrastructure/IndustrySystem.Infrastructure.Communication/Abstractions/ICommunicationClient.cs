namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface ICommunicationClient : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
}

using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class EthercatClient : IEthercatClient
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;
}

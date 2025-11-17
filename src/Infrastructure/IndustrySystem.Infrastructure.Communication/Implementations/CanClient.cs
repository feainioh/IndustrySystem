using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class CanClient : ICanClient
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task SendFrameAsync(byte[] data, CancellationToken ct = default) => Task.CompletedTask;
}

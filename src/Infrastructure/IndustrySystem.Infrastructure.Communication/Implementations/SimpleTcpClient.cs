using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class SimpleTcpClient : ITcpClient
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task SendAsync(byte[] payload, CancellationToken ct = default) => Task.CompletedTask;
    public Task<byte[]> ReceiveAsync(int maxBytes, CancellationToken ct = default) => Task.FromResult(Array.Empty<byte>());
}

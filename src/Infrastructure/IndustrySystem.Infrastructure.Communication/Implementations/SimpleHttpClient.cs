using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class SimpleHttpClient : IHttpClient
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task<string> GetAsync(string url, CancellationToken ct = default) => Task.FromResult("ok");
}

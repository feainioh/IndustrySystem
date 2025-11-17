namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface IHttpClient : ICommunicationClient
{
    Task<string> GetAsync(string url, CancellationToken ct = default);
}

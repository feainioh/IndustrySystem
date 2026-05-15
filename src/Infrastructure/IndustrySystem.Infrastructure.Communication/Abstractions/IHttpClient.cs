namespace IndustrySystem.Infrastructure.Communication.Abstractions;

public interface IHttpClient : ICommunicationClient
{
    Task<string> GetAsync(
        string url,
        IReadOnlyDictionary<string, string>? headers = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default);

    Task<string> PostAsync(
        string url,
        string body,
        string contentType = "application/json",
        IReadOnlyDictionary<string, string>? headers = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default);
}

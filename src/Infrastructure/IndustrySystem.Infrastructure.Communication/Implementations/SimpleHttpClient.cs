using IndustrySystem.Infrastructure.Communication.Abstractions;
using System.Net.Http.Headers;
using System.Text;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public class SimpleHttpClient : IHttpClient
{
    private readonly System.Net.Http.HttpClient _client = new();

    public ValueTask DisposeAsync()
    {
        _client.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task ConnectAsync(CancellationToken ct = default) => Task.CompletedTask;
    public Task DisconnectAsync(CancellationToken ct = default) => Task.CompletedTask;

    public async Task<string> GetAsync(
        string url,
        IReadOnlyDictionary<string, string>? headers = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(request, headers);
        var requestTimeout = timeout ?? TimeSpan.FromSeconds(100);
        using var timeoutCts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : null;
        if (timeoutCts is not null)
        {
            timeoutCts.CancelAfter(requestTimeout);
        }

        var requestToken = timeoutCts?.Token ?? ct;
        using var response = await _client.SendAsync(request, requestToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(requestToken).ConfigureAwait(false);
    }

    public async Task<string> PostAsync(
        string url,
        string body,
        string contentType = "application/json",
        IReadOnlyDictionary<string, string>? headers = null,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body ?? string.Empty, Encoding.UTF8, contentType)
        };
        ApplyHeaders(request, headers);
        var requestTimeout = timeout ?? TimeSpan.FromSeconds(100);

        using var timeoutCts = timeout.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : null;
        if (timeoutCts is not null)
        {
            timeoutCts.CancelAfter(requestTimeout);
        }

        var requestToken = timeoutCts?.Token ?? ct;
        using var response = await _client.SendAsync(request, requestToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(requestToken).ConfigureAwait(false);
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return;
        }

        foreach (var pair in headers)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            if (pair.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(pair.Value);
                continue;
            }

            request.Headers.Remove(pair.Key);
            request.Headers.TryAddWithoutValidation(pair.Key, pair.Value);
        }
    }
}

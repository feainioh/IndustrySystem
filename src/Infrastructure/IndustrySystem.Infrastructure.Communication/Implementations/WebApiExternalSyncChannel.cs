using IndustrySystem.Infrastructure.Communication.Abstractions;
using System.Text.Json;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

public sealed class WebApiExternalSyncChannel : IExternalSyncChannel
{
    private readonly IHttpClient _httpClient;
    private readonly ExternalSyncEndpointOptions _endpoint;
    private bool _isConnected;

    public WebApiExternalSyncChannel(IHttpClient httpClient, ExternalSyncEndpointOptions endpoint)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
    }

    public string Name => _endpoint.Name;

    public ExternalSyncProtocol Protocol => ExternalSyncProtocol.WebApi;

    public int PollingIntervalSeconds => _endpoint.PollingIntervalSeconds;

    public bool IsConnected => _isConnected;

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        await _httpClient.ConnectAsync(ct).ConfigureAwait(false);
        _isConnected = true;
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        _isConnected = false;
        await _httpClient.DisconnectAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ExternalSyncMessage>> ReceiveBatchAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_endpoint.Url))
        {
            return [];
        }

        // 拉取模式：每个周期一次 HTTP 请求，并应用端点鉴权与超时策略。
        var responseJson = await _httpClient.GetAsync(
            BuildBatchUrl(_endpoint.Url, _endpoint.BatchSize),
            BuildAuthHeaders(),
            ResolveTimeout(),
            ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(responseJson))
        {
            return [];
        }

        return ExternalSyncMessageParser.ParseBatch(responseJson);
    }

    public async Task AcknowledgeAsync(IReadOnlyList<ExternalSyncMessage> messages, CancellationToken ct = default)
    {
        if (!_endpoint.EnableAck || messages.Count == 0 || string.IsNullOrWhiteSpace(_endpoint.AckUrl))
        {
            return;
        }

        // ACK 仅回传标识信息，业务状态仍由上游系统维护。
        var payload = JsonSerializer.Serialize(new
        {
            endpoint = Name,
            acknowledgedAt = DateTimeOffset.UtcNow,
            messageIds = messages.Select(x => x.MessageId).ToArray()
        });

        await _httpClient.PostAsync(
            _endpoint.AckUrl,
            payload,
            headers: BuildAuthHeaders(),
            timeout: ResolveTimeout(),
            ct: ct).ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => _httpClient.DisposeAsync();

    private TimeSpan ResolveTimeout()
    {
        var timeoutSeconds = _endpoint.TimeoutSeconds > 0 ? _endpoint.TimeoutSeconds : 15;
        return TimeSpan.FromSeconds(timeoutSeconds);
    }

    private IReadOnlyDictionary<string, string> BuildAuthHeaders()
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_endpoint.ApiKey))
        {
            // 允许自定义 API Key 头名称，兼容外部网关约定。
            var apiKeyHeader = string.IsNullOrWhiteSpace(_endpoint.ApiKeyHeaderName)
                ? "X-Api-Key"
                : _endpoint.ApiKeyHeaderName;
            headers[apiKeyHeader] = _endpoint.ApiKey;
        }

        if (!string.IsNullOrWhiteSpace(_endpoint.AccessToken))
        {
            var token = _endpoint.AccessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? _endpoint.AccessToken
                : $"Bearer {_endpoint.AccessToken}";
            headers["Authorization"] = token;
        }

        return headers;
    }

    private static string BuildBatchUrl(string rawUrl, int batchSize)
    {
        if (batchSize <= 0)
        {
            return rawUrl;
        }

        var separator = rawUrl.Contains('?', StringComparison.Ordinal) ? "&" : "?";
        if (rawUrl.Contains("batchSize=", StringComparison.OrdinalIgnoreCase))
        {
            return rawUrl;
        }

        // 以非破坏方式追加参数，保持现有查询字符串不变。
        return $"{rawUrl}{separator}batchSize={batchSize}";
    }
}

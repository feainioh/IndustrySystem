using System.Text.Json;
using IndustrySystem.Infrastructure.Communication.Abstractions;

namespace IndustrySystem.Infrastructure.Communication.Implementations;

internal static class ExternalSyncMessageParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<ExternalSyncMessage> ParseBatch(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return [];
        }

        try
        {
            var json = rawJson.Trim();

            // 格式 A：纯数组 [{...},{...}]
            if (json.StartsWith("[", StringComparison.Ordinal))
            {
                var payloads = JsonSerializer.Deserialize<List<SyncPayload>>(json, JsonOptions);
                return payloads?.Select(Map).ToList() ?? [];
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 格式 B：包装对象 { messages: [...] }
            if (root.ValueKind == JsonValueKind.Object
                && TryGetPropertyIgnoreCase(root, "messages", out var messages)
                && messages.ValueKind == JsonValueKind.Array)
            {
                var payloads = JsonSerializer.Deserialize<List<SyncPayload>>(messages.GetRawText(), JsonOptions);
                return payloads?.Select(Map).ToList() ?? [];
            }

            // 格式 C：单对象消息
            if (root.ValueKind == JsonValueKind.Object)
            {
                var payload = JsonSerializer.Deserialize<SyncPayload>(root.GetRawText(), JsonOptions);
                return payload is null ? [] : [Map(payload)];
            }
        }
        catch
        {
            // 忽略非法负载；调用方可在更高层决定是否写入死信。
        }

        return [];
    }

    public static IReadOnlyList<ExternalSyncMessage> ParseLine(string? rawLine) => ParseBatch(rawLine);

    private static ExternalSyncMessage Map(SyncPayload payload)
    {
        // 源消息标识缺失时生成兜底 ID，避免中断处理流水线。
        var messageId = string.IsNullOrWhiteSpace(payload.MessageId)
            ? Guid.NewGuid().ToString("N")
            : payload.MessageId!;

        var entity = ParseEnum(payload.EntityType, ExternalSyncEntity.Material);
        var operation = ParseEnum(payload.OperationType, ExternalSyncOperation.Upsert);
        var occurredAt = payload.OccurredAt ?? DateTimeOffset.UtcNow;

        var payloadJson = payload.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            ? "{}"
            : payload.Payload.GetRawText();

        return new ExternalSyncMessage(messageId, entity, operation, occurredAt, payloadJson);
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed)
            ? parsed
            : fallback;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (property.NameEquals(propertyName) || property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private sealed class SyncPayload
    {
        public string? MessageId { get; init; }
        public string? EntityType { get; init; }
        public string? OperationType { get; init; }
        public DateTimeOffset? OccurredAt { get; init; }
        public JsonElement Payload { get; init; }
    }
}

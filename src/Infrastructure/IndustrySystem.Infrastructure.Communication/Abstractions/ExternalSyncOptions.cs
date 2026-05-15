namespace IndustrySystem.Infrastructure.Communication.Abstractions;

/// <summary>
/// 外部同步传输协议类型。
/// </summary>
public enum ExternalSyncProtocol
{
    /// <summary>
    /// 基于 HTTP 的轮询拉取模式。
    /// </summary>
    WebApi,

    /// <summary>
    /// 基于 TCP 原始流的推送模式。
    /// </summary>
    Socket,

    /// <summary>
    /// 基于 SignalR Hub 的推送模式。
    /// </summary>
    SignalR
}

/// <summary>
/// 外部同步实体类型。
/// </summary>
public enum ExternalSyncEntity
{
    /// <summary>
    /// 物料实体。
    /// </summary>
    Material,

    /// <summary>
    /// 货架实体。
    /// </summary>
    Shelf,

    /// <summary>
    /// 实验实体。
    /// </summary>
    Experiment
}

/// <summary>
/// 外部同步操作类型。
/// </summary>
public enum ExternalSyncOperation
{
    /// <summary>
    /// 新增或更新。
    /// </summary>
    Upsert,

    /// <summary>
    /// 删除。
    /// </summary>
    Delete,

    /// <summary>
    /// 全量快照。
    /// </summary>
    Snapshot
}

/// <summary>
/// 同步冲突处理策略。
/// </summary>
public enum ExternalSyncConflictPolicy
{
    /// <summary>
    /// 始终应用外部数据。
    /// </summary>
    ExternalWins,

    /// <summary>
    /// 保留本地数据并忽略外部 Upsert。
    /// </summary>
    LocalWins,

    /// <summary>
    /// 比较版本或时间戳，应用较新数据。
    /// </summary>
    NewerWins
}

/// <summary>
/// 外部同步标准消息。
/// </summary>
/// <param name="MessageId">消息唯一标识。</param>
/// <param name="EntityType">实体类型。</param>
/// <param name="OperationType">操作类型。</param>
/// <param name="OccurredAt">消息发生时间。</param>
/// <param name="PayloadJson">消息负载 JSON。</param>
public sealed record ExternalSyncMessage(
    string MessageId,
    ExternalSyncEntity EntityType,
    ExternalSyncOperation OperationType,
    DateTimeOffset OccurredAt,
    string PayloadJson);

/// <summary>
/// 单个同步端点配置。
/// </summary>
public sealed class ExternalSyncEndpointOptions
{
    /// <summary>
    /// 端点名称，用于日志与去重键。
    /// </summary>
    public string Name { get; set; } = "default";

    /// <summary>
    /// 是否启用该端点。
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 端点传输协议。
    /// </summary>
    public ExternalSyncProtocol Protocol { get; set; } = ExternalSyncProtocol.WebApi;

    /// <summary>
    /// 端点地址（HTTP/TCP/SignalR）。
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// 轮询间隔秒数；在重连场景下也可作为退避基础。
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// 请求或连接超时秒数。
    /// </summary>
    public int TimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// 每个同步周期最多处理的消息数。
    /// </summary>
    public int BatchSize { get; set; } = 200;

    /// <summary>
    /// API Key 值。
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// 访问令牌（可为原始 token 或 Bearer token）。
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// API Key 请求头名称。
    /// </summary>
    public string? ApiKeyHeaderName { get; set; }

    /// <summary>
    /// WebApi ACK 回调地址。
    /// </summary>
    public string? AckUrl { get; set; }

    /// <summary>
    /// SignalR ACK 方法名。
    /// </summary>
    public string? AckMethodName { get; set; }

    /// <summary>
    /// SignalR 消息推送方法名。
    /// </summary>
    public string? MessageMethodName { get; set; }

    /// <summary>
    /// Socket 消息分帧分隔符。
    /// </summary>
    public string MessageDelimiter { get; set; } = "\n";

    /// <summary>
    /// 是否启用 ACK。
    /// </summary>
    public bool EnableAck { get; set; } = false;

    /// <summary>
    /// 端点级重试策略；为空时使用全局重试策略。
    /// </summary>
    public ExternalSyncRetryOptions? Retry { get; set; }
}

/// <summary>
/// 外部同步全局配置。
/// </summary>
public sealed class ExternalSyncOptions
{
    /// <summary>
    /// 是否启用外部同步。
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 默认轮询间隔秒数。
    /// </summary>
    public int DefaultPollingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// 默认超时秒数。
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 15;

    /// <summary>
    /// 是否启用结构化诊断日志（Trace）。
    /// </summary>
    public bool EnableStructuredLog { get; set; } = true;

    /// <summary>
    /// 预留的并行度参数；当前同步循环默认串行。
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 1;

    /// <summary>
    /// 全局重试配置。
    /// </summary>
    public ExternalSyncRetryOptions Retry { get; set; } = new();

    /// <summary>
    /// 去重配置。
    /// </summary>
    public ExternalSyncDedupOptions Dedup { get; set; } = new();

    /// <summary>
    /// 冲突处理配置。
    /// </summary>
    public ExternalSyncConflictOptions Conflict { get; set; } = new();

    /// <summary>
    /// 死信配置。
    /// </summary>
    public ExternalSyncDeadLetterOptions DeadLetter { get; set; } = new();

    /// <summary>
    /// 同步端点集合。
    /// </summary>
    public List<ExternalSyncEndpointOptions> Endpoints { get; set; } = [];
}

/// <summary>
/// 重试策略配置。
/// </summary>
public sealed class ExternalSyncRetryOptions
{
    /// <summary>
    /// 最大尝试次数（首次执行 + 重试）。
    /// </summary>
    public int MaxAttempts { get; set; } = 3;

    /// <summary>
    /// 基础退避延迟（毫秒）。
    /// </summary>
    public int BaseDelayMilliseconds { get; set; } = 250;

    /// <summary>
    /// 最大退避延迟（毫秒）。
    /// </summary>
    public int MaxDelayMilliseconds { get; set; } = 5000;

    /// <summary>
    /// 是否启用抖动，降低重试风暴概率。
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// 幂等去重配置。
/// </summary>
public sealed class ExternalSyncDedupOptions
{
    /// <summary>
    /// 是否启用去重。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// endpoint::messageId 去重滑动窗口（分钟）。
    /// </summary>
    public int WindowMinutes { get; set; } = 1440;

    /// <summary>
    /// 去重表最大条目数，用于限制内存占用。
    /// </summary>
    public int MaxEntries { get; set; } = 100000;
}

/// <summary>
/// 冲突判定配置。
/// </summary>
public sealed class ExternalSyncConflictOptions
{
    /// <summary>
    /// 冲突策略。
    /// </summary>
    public ExternalSyncConflictPolicy Policy { get; set; } = ExternalSyncConflictPolicy.NewerWins;

    /// <summary>
    /// NewerWins 策略使用的时间戳字段名。
    /// </summary>
    public string TimestampFieldName { get; set; } = "updatedAt";

    /// <summary>
    /// NewerWins 策略使用的版本字段名。
    /// </summary>
    public string VersionFieldName { get; set; } = "version";
}

/// <summary>
/// 死信处理配置。
/// </summary>
public sealed class ExternalSyncDeadLetterOptions
{
    /// <summary>
    /// 是否启用死信。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 内存中保留的最近死信数量。
    /// </summary>
    public int MaxInMemoryItems { get; set; } = 1000;

    /// <summary>
    /// 死信输出文件路径（jsonl）；相对路径将解析到应用基目录。
    /// </summary>
    public string? FilePath { get; set; } = "logs/external-sync-deadletter.jsonl";
}

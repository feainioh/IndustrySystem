namespace IndustrySystem.Application.Contracts.Dtos;

public enum ExternalSyncEntityType
{
    Material,
    Shelf,
    Experiment
}

public enum ExternalSyncOperationType
{
    Upsert,
    Delete,
    Snapshot
}

public sealed record ExternalSyncMessageDto(
    string MessageId,
    ExternalSyncEntityType EntityType,
    ExternalSyncOperationType OperationType,
    DateTimeOffset OccurredAt,
    string PayloadJson);

public sealed record ExternalSyncRuntimeStatusDto(
    bool Enabled,
    bool IsRunning,
    int EndpointCount,
    long TotalProcessedCount,
    long TotalFailedCount,
    long TotalRetriedCount,
    long DeadLetterCount,
    long LastBatchLatencyMilliseconds,
    DateTimeOffset? LastSyncedAt,
    string? LastError);

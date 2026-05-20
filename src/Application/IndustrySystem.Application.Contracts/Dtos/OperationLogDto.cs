namespace IndustrySystem.Application.Contracts.Dtos;

public record OperationLogDto(
    Guid Id,
    DateTime Timestamp,
    string Level,
    string OperationType,
    string Operator,
    string Description,
    string IPAddress,
    string Logger,
    long ElapsedMs,
    bool IsSuccess,
    string? ErrorMessage
);

public record CreateOperationLogDto(
    string Level,
    string OperationType,
    string Operator,
    string Description,
    string IPAddress,
    string Logger,
    long ElapsedMs,
    bool IsSuccess,
    string? ErrorMessage
);

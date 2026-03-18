using System;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Contracts.Dtos;

public record ExperimentSummaryDto(Guid Id, string Name, string Status);
public record ExperimentHistoryDto(DateTime Time, string Name, string Result);
public record AlarmDto(Guid Id, string Message, DateTime Time, bool Acknowledged);

public record ExperimentOptionDto(Guid Id, string Name);

public record ExperimentParameterOptionDto(Guid Id, string Name, ExperimentType Type);

public record ExperimentParameterItemDto
{
    public Guid Id { get; init; }
    public ExperimentType Type { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    // === 反应 (Reaction) ===
    public string? RawMaterial { get; init; }
    public string? StockSolution { get; init; }
    public decimal? TemperatureC { get; init; }
    public decimal? PressureKpa { get; init; }
    public int? DurationMinutes { get; init; }
    public int? StirSpeedRpm { get; init; }
    public decimal? LiquidAddSpeedMlMin { get; init; }
    public decimal? PowderAddSpeedGMin { get; init; }

    // === 旋蒸 (RotaryEvaporation) ===
    public decimal? BathTemperatureC { get; init; }
    public decimal? VaporTemperatureC { get; init; }
    public decimal? VacuumKpa { get; init; }
    public int? RotationRpm { get; init; }
    public decimal? LiftStrokeMm { get; init; }
    public decimal? CoolantTemperatureC { get; init; }
    public bool? CollectCondensate { get; init; }
    public bool? ContinuousFeed { get; init; }

    // === 检测 (Detection) / 自定义检测 ===
    public string? Method { get; init; }
    public int? WavelengthNm { get; init; }
    public string? Notes { get; init; }
    public string? ParameterJson { get; init; }

    // === 过滤 (Filtration) / 取样 / 萃取 / 淬灭 / 反应 共用 ===
    public string? Detergent { get; init; }
    public decimal? DetergentVolumeMl { get; init; }

    // === 干燥 (Drying) ===
    public Guid? DesiccantId { get; init; }
    public decimal? DesiccantVolumeMl { get; init; }
    public int? ShakeSpeedRpm { get; init; }
    public int? ShakeDurationMinutes { get; init; }

    // === 淬灭 (Quenching) ===
    public string? QuenchingAgent { get; init; }
    public decimal? QuenchingAgentVolumeMl { get; init; }
    public decimal? QuenchingAgentDripSpeedMlMin { get; init; }
    public bool? AddQuenchingAgentFirst { get; init; }
    public decimal? PreTemperatureC { get; init; }
    public decimal? MaxTemperatureC { get; init; }
    public decimal? TotalProductVolumeMl { get; init; }

    // === 萃取 (Extraction) ===
    public string? ExtractAgent { get; init; }
    public decimal? ExtractAgentVolumeMl { get; init; }
    public int? StirDurationMinutes { get; init; }
    public int? SettlingMinutes { get; init; }

    // === 取样 (Sampling) ===
    public decimal? SampleVolumeMl { get; init; }

    // === 离心 (Centrifugation) ===
    public int? SpeedRpm { get; init; }
}

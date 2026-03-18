using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Domain.Entities.Experiments;

/// <summary>反应实验参数</summary>
public class ReactionParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? RawMaterial { get; set; }
    public string? StockSolution { get; set; }
    public decimal? TemperatureC { get; set; }
    public decimal? PressureKpa { get; set; }
    public int? DurationMinutes { get; set; }
    public int? StirSpeedRpm { get; set; }
    public decimal? LiquidAddSpeedMlMin { get; set; }
    public decimal? PowderAddSpeedGMin { get; set; }
    public string? Detergent { get; set; }
    public decimal? DetergentVolumeMl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>旋蒸参数</summary>
public class RotaryEvaporationParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal? BathTemperatureC { get; set; }
    public decimal? VaporTemperatureC { get; set; }
    public decimal? VacuumKpa { get; set; }
    public int? RotationRpm { get; set; }
    public decimal? LiftStrokeMm { get; set; }
    public decimal? CoolantTemperatureC { get; set; }
    public bool CollectCondensate { get; set; }
    public bool ContinuousFeed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>检测参数</summary>
public class DetectionParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Method { get; set; }
    public int? WavelengthNm { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>过滤参数</summary>
public class FiltrationParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public string? Detergent { get; set; }
    public decimal? DetergentVolumeMl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>干燥参数</summary>
public class DryingParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid? DesiccantId { get; set; }
    public decimal? DesiccantVolumeMl { get; set; }
    public int? ShakeSpeedRpm { get; set; }
    public int? ShakeDurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>淬灭参数</summary>
public class QuenchingParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? QuenchingAgent { get; set; }
    public decimal? QuenchingAgentVolumeMl { get; set; }
    public decimal? QuenchingAgentDripSpeedMlMin { get; set; }
    public bool AddQuenchingAgentFirst { get; set; }
    public decimal? PreTemperatureC { get; set; }
    public decimal? MaxTemperatureC { get; set; }
    public int? StirSpeedRpm { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Detergent { get; set; }
    public decimal? DetergentVolumeMl { get; set; }
    public decimal? TotalProductVolumeMl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>萃取参数</summary>
public class ExtractionParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? ExtractAgent { get; set; }
    public decimal? ExtractAgentVolumeMl { get; set; }
    public int? StirSpeedRpm { get; set; }
    public int? StirDurationMinutes { get; set; }
    public int? SettlingMinutes { get; set; }
    public string? Detergent { get; set; }
    public decimal? DetergentVolumeMl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>取样参数</summary>
public class SamplingParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal? SampleVolumeMl { get; set; }
    public string? Detergent { get; set; }
    public decimal? DetergentVolumeMl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>离心参数</summary>
public class CentrifugationParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int? SpeedRpm { get; set; }
    public decimal? TemperatureC { get; set; }
    public int? DurationMinutes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>自定义检测参数</summary>
public class CustomDetectionParameter
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Method { get; set; }
    public string? ParameterJson { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

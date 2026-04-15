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
    /// <summary>主键ID</summary>
    public Guid Id { get; init; }
    /// <summary>实验类型</summary>
    public ExperimentType Type { get; init; }
    /// <summary>参数名称</summary>
    public string Name { get; init; } = string.Empty;
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; init; }
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; init; }

    // === 反应 (Reaction) ===
    /// <summary>原料名称</summary>
    public string RawMaterial { get; init; } = string.Empty;
    /// <summary>母液名称</summary>
    public string StockSolution { get; init; } = string.Empty;
    /// <summary>反应温度(℃)</summary>
    public decimal TemperatureC { get; init; }
    /// <summary>反应压力(kPa)</summary>
    public decimal PressureKpa { get; init; }
    /// <summary>时长(分钟)</summary>
    public int DurationMinutes { get; init; }
    /// <summary>搅拌速度(rpm)</summary>
    public int StirSpeedRpm { get; init; }
    /// <summary>加液速度(mL/min)</summary>
    public decimal LiquidAddSpeedMlMin { get; init; }
    /// <summary>加粉速度(g/min)</summary>
    public decimal PowderAddSpeedGMin { get; init; }

    // === 旋蒸 (RotaryEvaporation) ===
    /// <summary>水浴温度(℃)</summary>
    public decimal BathTemperatureC { get; init; }
    /// <summary>蒸汽温度(℃)</summary>
    public decimal VaporTemperatureC { get; init; }
    /// <summary>真空度(kPa)</summary>
    public decimal VacuumKpa { get; init; }
    /// <summary>旋转速度(rpm)</summary>
    public int RotationRpm { get; init; }
    /// <summary>升降行程(mm)</summary>
    public decimal LiftStrokeMm { get; init; }
    /// <summary>冷却液温度(℃)</summary>
    public decimal CoolantTemperatureC { get; init; }
    /// <summary>是否收集冷凝液</summary>
    public bool CollectCondensate { get; init; }
    /// <summary>是否连续进料</summary>
    public bool ContinuousFeed { get; init; }

    // === 检测 (Detection) / 自定义检测 ===
    /// <summary>检测方法</summary>
    public string Method { get; init; } = string.Empty;
    /// <summary>检测波长(nm)</summary>
    public int WavelengthNm { get; init; }
    /// <summary>备注</summary>
    public string Notes { get; init; } = string.Empty;
    /// <summary>自定义参数JSON</summary>
    public string ParameterJson { get; init; } = string.Empty;

    // === 过滤 (Filtration) / 取样 / 萃取 / 淬灭 / 反应 共用 ===
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; init; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; init; }

    // === 干燥 (Drying) ===
    /// <summary>干燥剂物料ID</summary>
    public Guid? DesiccantId { get; init; }
    /// <summary>干燥剂用量(mL)</summary>
    public decimal DesiccantVolumeMl { get; init; }
    /// <summary>摇晃速度(rpm)</summary>
    public int ShakeSpeedRpm { get; init; }
    /// <summary>摇晃时长(分钟)</summary>
    public int ShakeDurationMinutes { get; init; }

    // === 淬灭 (Quenching) ===
    /// <summary>淬灭剂名称</summary>
    public string QuenchingAgent { get; init; } = string.Empty;
    /// <summary>淬灭剂用量(mL)</summary>
    public decimal QuenchingAgentVolumeMl { get; init; }
    /// <summary>淬灭剂滴加速度(mL/min)</summary>
    public decimal QuenchingAgentDripSpeedMlMin { get; init; }
    /// <summary>是否先加淬灭剂</summary>
    public bool AddQuenchingAgentFirst { get; init; }
    /// <summary>预热温度(℃)</summary>
    public decimal PreTemperatureC { get; init; }
    /// <summary>最高温度(℃)</summary>
    public decimal MaxTemperatureC { get; init; }
    /// <summary>产品总量(mL)</summary>
    public decimal TotalProductVolumeMl { get; init; }

    // === 萃取 (Extraction) ===
    /// <summary>萃取剂名称</summary>
    public string ExtractAgent { get; init; } = string.Empty;
    /// <summary>萃取剂用量(mL)</summary>
    public decimal ExtractAgentVolumeMl { get; init; }
    /// <summary>搅拌时长(分钟)</summary>
    public int StirDurationMinutes { get; init; }
    /// <summary>静置时间(分钟)</summary>
    public int SettlingMinutes { get; init; }

    // === 取样 (Sampling) ===
    /// <summary>取样量(mL)</summary>
    public decimal SampleVolumeMl { get; init; }

    // === 离心 (Centrifugation) ===
    /// <summary>离心速度(rpm)</summary>
    public int SpeedRpm { get; init; }
}

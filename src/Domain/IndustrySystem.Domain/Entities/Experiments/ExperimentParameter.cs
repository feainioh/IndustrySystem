using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Domain.Entities.Experiments;

/// <summary>反应实验参数</summary>
public class ReactionParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>原料名称</summary>
    public string RawMaterial { get; set; } = string.Empty;
    /// <summary>母液名称</summary>
    public string StockSolution { get; set; } = string.Empty;
    /// <summary>反应温度(℃)</summary>
    public decimal TemperatureC { get; set; }
    /// <summary>反应压力(kPa)</summary>
    public decimal PressureKpa { get; set; }
    /// <summary>反应时长(分钟)</summary>
    public int DurationMinutes { get; set; }
    /// <summary>搅拌速度(rpm)</summary>
    public int StirSpeedRpm { get; set; }
    /// <summary>加液速度(mL/min)</summary>
    public decimal LiquidAddSpeedMlMin { get; set; }
    /// <summary>加粉速度(g/min)</summary>
    public decimal PowderAddSpeedGMin { get; set; }
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; set; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>旋蒸参数</summary>
public class RotaryEvaporationParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>水浴温度(℃)</summary>
    public decimal BathTemperatureC { get; set; }
    /// <summary>蒸汽温度(℃)</summary>
    public decimal VaporTemperatureC { get; set; }
    /// <summary>真空度(kPa)</summary>
    public decimal VacuumKpa { get; set; }
    /// <summary>旋转速度(rpm)</summary>
    public int RotationRpm { get; set; }
    /// <summary>升降行程(mm)</summary>
    public decimal LiftStrokeMm { get; set; }
    /// <summary>冷却液温度(℃)</summary>
    public decimal CoolantTemperatureC { get; set; }
    /// <summary>是否收集冷凝液</summary>
    public bool CollectCondensate { get; set; }
    /// <summary>是否连续进料</summary>
    public bool ContinuousFeed { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>检测参数</summary>
public class DetectionParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>检测方法</summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>检测波长(nm)</summary>
    public int WavelengthNm { get; set; }
    /// <summary>检测时长(分钟)</summary>
    public int DurationMinutes { get; set; }
    /// <summary>备注</summary>
    public string Notes { get; set; } = string.Empty;
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>过滤参数</summary>
public class FiltrationParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>过滤时长(分钟)</summary>
    public int DurationMinutes { get; set; }
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; set; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>干燥参数</summary>
public class DryingParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>干燥剂物料ID</summary>
    public Guid? DesiccantId { get; set; }
    /// <summary>干燥剂用量(mL)</summary>
    public decimal DesiccantVolumeMl { get; set; }
    /// <summary>摇晃速度(rpm)</summary>
    public int ShakeSpeedRpm { get; set; }
    /// <summary>摇晃时长(分钟)</summary>
    public int ShakeDurationMinutes { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>淬灭参数</summary>
public class QuenchingParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>淬灭剂名称</summary>
    public string QuenchingAgent { get; set; } = string.Empty;
    /// <summary>淬灭剂用量(mL)</summary>
    public decimal QuenchingAgentVolumeMl { get; set; }
    /// <summary>淬灭剂滴加速度(mL/min)</summary>
    public decimal QuenchingAgentDripSpeedMlMin { get; set; }
    /// <summary>是否先加淬灭剂</summary>
    public bool AddQuenchingAgentFirst { get; set; }
    /// <summary>预热温度(℃)</summary>
    public decimal PreTemperatureC { get; set; }
    /// <summary>最高温度(℃)</summary>
    public decimal MaxTemperatureC { get; set; }
    /// <summary>搅拌速度(rpm)</summary>
    public int StirSpeedRpm { get; set; }
    /// <summary>淬灭时长(分钟)</summary>
    public int DurationMinutes { get; set; }
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; set; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; set; }
    /// <summary>产品总量(mL)</summary>
    public decimal TotalProductVolumeMl { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>萃取参数</summary>
public class ExtractionParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>萃取剂名称</summary>
    public string ExtractAgent { get; set; } = string.Empty;
    /// <summary>萃取剂用量(mL)</summary>
    public decimal ExtractAgentVolumeMl { get; set; }
    /// <summary>搅拌速度(rpm)</summary>
    public int StirSpeedRpm { get; set; }
    /// <summary>搅拌时长(分钟)</summary>
    public int StirDurationMinutes { get; set; }
    /// <summary>静置时间(分钟)</summary>
    public int SettlingMinutes { get; set; }
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; set; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>取样参数</summary>
public class SamplingParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>取样量(mL)</summary>
    public decimal SampleVolumeMl { get; set; }
    /// <summary>清洗剂名称</summary>
    public string Detergent { get; set; } = string.Empty;
    /// <summary>清洗剂用量(mL)</summary>
    public decimal DetergentVolumeMl { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>离心参数</summary>
public class CentrifugationParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>离心速度(rpm)</summary>
    public int SpeedRpm { get; set; }
    /// <summary>离心温度(℃)</summary>
    public decimal TemperatureC { get; set; }
    /// <summary>离心时长(分钟)</summary>
    public int DurationMinutes { get; set; }
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>自定义检测参数</summary>
public class CustomDetectionParameter
{
    /// <summary>主键ID</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)] public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>参数名称</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>检测方法</summary>
    public string Method { get; set; } = string.Empty;
    /// <summary>自定义参数JSON</summary>
    public string ParameterJson { get; set; } = string.Empty;
    /// <summary>备注</summary>
    public string Notes { get; set; } = string.Empty;
    /// <summary>创建时间</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>更新时间</summary>
    public DateTime? UpdatedAt { get; set; }
}

using IndustrySystem.Domain.Shared.Enums;
using SqlSugar;

namespace IndustrySystem.Domain.Entities.Experiments;

/// <summary>
/// 实验实体。
/// </summary>
public class Experiment
{
    /// <summary>实验主键</summary>
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>实验名称（可根据实验类型自动生成）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>实验类型</summary>
    public ExperimentType Type { get; set; } = ExperimentType.Reaction;

    /// <summary>参数ID（根据实验类型关联参数表）</summary>
    [SugarColumn(IsNullable = true)]
    public Guid? ParameterId { get; set; }

    /// <summary>是否模板</summary>
    public bool IsTemplate { get; set; }

    /// <summary>所属实验组（非模板时可用）</summary>
    [SugarColumn(IsNullable = true)]
    public Guid? GroupId { get; set; }

    /// <summary>创建时间（UTC）</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

using IndustrySystem.Domain.Shared.Enums;
using SqlSugar;

namespace IndustrySystem.Domain.Entities.Experiments;

public class Experiment
{
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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

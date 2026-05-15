namespace IndustrySystem.Domain.Entities.Experiments;

/// <summary>
/// 实验组实体。
/// </summary>
public class ExperimentGroup
{
    /// <summary>实验组主键</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>实验组编号</summary>
    public string GroupCode { get; set; } = string.Empty;

    /// <summary>实验组名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>描述</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>实验步骤（实验ID数组，CSV存储）</summary>
    public string StepExperimentIds { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>创建人</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>创建时间（UTC）</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>步骤实验ID列表（运行时解析，不落库）</summary>
    [SqlSugar.SugarColumn(IsIgnore = true)]
    public List<Guid> StepExperimentIdList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(StepExperimentIds)) return [];
            return StepExperimentIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();
        }
        set
        {
            StepExperimentIds = value is null || value.Count == 0
                ? string.Empty
                : string.Join(',', value.Distinct());
        }
    }
}

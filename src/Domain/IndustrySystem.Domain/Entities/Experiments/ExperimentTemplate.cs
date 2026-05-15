namespace IndustrySystem.Domain.Entities.Experiments;

/// <summary>
/// 实验模板实体。
/// 用于维护可复用的实验定义。
/// </summary>
public class ExperimentTemplate
{
    /// <summary>
    /// 模板主键。
    /// </summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 模板名称。
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述。
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间（UTC）。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间（UTC）。
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

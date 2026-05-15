namespace IndustrySystem.Domain.Entities.Roles;

/// <summary>
/// 角色实体。
/// </summary>
public class Role
{
    /// <summary>角色主键</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>角色名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>角色描述</summary>
    public string? Description { get; set; }

    /// <summary>是否默认角色</summary>
    public bool IsDefault { get; set; }

    /// <summary>创建时间（UTC）</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    public DateTime? UpdatedAt { get; set; }
}

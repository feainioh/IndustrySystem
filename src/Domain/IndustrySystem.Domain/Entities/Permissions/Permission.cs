namespace IndustrySystem.Domain.Entities.Permissions;

/// <summary>
/// 权限定义实体。
/// </summary>
public class Permission
{
    /// <summary>权限主键</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>权限名称（系统标识）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>权限显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>权限分组名称</summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>创建时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

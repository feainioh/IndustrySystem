namespace IndustrySystem.Domain.Entities.Roles;

/// <summary>
/// 角色与权限关系实体。
/// </summary>
public class RolePermission
{
    /// <summary>角色ID（联合主键）</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid RoleId { get; set; }

    /// <summary>权限ID（联合主键）</summary>
    public Guid PermissionId { get; set; }

    /// <summary>创建时间（UTC）</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    public DateTime? UpdatedAt { get; set; }
}

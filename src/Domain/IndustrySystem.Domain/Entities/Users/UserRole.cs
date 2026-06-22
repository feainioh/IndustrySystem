namespace IndustrySystem.Domain.Entities.Users;

/// <summary>
/// 用户与角色关系实体。
/// </summary>
public class UserRole
{
    /// <summary>用户ID（联合主键）</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid UserId { get; set; }

    /// <summary>角色ID（联合主键）</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid RoleId { get; set; }

    /// <summary>创建时间（UTC）</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    public DateTime? UpdatedAt { get; set; }
}

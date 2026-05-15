namespace IndustrySystem.Domain.Entities.Users;

/// <summary>
/// 系统用户实体。
/// </summary>
public class User
{
    /// <summary>用户主键</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>登录用户名</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>显示名称</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>密码哈希值</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>是否启用</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>创建时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

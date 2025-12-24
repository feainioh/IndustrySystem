namespace IndustrySystem.Domain.Entities.Users;

public class UserRole
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

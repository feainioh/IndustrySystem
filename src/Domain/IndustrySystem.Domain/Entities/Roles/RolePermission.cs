namespace IndustrySystem.Domain.Entities.Roles;

public class RolePermission
{
 public Guid RoleId { get; set; }
 public Guid PermissionId { get; set; }
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 public DateTime? UpdatedAt { get; set; }
}

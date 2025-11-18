namespace IndustrySystem.Domain.Entities.Permissions;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    // Added timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

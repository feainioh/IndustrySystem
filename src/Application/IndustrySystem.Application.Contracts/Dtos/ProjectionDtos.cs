namespace IndustrySystem.Application.Contracts.Dtos;

public record UserWithRolesDto(Guid Id, string UserName, string DisplayName, bool IsActive, IReadOnlyList<RoleBriefDto> Roles);
public record RoleWithPermissionsDto(Guid Id, string Name, string? Description, bool IsDefault, bool IsActive, IReadOnlyList<PermissionBriefDto> Permissions);
public record RoleBriefDto(Guid Id, string Name);
public record PermissionBriefDto(Guid Id, string Name, string DisplayName);

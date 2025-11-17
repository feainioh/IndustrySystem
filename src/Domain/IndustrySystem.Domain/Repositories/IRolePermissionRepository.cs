namespace IndustrySystem.Domain.Repositories;

public interface IRolePermissionRepository
{
 Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId);
 Task<Dictionary<Guid, List<Guid>>> GetPermissionIdsByRoleIdsAsync(IEnumerable<Guid> roleIds);
 Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
}

namespace IndustrySystem.Domain.Repositories;

/// <summary>
/// 角色-权限关系仓储契约。
/// </summary>
public interface IRolePermissionRepository
{
	/// <summary>
	/// 获取角色关联的权限标识集合。
	/// </summary>
	Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId);

	/// <summary>
	/// 批量获取角色到权限标识集合的映射。
	/// </summary>
	Task<Dictionary<Guid, List<Guid>>> GetPermissionIdsByRoleIdsAsync(IEnumerable<Guid> roleIds);

	/// <summary>
	/// 覆盖设置角色的权限集合。
	/// </summary>
	Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
}

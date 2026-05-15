namespace IndustrySystem.Domain.Repositories;

/// <summary>
/// 用户-角色关系仓储契约。
/// </summary>
public interface IUserRoleRepository
{
	/// <summary>
	/// 获取用户关联的角色标识集合。
	/// </summary>
	Task<List<Guid>> GetRoleIdsByUserIdAsync(Guid userId);

	/// <summary>
	/// 批量获取用户到角色标识集合的映射。
	/// </summary>
	Task<Dictionary<Guid, List<Guid>>> GetRoleIdsByUserIdsAsync(IEnumerable<Guid> userIds);

	/// <summary>
	/// 覆盖设置用户的角色集合。
	/// </summary>
	Task SetUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct = default);
}

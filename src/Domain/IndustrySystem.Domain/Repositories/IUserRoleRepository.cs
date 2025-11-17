namespace IndustrySystem.Domain.Repositories;

public interface IUserRoleRepository
{
 Task<List<Guid>> GetRoleIdsByUserIdAsync(Guid userId);
 Task<Dictionary<Guid, List<Guid>>> GetRoleIdsByUserIdsAsync(IEnumerable<Guid> userIds);
 Task SetUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct = default);
}

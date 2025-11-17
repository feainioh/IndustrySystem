using IndustrySystem.Domain.Entities.Users;
using IndustrySystem.Domain.Repositories;
using SqlSugar;

namespace IndustrySystem.Infrastructure.SqlSugar.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
 private readonly ISqlSugarClient _db;
 public UserRoleRepository(ISqlSugarClient db) => _db = db;

 public async Task<List<Guid>> GetRoleIdsByUserIdAsync(Guid userId)
 => await _db.Queryable<UserRole>().Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();

 public async Task<Dictionary<Guid, List<Guid>>> GetRoleIdsByUserIdsAsync(IEnumerable<Guid> userIds)
 {
 var ids = userIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
 if (ids.Length ==0) return new();
 var list = await _db.Queryable<UserRole>().Where(x => ids.Contains(x.UserId)).ToListAsync();
 return list.GroupBy(x => x.UserId).ToDictionary(g => g.Key, g => g.Select(x => x.RoleId).ToList());
 }

 public async Task SetUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken ct = default)
 {
 // Replace-all strategy in a transaction
 await _db.Ado.BeginTranAsync();
 try
 {
 await _db.Deleteable<UserRole>().Where(x => x.UserId == userId).ExecuteCommandAsync();
 if (roleIds != null)
 {
 var now = DateTime.UtcNow;
 var records = roleIds.Distinct().Select(rid => new UserRole { UserId = userId, RoleId = rid, CreatedAt = now }).ToList();
 if (records.Count >0)
 await _db.Insertable(records).ExecuteCommandAsync();
 }
 await _db.Ado.CommitTranAsync();
 }
 catch
 {
 await _db.Ado.RollbackTranAsync();
 throw;
 }
 }
}

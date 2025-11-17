using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Repositories;
using SqlSugar;

namespace IndustrySystem.Infrastructure.SqlSugar.Repositories;

public class RolePermissionRepository : IRolePermissionRepository
{
 private readonly ISqlSugarClient _db;
 public RolePermissionRepository(ISqlSugarClient db) => _db = db;

 public async Task<List<Guid>> GetPermissionIdsByRoleIdAsync(Guid roleId)
 => await _db.Queryable<RolePermission>().Where(x => x.RoleId == roleId).Select(x => x.PermissionId).ToListAsync();

 public async Task<Dictionary<Guid, List<Guid>>> GetPermissionIdsByRoleIdsAsync(IEnumerable<Guid> roleIds)
 {
 var ids = roleIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
 if (ids.Length ==0) return new();
 var list = await _db.Queryable<RolePermission>().Where(x => ids.Contains(x.RoleId)).ToListAsync();
 return list.GroupBy(x => x.RoleId).ToDictionary(g => g.Key, g => g.Select(x => x.PermissionId).ToList());
 }

 public async Task SetRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default)
 {
 await _db.Ado.BeginTranAsync();
 try
 {
 await _db.Deleteable<RolePermission>().Where(x => x.RoleId == roleId).ExecuteCommandAsync();
 if (permissionIds != null)
 {
 var now = DateTime.UtcNow;
 var records = permissionIds.Distinct().Select(pid => new RolePermission { RoleId = roleId, PermissionId = pid, CreatedAt = now }).ToList();
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

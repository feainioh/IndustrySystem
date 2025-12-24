using System.Threading.Tasks;
using System;
using System.Linq;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.Services;

public class AuthService : IAuthService
{
 private readonly IUserAppService _userSvc;
 private readonly IRoleAppService _roleSvc;
 private readonly IPermissionAppService _permSvc;
 public AuthService(IUserAppService userSvc, IRoleAppService roleSvc, IPermissionAppService permSvc) { _userSvc = userSvc; _roleSvc = roleSvc; _permSvc = permSvc; }

 public async Task<bool> SignInAsync(string userName, string password)
 {
 if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
 var users = await _userSvc.GetListAsync();
 return users.Any(u => u.UserName.Equals(userName, System.StringComparison.OrdinalIgnoreCase));
 }

 public async Task<(Guid[] RoleIds, string[] Permissions)> GetIdentityAsync(string userName)
 {
 var users = await _userSvc.GetListAsync();
 var user = users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
 if (user == null) return (Array.Empty<Guid>(), Array.Empty<string>());
 var roleIds = await _userSvc.GetRoleIdsAsync(user.Id);
 // Aggregate permissions by roles
 var permNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
 foreach (var roleId in roleIds)
 {
 var permIds = await _roleSvc.GetPermissionIdsAsync(roleId);
 // In absence of a service to query permission names by id efficiently, fetch all and match
 var allPerms = await _permSvc.GetListAsync();
 foreach (var pid in permIds)
 {
 var name = allPerms.FirstOrDefault(p => p.Id == pid)?.Name;
 if (!string.IsNullOrWhiteSpace(name)) permNames.Add(name!);
 }
 }
 return (roleIds, permNames.ToArray());
 }
}

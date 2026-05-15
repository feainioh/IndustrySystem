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
        return await _userSvc.ValidateCredentialsAsync(userName, password);
    }

    public async Task<(Guid[] RoleIds, string[] Permissions)> GetIdentityAsync(string userName)
    {
        var users = await _userSvc.GetListAsync();
        var user = users.FirstOrDefault(u => u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
        if (user == null) return (Array.Empty<Guid>(), Array.Empty<string>());
        var roleIds = await _userSvc.GetRoleIdsAsync(user.Id);
        // Aggregate permission names and role names into a single capability set.
        var permNames = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var allPerms = await _permSvc.GetListAsync();
        var allRoles = await _roleSvc.GetListAsync();

        // Role names are also used by current menu visibility bindings (e.g. Admin/SuperAdmin).
        foreach (var roleId in roleIds)
        {
            var roleName = allRoles.FirstOrDefault(r => r.Id == roleId)?.Name;
            if (!string.IsNullOrWhiteSpace(roleName)) permNames.Add(roleName!);
        }

        foreach (var roleId in roleIds)
        {
            var permIds = await _roleSvc.GetPermissionIdsAsync(roleId);
            foreach (var pid in permIds)
            {
                var name = allPerms.FirstOrDefault(p => p.Id == pid)?.Name;
                if (!string.IsNullOrWhiteSpace(name)) permNames.Add(name!);
            }
        }

        // Fallback for default bootstrap admin when role assignment is not initialized yet.
        if (permNames.Count == 0 && user.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            permNames.Add("Admin");
        }

        return (roleIds, permNames.ToArray());
    }
}

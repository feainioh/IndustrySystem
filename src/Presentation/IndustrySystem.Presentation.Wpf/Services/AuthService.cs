using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Presentation.Wpf.Services;

public class AuthService : IAuthService
{
    private readonly IUserAppService _userSvc;
    private readonly IRoleAppService _roleSvc;
    private readonly IPermissionAppService _permSvc;
    private readonly IRolePermissionRepository _rolePermRepo;

    public AuthService(
        IUserAppService userSvc,
        IRoleAppService roleSvc,
        IPermissionAppService permSvc,
        IRolePermissionRepository rolePermRepo)
    {
        _userSvc = userSvc;
        _roleSvc = roleSvc;
        _permSvc = permSvc;
        _rolePermRepo = rolePermRepo;
    }

    public async Task<bool> SignInAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
        return await _userSvc.ValidateCredentialsAsync(userName, password);
    }

    public async Task<(Guid[] RoleIds, string[] Permissions)> GetIdentityAsync(string userName)
    {
        // 1. 定向查询用户（WHERE UserName = @name）
        var user = await _userSvc.GetByUserNameAsync(userName);
        if (user == null) return (Array.Empty<Guid>(), Array.Empty<string>());

        // 2. 查用户角色（一条 SQL）
        var roleIds = await _userSvc.GetRoleIdsAsync(user.Id);
        if (roleIds.Length == 0) return (Array.Empty<Guid>(), Array.Empty<string>());

        // 3. 角色表和权限表极小（<20 行），一次性全量加载
        var allRoles = await _roleSvc.GetListAsync();
        var allPerms = await _permSvc.GetListAsync();

        var permNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 角色名作为权限标识（菜单可见性绑定）
        foreach (var roleId in roleIds)
        {
            var roleName = allRoles.FirstOrDefault(r => r.Id == roleId)?.Name;
            if (!string.IsNullOrWhiteSpace(roleName)) permNames.Add(roleName);
        }

        // 4. 批量查询角色权限映射（一条 SQL 替代 N+1 次单角色查询）
        var rolePermMap = await _rolePermRepo.GetPermissionIdsByRoleIdsAsync(roleIds);
        foreach (var kv in rolePermMap)
        {
            foreach (var pid in kv.Value)
            {
                var name = allPerms.FirstOrDefault(p => p.Id == pid)?.Name;
                if (!string.IsNullOrWhiteSpace(name)) permNames.Add(name);
            }
        }

        return (roleIds, permNames.ToArray());
    }
}

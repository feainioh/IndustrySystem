using System;
using System.Collections.Generic;
using System.Linq;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Implementation for global auth state.
/// </summary>
public class AuthState : IAuthState
{
    public bool IsAuthenticated { get; private set; }
    public string? UserName { get; private set; }
    private readonly HashSet<Guid> _roleIds = new();
    private readonly HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyCollection<Guid> RoleIds => _roleIds;
    public IReadOnlyCollection<string> Permissions => _permissions;
    public event EventHandler? AuthChanged;

    public void SetAuthenticated(string userName)
    {
        IsAuthenticated = true; UserName = userName;
        _roleIds.Clear();
        _permissions.Clear();
        AuthChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetAuthenticated(string userName, Guid[] roleIds, string[] permissions)
    {
        IsAuthenticated = true; UserName = userName;
        _roleIds.Clear(); foreach (var id in roleIds) _roleIds.Add(id);
        _permissions.Clear(); foreach (var p in permissions) if (!string.IsNullOrWhiteSpace(p)) _permissions.Add(p);
        AuthChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SignOut()
    {
        IsAuthenticated = false; UserName = null; _roleIds.Clear(); _permissions.Clear();
        AuthChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool HasPermission(string permission) => IsAuthenticated && _permissions.Contains(permission);
}

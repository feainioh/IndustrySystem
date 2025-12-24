using System;
using System;
using System.Collections.Generic;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Holds current authentication state and notifies changes.
/// </summary>
public interface IAuthState
{
    bool IsAuthenticated { get; }
    string? UserName { get; }
    IReadOnlyCollection<Guid> RoleIds { get; }
    IReadOnlyCollection<string> Permissions { get; }
    event EventHandler? AuthChanged;

    void SetAuthenticated(string userName);
    void SetAuthenticated(string userName, Guid[] roleIds, string[] permissions);
    void SignOut();

    bool HasPermission(string permission);
}

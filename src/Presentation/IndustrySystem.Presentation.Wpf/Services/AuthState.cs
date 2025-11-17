using System;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Implementation for global auth state.
/// </summary>
public class AuthState : IAuthState
{
 public bool IsAuthenticated { get; private set; }
 public string? UserName { get; private set; }
 public event EventHandler? AuthChanged;

 public void SetAuthenticated(string userName)
 {
 IsAuthenticated = true; UserName = userName;
 AuthChanged?.Invoke(this, EventArgs.Empty);
 }

 public void SignOut()
 {
 IsAuthenticated = false; UserName = null;
 AuthChanged?.Invoke(this, EventArgs.Empty);
 }
}

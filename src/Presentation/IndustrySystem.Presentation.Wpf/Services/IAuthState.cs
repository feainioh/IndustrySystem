using System;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Holds current authentication state and notifies changes.
/// </summary>
public interface IAuthState
{
 bool IsAuthenticated { get; }
 string? UserName { get; }
 event EventHandler? AuthChanged;
 void SetAuthenticated(string userName);
 void SignOut();
}

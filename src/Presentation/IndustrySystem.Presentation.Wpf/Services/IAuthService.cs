using System.Threading.Tasks;
using System;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Authentication service abstraction for the WPF shell.
/// </summary>
public interface IAuthService
{
 /// <summary>
 /// Try to sign-in with the given credentials.
 /// </summary>
 Task<bool> SignInAsync(string userName, string password);
 
 /// <summary>
 /// Get current user's role ids and permissions after sign-in.
 /// </summary>
 Task<(Guid[] RoleIds, string[] Permissions)> GetIdentityAsync(string userName);
}

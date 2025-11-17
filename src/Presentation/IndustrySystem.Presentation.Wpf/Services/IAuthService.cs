using System.Threading.Tasks;

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
}

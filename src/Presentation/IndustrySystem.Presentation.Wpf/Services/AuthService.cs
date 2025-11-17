using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.Services;

/// <summary>
/// Simple auth implementation: delegates to IUserAppService for verification.
/// </summary>
public class AuthService : IAuthService
{
 private readonly IUserAppService _userSvc;
 public AuthService(IUserAppService userSvc) { _userSvc = userSvc; }

 public async Task<bool> SignInAsync(string userName, string password)
 {
 // TODO: replace with real credential validation when backend supports it.
 // For now treat non-empty as success and ensure user exists or create a temp principal.
 if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
 var paged = await _userSvc.GetPagedWithRolesAsync(1,1, userName);
 return paged.TotalCount >0; // simple check
 }
}

using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.Services;

public class AuthService : IAuthService
{
 private readonly IUserAppService _userSvc;
 public AuthService(IUserAppService userSvc) { _userSvc = userSvc; }

 public async Task<bool> SignInAsync(string userName, string password)
 {
 if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) return false;
 var users = await _userSvc.GetListAsync();
 return users.Any(u => u.UserName.Contains(userName, System.StringComparison.OrdinalIgnoreCase));
 }
}

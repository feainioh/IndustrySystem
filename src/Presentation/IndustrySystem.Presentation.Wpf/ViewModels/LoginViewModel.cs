using Prism.Commands;
using Prism.Dialogs;
using System.Windows.Input;
using System.Threading.Tasks;
using IndustrySystem.Presentation.Wpf.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using System.IO.IsolatedStorage;
using System.IO;
using System;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>
/// Login view model with remember and quick-login support.
/// </summary>
public class LoginViewModel : DialogViewModel
{
    private const string LoginFailedFallback = "\u767B\u5F55\u5931\u8D25";
    private const string LoginFailedCheckCredentialsFallback = "\u767B\u5F55\u5931\u8D25\uFF0C\u8BF7\u68C0\u67E5\u8D26\u53F7\u6216\u5BC6\u7801";

    private readonly IAuthService _auth;
    private readonly IAuthState _state;

    public LoginViewModel(IAuthService auth, IAuthState state)
    {
        _auth = auth;
        _state = state;
        Title = Strings.Page_Login_Title;

        LoginCommand = new DelegateCommand(async () => await LoginAsync(), CanLogin)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);
        QuickLoginCommand = new DelegateCommand(async () => await QuickLoginAsync());
        LoadRemembered();
    }

    private void CloseDialog(ButtonResult result) => RequestClose.Invoke(result);

    private string _userName = string.Empty;
    public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private bool _remember;
    public bool Remember { get => _remember; set => SetProperty(ref _remember, value); }

    private string? _errorMessage;
    /// <summary>Login failure or error message.</summary>
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                RaisePropertyChanged(nameof(HasError));
            }
        }
    }

    /// <summary>Whether an error message exists.</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ICommand LoginCommand { get; }
    public ICommand QuickLoginCommand { get; }

    private bool CanLogin() => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

    private async Task LoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged();
        try
        {
            if (await _auth.SignInAsync(UserName, Password))
            {
                if (Remember) SaveRemembered(); else ClearRemembered();
                var identity = await _auth.GetIdentityAsync(UserName);
                _state.SetAuthenticated(UserName, identity.RoleIds, identity.Permissions);
                CloseDialog(ButtonResult.OK);
            }
            else
            {
                ErrorMessage = GetString("Login_Failed_CheckCredentials", LoginFailedCheckCredentialsFallback);
            }
        }
        finally
        {
            IsBusy = false;
            ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    private async Task QuickLoginAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged();
        try
        {
            var u = string.IsNullOrWhiteSpace(UserName) ? "admin" : UserName;
            var p = string.IsNullOrWhiteSpace(Password) ? "admin" : Password;
            if (await _auth.SignInAsync(u, p))
            {
                var identity = await _auth.GetIdentityAsync(u);
                _state.SetAuthenticated(u, identity.RoleIds, identity.Permissions);
                CloseDialog(ButtonResult.OK);
            }
            else
            {
                ErrorMessage = GetString("Login_Failed", LoginFailedFallback);
            }
        }
        finally
        {
            IsBusy = false;
            ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged();
        }
    }

    protected override void OnCancel()
    {
        UserName = string.Empty;
        Password = string.Empty;
        ErrorMessage = null;
        base.OnCancel();
    }

    private void LoadRemembered()
    {
        try
        {
            using var store = IsolatedStorageFile.GetUserStoreForAssembly();
            if (store.FileExists("login.cache"))
            {
                using var s = new IsolatedStorageFileStream("login.cache", FileMode.Open, store);
                using var sr = new StreamReader(s);
                var user = sr.ReadLine();
                var pass = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(user))
                {
                    UserName = user;
                    Remember = true;
                }
                if (!string.IsNullOrEmpty(pass))
                {
                    Password = pass;
                }
            }
        }
        catch { }
    }

    private void SaveRemembered()
    {
        try
        {
            using var store = IsolatedStorageFile.GetUserStoreForAssembly();
            using var s = new IsolatedStorageFileStream("login.cache", FileMode.Create, store);
            using var sw = new StreamWriter(s);
            sw.WriteLine(UserName);
            sw.WriteLine(Password);
        }
        catch { }
    }

    private void ClearRemembered()
    {
        try
        {
            using var store = IsolatedStorageFile.GetUserStoreForAssembly();
            if (store.FileExists("login.cache"))
            {
                store.DeleteFile("login.cache");
            }
        }
        catch { }
    }

    private static string GetString(string key, string fallback)
        => Strings.ResourceManager.GetString(key, Strings.Culture) ?? fallback;
}

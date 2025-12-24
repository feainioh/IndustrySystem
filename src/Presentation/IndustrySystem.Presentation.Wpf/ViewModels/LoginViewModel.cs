using Prism.Mvvm;
using Prism.Commands;
using Prism.Dialogs;
using System.Windows.Input;
using System.Threading.Tasks;
using IndustrySystem.Presentation.Wpf.Services;
using System.IO.IsolatedStorage;
using System.IO;
using System;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>
/// Login view model with remember and quick-login support.
/// Implements IDialogAware for Prism DialogService.
/// </summary>
public class LoginViewModel : BindableBase, IDialogAware
{
    private readonly IAuthService _auth;
    private readonly IAuthState _state;

    public LoginViewModel(IAuthService auth, IAuthState state)
    {
        _auth = auth;
        _state = state;
        LoginCommand = new DelegateCommand(async () => await LoginAsync(), CanLogin)
            .ObservesProperty(() => UserName)
            .ObservesProperty(() => Password);
        CancelCommand = new DelegateCommand(Cancel);
        QuickLoginCommand = new DelegateCommand(async () => await QuickLoginAsync());
        LoadRemembered();
    }

    #region IDialogAware Implementation

    public string Title => "µÇÂ¼";

    public DialogCloseListener RequestClose { get; set; }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed() { }

    public void OnDialogOpened(IDialogParameters parameters) { }

    private void CloseDialog(ButtonResult result) => RequestClose.Invoke(result);

    #endregion

    private string _userName = string.Empty;
    public string UserName { get => _userName; set => SetProperty(ref _userName, value); }

    private string _password = string.Empty;
    public string Password { get => _password; set => SetProperty(ref _password, value); }

    private bool _remember;
    public bool Remember { get => _remember; set => SetProperty(ref _remember, value); }

    private string? _errorMessage;
    /// <summary>µÇÂ¼Ê§°Ü»ò´íÎóÌáÊ¾</summary>
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

    /// <summary>ÊÇ·ñ´æÔÚ´íÎó</summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    private bool _isBusy;
    /// <summary>µÇÂ¼Ã¦Âµ×´Ì¬</summary>
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }

    public ICommand LoginCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand QuickLoginCommand { get; }

    private bool CanLogin() => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password) && !IsBusy;

    private async Task LoginAsync()
    {
        ErrorMessage = null; IsBusy = true;
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
            else { ErrorMessage = "µÇÂ¼Ê§°Ü£¬Çë¼ì²éÕËºÅ»òÃÜÂë"; }
        }
        finally { IsBusy = false; ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    private async Task QuickLoginAsync()
    {
        ErrorMessage = null; IsBusy = true;
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
            else { ErrorMessage = "µÇÂ¼Ê§°Ü"; }
        }
        finally { IsBusy = false; ((DelegateCommand)LoginCommand).RaiseCanExecuteChanged(); }
    }

    private void Cancel() { UserName = string.Empty; Password = string.Empty; ErrorMessage = null; CloseDialog(ButtonResult.Cancel); }

    private void LoadRemembered()
    {
        try
        {
            using var store = IsolatedStorageFile.GetUserStoreForAssembly();
            if (store.FileExists("login.cache"))
            {
                using var s = new IsolatedStorageFileStream("login.cache", FileMode.Open, store);
                using var sr = new StreamReader(s);
                var user = sr.ReadLine(); var pass = sr.ReadLine();
                if (!string.IsNullOrWhiteSpace(user)) { UserName = user; Remember = true; }
                if (!string.IsNullOrEmpty(pass)) { Password = pass; }
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
            sw.WriteLine(UserName); sw.WriteLine(Password);
        }
        catch { }
    }

    private void ClearRemembered()
    {
        try
        {
            using var store = IsolatedStorageFile.GetUserStoreForAssembly();
            if (store.FileExists("login.cache")) store.DeleteFile("login.cache");
        }
        catch { }
    }
}

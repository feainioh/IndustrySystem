using Prism.Mvvm;
using Prism.Commands;
using Prism.Navigation;
using NLog;
using System.Windows.Input;
using ModernWpf.Controls;
using IndustrySystem.Presentation.Wpf.Services;
using IndustrySystem.Presentation.Wpf.Resources;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ShellViewModel : BindableBase
{
    public const string MainRegionName = "ShellMainRegion";
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly IRegionManager _regionManager;
    private readonly IAuthState _authState;
    private readonly IAppSessionService _appSessionService;
    private readonly IMainWindowService _mainWindowService;

    private string _currentUserName = Strings.Lbl_NotLoggedIn;
    private string _visualThemeGlyph = "\uE771";
    private string _visualThemeText = "经典主题";
    private string _visualThemeToolTip = "当前：经典主题，点击切换到液态玻璃";

    public string CurrentUserName
    {
        get => _currentUserName;
        private set => SetProperty(ref _currentUserName, value);
    }

    public string VisualThemeGlyph
    {
        get => _visualThemeGlyph;
        private set => SetProperty(ref _visualThemeGlyph, value);
    }

    public string VisualThemeText
    {
        get => _visualThemeText;
        private set => SetProperty(ref _visualThemeText, value);
    }

    public string VisualThemeToolTip
    {
        get => _visualThemeToolTip;
        private set => SetProperty(ref _visualThemeToolTip, value);
    }

    /// <summary>
    /// </summary>
    public IAuthState AuthState => _authState;

    public ICommand OnLoadedCommand { get; }
    public ICommand NavSelectionChangedCommand { get; }
    public ICommand ToggleVisualThemeCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand MinimizeWindowCommand { get; }
    public ICommand ToggleWindowStateCommand { get; }
    public ICommand CloseWindowCommand { get; }

    public ShellViewModel(
        IAuthState authState,
        IRegionManager regionManager,
        IAppSessionService appSessionService,
        IMainWindowService mainWindowService)
    {
        _authState = authState;
        _regionManager = regionManager;
        _appSessionService = appSessionService;
        _mainWindowService = mainWindowService;

        CurrentUserName = _authState.UserName ?? Strings.Lbl_NotLoggedIn;
        _authState.AuthChanged += (s, e) =>
        {
            CurrentUserName = _authState.UserName ?? Strings.Lbl_NotLoggedIn;
            RaisePropertyChanged(nameof(AuthState));
        };

        OnLoadedCommand = new DelegateCommand<object?>(OnLoaded);
        NavSelectionChangedCommand = new DelegateCommand<object?>(OnSelectionChanged);
        ToggleVisualThemeCommand = new DelegateCommand(OnToggleVisualTheme);
        LogoutCommand = new DelegateCommand(OnLogout);
        MinimizeWindowCommand = new DelegateCommand(() => _mainWindowService.Minimize());
        ToggleWindowStateCommand = new DelegateCommand(() => _mainWindowService.ToggleMaximizeRestore());
        CloseWindowCommand = new DelegateCommand(() => _mainWindowService.Close());

        UpdateVisualThemeMetadata(AppVisualThemeService.Current);
    }

    private void OnLoaded(object? _)
    {
        // Default -> Users page
        Navigate("UsersView");
    }

    private void OnSelectionChanged(object? args)
    {
        if (args is NavigationViewSelectionChangedEventArgs ev && ev.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            Navigate(tag);
        }
    }

    /// <summary>
    /// Public navigation API for code-behind to navigate after login.
    /// </summary>
    public void NavigateTo(string viewName) => Navigate(viewName);

    private void OnToggleVisualTheme()
    {
        var theme = AppVisualThemeService.Toggle();
        UpdateVisualThemeMetadata(theme);
    }

    private void OnLogout()
    {
        _appSessionService.LogoutAndShowLoginDialog();
    }

    private void UpdateVisualThemeMetadata(AppVisualTheme theme)
    {
        if (theme == AppVisualTheme.LiquidGlass)
        {
            VisualThemeGlyph = "\uE790";
            VisualThemeText = "液态玻璃";
            VisualThemeToolTip = "当前：液态玻璃，点击切换到经典主题";
            return;
        }

        VisualThemeGlyph = "\uE771";
        VisualThemeText = "经典主题";
        VisualThemeToolTip = "当前：经典主题，点击切换到液态玻璃";
    }

    private void Navigate(string viewName)
    {
        if (string.IsNullOrWhiteSpace(viewName))
        {
            Logger.Warn("[Shell Navigation] Ignored empty view name.");
            return;
        }

        try
        {
            _regionManager.RequestNavigate(MainRegionName, viewName, result =>
            {
                if (result.Success)
                {
                    Logger.Debug($"[Shell Navigation] Success: {viewName} -> {MainRegionName}");
                    return;
                }

                if (result.Cancelled)
                {
                    Logger.Warn($"[Shell Navigation] Cancelled: {viewName} -> {MainRegionName}");
                    return;
                }

                if (result.Exception is not null)
                {
                    Logger.Error(result.Exception, $"[Shell Navigation] Failed: {viewName} -> {MainRegionName}");
                }
                else
                {
                    Logger.Warn($"[Shell Navigation] Failed without exception: {viewName} -> {MainRegionName}");
                }
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"[Shell Navigation] RequestNavigate threw: {viewName} -> {MainRegionName}");
        }
    } 
}

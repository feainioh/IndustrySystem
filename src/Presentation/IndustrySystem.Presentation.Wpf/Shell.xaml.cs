using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.Views;
using ModernWpf;
using ModernWpf.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows.Threading;

namespace IndustrySystem.Presentation.Wpf
{
    public partial class Shell : Window
    {
        private readonly IContainerProvider _container;
        private bool _loginShown;

        public Shell(IContainerProvider container)
        {
            _container = container;
            InitializeComponent();
            DataContext = _container.Resolve<ViewModels.ShellViewModel>();

            // Show login only after first render to ensure DialogHost is loaded
            this.ContentRendered += async (_, __) => await ShowLoginIfNeededAsync();
        }

        private async Task ShowLoginIfNeededAsync()
        {
            if (_loginShown) return;
            _loginShown = true;
            var authState = _container.Resolve<Services.IAuthState>();
            if (authState.IsAuthenticated) return;

#if DEBUG
            // Auto-login in debug to speed up dev
            var auth = _container.Resolve<Services.IAuthService>();
            if (await auth.SignInAsync("admin", "admin"))
            {
                authState.SetAuthenticated("admin");
                return;
            }
#endif
            var login = new Views.LoginView();
            // Ensure this runs after the DialogHost registered as loaded
            await Dispatcher.InvokeAsync(async () =>
            {
                await DialogHost.Show(login, "RootDialogHost");
            }, DispatcherPriority.ApplicationIdle);
        }

        private void NavigateToRole(object sender, RoutedEventArgs e)
        {
            var view = _container.Resolve<RoleManageView>();
            if (FindName("MainRegionHost") is ContentControl host) host.Content = view;
        }

        private void NavigateToTemplate(object sender, RoutedEventArgs e)
        {
            var view = _container.Resolve<ExperimentTemplateView>();
            if (FindName("MainRegionHost") is ContentControl host) host.Content = view;
        }

        private void NavigateToPermissions(object sender, RoutedEventArgs e)
        {
            var view = _container.Resolve<PermissionsView>();
            if (FindName("MainRegionHost") is ContentControl host) host.Content = view;
        }

        private void NavigateToUsers(object sender, RoutedEventArgs e)
        {
            var view = _container.Resolve<UsersView>();
            if (FindName("MainRegionHost") is ContentControl host) host.Content = view;
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch ?? FindName("ThemeToggle") as ToggleSwitch;
            var isOn = ts?.IsOn == true;
            ThemeManager.Current.ApplicationTheme = isOn ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItemContainer?.Tag is string tag && DataContext is ViewModels.ShellViewModel vm)
            {
                vm.NavigateTo(tag);
            }
        }
    }
}

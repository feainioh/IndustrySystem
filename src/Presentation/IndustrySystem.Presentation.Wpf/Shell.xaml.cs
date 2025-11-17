using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.Views;
using ModernWpf;
using ModernWpf.Controls;

namespace IndustrySystem.Presentation.Wpf
{
    public partial class Shell : Window
    {
        private readonly IContainerProvider _container;

        public Shell(IContainerProvider container)
        {
            _container = container;
            InitializeComponent();
            // Set default view on startup
            try
            {
                var view = _container.Resolve<UsersView>();
                if (FindName("MainRegionHost") is ContentControl host) host.Content = view;
            }
            catch { }
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
    }
}

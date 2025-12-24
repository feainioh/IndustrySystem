using System.Windows;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.Services;
using ModernWpf;
using ModernWpf.Controls;
using System;

namespace IndustrySystem.Presentation.Wpf
{
    public partial class Shell : Window
    {
        private readonly IContainerProvider _container;
        private readonly ShellViewModel _viewModel;
        private bool _isLoggingOut = false;

        public Shell(IContainerProvider container)
        {
            _container = container;
            InitializeComponent();
            
            // Create and set ViewModel
            _viewModel = new ShellViewModel(container);
            DataContext = _viewModel;
            
            // Subscribe to auth state changes
            var authState = container.Resolve<IAuthState>();
            UpdateUserInfo(authState);
            authState.AuthChanged += (s, e) => UpdateUserInfo(authState);
            
            // Handle window closing
            Closed += OnWindowClosed;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            // If not logging out, shut down the application
            if (!_isLoggingOut)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void UpdateUserInfo(IAuthState authState)
        {
            // Update user display in the Shell UI if needed
            if (FindName("UserNameText") is System.Windows.Controls.TextBlock userText)
            {
                userText.Text = authState.IsAuthenticated ? authState.UserName! : "Î´µÇÂ¼";
            }
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var ts = sender as ToggleSwitch ?? FindName("ThemeToggle") as ToggleSwitch;
            var isOn = ts?.IsOn == true;
            ThemeManager.Current.ApplicationTheme = isOn ? ApplicationTheme.Dark : ApplicationTheme.Light;
        }

        private void OnLogoutClick(object sender, RoutedEventArgs e)
        {
            var authState = _container.Resolve<IAuthState>();
            authState.SignOut();
            
            // Mark as logging out to prevent application shutdown
            _isLoggingOut = true;
            
            // Close shell and show login dialog again
            Close();
            
            // Show login dialog
            ((App)System.Windows.Application.Current).ShowLoginDialog();
        }
    }
}

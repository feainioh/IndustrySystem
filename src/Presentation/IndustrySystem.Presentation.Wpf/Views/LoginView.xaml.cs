using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.Services;
using System.Windows;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class LoginView : UserControl
    {
        private bool _passwordHooked;

        public LoginView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualThemeButton(AppVisualThemeService.Current);

            if (DataContext is not LoginViewModel vm)
            {
                return;
            }

            if (this.FindName("Pwd") is not PasswordBox pb)
            {
                return;
            }

            if (pb.Password != vm.Password)
            {
                pb.Password = vm.Password ?? string.Empty;
            }

            if (_passwordHooked)
            {
                return;
            }

            pb.PasswordChanged += (s, _) => vm.Password = pb.Password;
            _passwordHooked = true;
        }

        private void LoginVisualThemeButton_Click(object sender, RoutedEventArgs e)
        {
            var theme = AppVisualThemeService.Toggle();
            UpdateVisualThemeButton(theme);
        }

        private void UpdateVisualThemeButton(AppVisualTheme theme)
        {
            LoginVisualThemeIcon.Text = theme == AppVisualTheme.LiquidGlass ? "\uE790" : "\uE771";
            LoginVisualThemeText.Text = theme == AppVisualTheme.LiquidGlass ? "液态玻璃" : "经典主题";
            LoginVisualThemeButton.ToolTip = theme == AppVisualTheme.LiquidGlass
                ? "当前：液态玻璃，点击切换到经典主题"
                : "当前：经典主题，点击切换到液态玻璃";
        }
    }
}

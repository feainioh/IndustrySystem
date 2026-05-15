using System.Windows;
using System.Windows.Media;
using System;

namespace IndustrySystem.Presentation.Wpf
{
    public partial class Shell : Window
    {
        private bool _isLoggingOut = false;

        public Shell()
        {
            InitializeComponent();

            // Handle window closing
            Closed += OnWindowClosed;
            StateChanged += OnWindowStateChanged;
            UpdateWindowStateButtonGlyph();
            UpdateShellContentClip();
        }

        public void PrepareForLogout()
        {
            _isLoggingOut = true;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            // If not logging out, shut down the application
            if (!_isLoggingOut)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            UpdateWindowStateButtonGlyph();
        }

        private void UpdateWindowStateButtonGlyph()
        {
            if (FindName("WindowStateIcon") is ModernWpf.Controls.FontIcon icon)
            {
                icon.Glyph = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            }
        }

        private void ShellContentClipHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShellContentClip();
        }

        private void UpdateShellContentClip()
        {
            if (ShellContentClipHost.ActualWidth <= 0 || ShellContentClipHost.ActualHeight <= 0)
            {
                return;
            }

            var corner = ShellContentBorder.CornerRadius;
            var radius = Math.Max(0,
                Math.Min(
                    Math.Min(corner.TopLeft, corner.TopRight),
                    Math.Min(corner.BottomLeft, corner.BottomRight)));

            ShellContentClipHost.Clip = new RectangleGeometry(
                new Rect(0, 0, ShellContentClipHost.ActualWidth, ShellContentClipHost.ActualHeight),
                radius,
                radius);
        }
    }
}

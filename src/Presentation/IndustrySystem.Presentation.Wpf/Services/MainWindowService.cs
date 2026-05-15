using System.Windows;

namespace IndustrySystem.Presentation.Wpf.Services;

public class MainWindowService : IMainWindowService
{
    public void Minimize()
    {
        if (System.Windows.Application.Current?.MainWindow is not Window window)
        {
            return;
        }

        window.WindowState = WindowState.Minimized;
    }

    public void ToggleMaximizeRestore()
    {
        if (System.Windows.Application.Current?.MainWindow is not Window window)
        {
            return;
        }

        window.WindowState = window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    public void Close()
    {
        System.Windows.Application.Current?.MainWindow?.Close();
    }
}

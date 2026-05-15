using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;

namespace IndustrySystem.Presentation.Wpf.Behaviors;

public sealed class WindowTitleBarDragBehavior : Behavior<FrameworkElement>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        base.OnDetaching();
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindVisualParent<ButtonBase>(e.OriginalSource as DependencyObject) is not null)
        {
            return;
        }

        var window = Window.GetWindow(AssociatedObject);
        if (window is null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            e.Handled = true;
            return;
        }

        if (e.LeftButton == MouseButtonState.Pressed)
        {
            window.DragMove();
            e.Handled = true;
        }
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T parent)
            {
                return parent;
            }

            child = VisualTreeHelper.GetParent(child);
        }

        return null;
    }
}
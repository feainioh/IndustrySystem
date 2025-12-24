using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using IndustrySystem.Presentation.Wpf.Services;

namespace IndustrySystem.Presentation.Wpf.Converters;

public class PermissionToVisibilityConverter : IMultiValueConverter, IValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 1) return Visibility.Collapsed;
        var state = values[0] as IAuthState;
        if (state is null) return Visibility.Collapsed;

        // Accept one or more permission strings after the first value
        var perms = values.Skip(1)
                          .Select(v => v as string)
                          .Where(s => !string.IsNullOrWhiteSpace(s))
                          .ToArray();

        if (perms.Length == 0 && parameter is string p && !string.IsNullOrWhiteSpace(p))
        {
            perms = new[] { p };
        }

        if (perms.Length == 0) return Visibility.Collapsed;

        // Visible if any permission matches
        var ok = perms.Any(state.HasPermission);
        return ok ? Visibility.Visible : Visibility.Collapsed;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var state = value as IAuthState;
        if (state is null) return Visibility.Collapsed;
        if (parameter is string perm && !string.IsNullOrWhiteSpace(perm))
        {
            return state.HasPermission(perm) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}

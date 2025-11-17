using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// Negates a boolean value for bindings.
/// </summary>
public sealed class BooleanNegationConverter : IValueConverter
{
    public static readonly BooleanNegationConverter Instance = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => !(value as bool? ?? false);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => !(value as bool? ?? false);
}

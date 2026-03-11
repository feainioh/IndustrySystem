using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// Returns true when value is not null; false otherwise.
/// </summary>
public class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

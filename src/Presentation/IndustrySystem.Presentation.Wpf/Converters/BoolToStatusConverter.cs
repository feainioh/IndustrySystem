using System;
using System.Globalization;
using System.Windows.Data;
using IndustrySystem.Presentation.Wpf.Resources;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// Converts a boolean value to a status string (Active/Inactive).
/// </summary>
public class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            return isActive ? Strings.Status_Active : Strings.Status_Inactive;
        }
        return "Unknown";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

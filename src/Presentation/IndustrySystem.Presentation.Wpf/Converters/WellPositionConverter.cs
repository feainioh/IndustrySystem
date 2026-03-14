using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// Converts WellRow and WellColumn to a display label like "A1", "B3", etc.
/// If WellRow is 0, returns "-".
/// </summary>
public class WellPositionConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2
            && values[0] is int row && row > 0
            && values[1] is int col && col > 0)
        {
            var rowLetter = (char)('A' + row - 1);
            return $"{rowLetter}{col}";
        }
        return "-";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

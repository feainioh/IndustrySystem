using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters;

/// <summary>
/// MultiValueConverter: returns true when value[0] &lt; value[1] (both converted to decimal).
/// </summary>
public class LessThanConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2
            && values[0] is IConvertible v0
            && values[1] is IConvertible v1)
        {
            try
            {
                var a = System.Convert.ToDecimal(v0, culture);
                var b = System.Convert.ToDecimal(v1, culture);
                return a < b;
            }
            catch { }
        }
        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

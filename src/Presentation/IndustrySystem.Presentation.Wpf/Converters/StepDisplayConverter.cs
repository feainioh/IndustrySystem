using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters;

public class StepDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string text && !string.IsNullOrWhiteSpace(text))
            return text;

        if (value is IReadOnlyList<string> names && names.Count > 0)
            return Build(names);

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;

    private static string Build(IReadOnlyList<string> names)
    {
        var valid = names.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (valid.Count == 0) return string.Empty;
        if (valid.Count <= 3) return string.Join("、", valid);
        return $"{string.Join("、", valid.Take(3))} +{valid.Count - 3}";
    }
}

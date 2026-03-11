using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using IndustrySystem.Presentation.Wpf.Resources;

namespace IndustrySystem.Presentation.Wpf.Converters;

public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Enum enumValue) return value;

        var field = enumValue.GetType().GetField(enumValue.ToString());
        var description = field?.GetCustomAttribute<DescriptionAttribute>()?.Description;
        if (string.IsNullOrWhiteSpace(description)) return enumValue.ToString();

        var localized = Strings.ResourceManager.GetString(description, Strings.Culture);
        return string.IsNullOrWhiteSpace(localized) ? description : localized;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}

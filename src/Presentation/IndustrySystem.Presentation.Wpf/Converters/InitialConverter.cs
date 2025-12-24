using System;
using System.Globalization;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Converters
{
    /// <summary>
    /// 将字符串转换为首字母的转换器，用于显示用户头像
    /// </summary>
    public class InitialConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && !string.IsNullOrWhiteSpace(str))
            {
                return str.Substring(0, 1).ToUpper();
            }
            return "?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

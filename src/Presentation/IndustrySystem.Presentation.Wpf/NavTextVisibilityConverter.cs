using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf
{
    public class NavTextVisibilityConverter : IValueConverter
    {
        // Hide text when nav column is compact (<= 60px), show otherwise
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width <= 60 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}

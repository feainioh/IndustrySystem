using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace IndustrySystem.Presentation.Wpf.Views.Controls
{
    public partial class PagingControl : UserControl
    {
        public PagingControl()
        {
            InitializeComponent();
        }

        private TextBox? _jumpBox; // reference after load
        private void JumpBox_Loaded(object sender, RoutedEventArgs e) => _jumpBox = sender as TextBox;

        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register(
                nameof(PageIndex), typeof(int), typeof(PagingControl),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageChanged));

        public int PageSize
        {
            get => (int)GetValue(PageSizeProperty);
            set => SetValue(PageSizeProperty, value);
        }
        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PagingControl),
                new FrameworkPropertyMetadata(20, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageChanged));

        public int TotalCount
        {
            get => (int)GetValue(TotalCountProperty);
            set => SetValue(TotalCountProperty, value);
        }
        public static readonly DependencyProperty TotalCountProperty =
            DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(PagingControl),
                new PropertyMetadata(0, OnPageChanged));

        public System.Windows.Input.ICommand? NextPageCommand
        {
            get => (System.Windows.Input.ICommand?)GetValue(NextPageCommandProperty);
            set => SetValue(NextPageCommandProperty, value);
        }
        public static readonly DependencyProperty NextPageCommandProperty =
            DependencyProperty.Register(nameof(NextPageCommand), typeof(System.Windows.Input.ICommand), typeof(PagingControl));

        public System.Windows.Input.ICommand? PrevPageCommand
        {
            get => (System.Windows.Input.ICommand?)GetValue(PrevPageCommandProperty);
            set => SetValue(PrevPageCommandProperty, value);
        }
        public static readonly DependencyProperty PrevPageCommandProperty =
            DependencyProperty.Register(nameof(PrevPageCommand), typeof(System.Windows.Input.ICommand), typeof(PagingControl));

        public int PageCount => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public int DisplayPageIndex => PageIndex + 1;
        public bool CanPrev => PageIndex > 0;
        public bool CanNext => PageIndex < PageCount - 1;

        private static void OnPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PagingControl pc)
            {
                pc.OnPropertyChanged(nameof(PageCount));
                pc.OnPropertyChanged(nameof(DisplayPageIndex));
                pc.OnPropertyChanged(nameof(CanPrev));
                pc.OnPropertyChanged(nameof(CanNext));
            }
        }

        private void First_Click(object sender, RoutedEventArgs e)
        {
            if (CanPrev) PageIndex = 0;
        }
        private void Last_Click(object sender, RoutedEventArgs e)
        {
            if (CanNext) PageIndex = Math.Max(0, PageCount - 1);
        }
        private void Go_Click(object sender, RoutedEventArgs e)
        {
            if (_jumpBox != null && int.TryParse(_jumpBox.Text, out var p))
            {
                p = Math.Max(1, p); // 1-based
                var last = PageCount < 1 ? 1 : PageCount;
                if (p > last) p = last;
                PageIndex = p - 1;
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
    }

    public class PageInfoMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return string.Empty;
            var current = values[0] is int ci ? ci : 0;
            var totalPages = values[1] is int tp ? tp : 0;
            var totalRecords = values[2] is int tr ? tr : 0;
            return $"µÚ {current}/{totalPages} Ò³ (¹² {totalRecords} Ìõ)";
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => Array.Empty<object>();
    }
}

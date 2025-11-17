using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IndustrySystem.Presentation.Wpf.Controls;

/// <summary>
/// 通用分页条控件。透传 ViewModel 分页属性与命令，避免在每个页面重复 XAML。
/// </summary>
public partial class PagingBar : UserControl
{
 public PagingBar()
 {
 InitializeComponent();
 }

 public static readonly DependencyProperty PagingTextProperty =
 DependencyProperty.Register(nameof(PagingText), typeof(string), typeof(PagingBar));
 public string PagingText { get => (string)GetValue(PagingTextProperty); set => SetValue(PagingTextProperty, value); }

 public static readonly DependencyProperty PageSizesProperty =
 DependencyProperty.Register(nameof(PageSizes), typeof(ObservableCollection<int>), typeof(PagingBar));
 public ObservableCollection<int> PageSizes { get => (ObservableCollection<int>)GetValue(PageSizesProperty); set => SetValue(PageSizesProperty, value); }

 public static readonly DependencyProperty PageSizeProperty =
 DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(PagingBar));
 public int PageSize { get => (int)GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }

 public static readonly DependencyProperty GoToTextProperty =
 DependencyProperty.Register(nameof(GoToText), typeof(string), typeof(PagingBar));
 public string GoToText { get => (string)GetValue(GoToTextProperty); set => SetValue(GoToTextProperty, value); }

 public static readonly DependencyProperty PrevCommandProperty =
 DependencyProperty.Register(nameof(PrevCommand), typeof(ICommand), typeof(PagingBar));
 public ICommand PrevCommand { get => (ICommand)GetValue(PrevCommandProperty); set => SetValue(PrevCommandProperty, value); }

 public static readonly DependencyProperty NextCommandProperty =
 DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(PagingBar));
 public ICommand NextCommand { get => (ICommand)GetValue(NextCommandProperty); set => SetValue(NextCommandProperty, value); }

 public static readonly DependencyProperty GoToCommandProperty =
 DependencyProperty.Register(nameof(GoToCommand), typeof(ICommand), typeof(PagingBar));
 public ICommand GoToCommand { get => (ICommand)GetValue(GoToCommandProperty); set => SetValue(GoToCommandProperty, value); }
}

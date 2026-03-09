using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IndustrySystem.MotionDesigner.Models;
using IndustrySystem.MotionDesigner.ViewModels;

namespace IndustrySystem.MotionDesigner.Controls;

/// <summary>
/// SubProgramTabControl.xaml 的交互逻辑
/// </summary>
public partial class SubProgramTabControl : UserControl
{
    public SubProgramTabControl()
    {
        InitializeComponent();
    }

    #region Dependency Properties

    public static readonly DependencyProperty OpenSubProgramsProperty =
        DependencyProperty.Register(
            nameof(OpenSubPrograms),
            typeof(System.Collections.ObjectModel.ObservableCollection<SubProgram>),
            typeof(SubProgramTabControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActiveSubProgramProperty =
        DependencyProperty.Register(
            nameof(ActiveSubProgram),
            typeof(SubProgram),
            typeof(SubProgramTabControl),
            new PropertyMetadata(null));

    public System.Collections.ObjectModel.ObservableCollection<SubProgram> OpenSubPrograms
    {
        get => (System.Collections.ObjectModel.ObservableCollection<SubProgram>)GetValue(OpenSubProgramsProperty);
        set => SetValue(OpenSubProgramsProperty, value);
    }

    public SubProgram? ActiveSubProgram
    {
        get => (SubProgram?)GetValue(ActiveSubProgramProperty);
        set => SetValue(ActiveSubProgramProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<SubProgram>? TabActivated;
    public event EventHandler<SubProgram>? TabCloseRequested;

    #endregion

    private void OnTabClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is SubProgram subProgram)
        {
            ActiveSubProgram = subProgram;
            TabActivated?.Invoke(this, subProgram);
        }
    }

    private void OnCloseTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is SubProgram subProgram)
        {
            TabCloseRequested?.Invoke(this, subProgram);
        }
    }
}

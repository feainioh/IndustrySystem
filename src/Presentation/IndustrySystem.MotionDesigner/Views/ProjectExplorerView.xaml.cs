using System.Windows;
using System.Windows.Controls;
using IndustrySystem.MotionDesigner.Models;
using IndustrySystem.MotionDesigner.ViewModels;

namespace IndustrySystem.MotionDesigner.Views;

/// <summary>
/// ProjectExplorerView.xaml 的交互逻辑
/// </summary>
public partial class ProjectExplorerView : UserControl
{
    public ProjectExplorerView()
    {
        InitializeComponent();
    }

    private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is ProjectExplorerViewModel vm)
        {
            vm.SelectedItem = e.NewValue;

            // 如果双击了子程序，打开它
            if (e.NewValue is SubProgram subProgram)
            {
                // 单击只是选中，双击才打开（在 XAML 中通过 InputBindings 处理）
            }
        }
    }
}

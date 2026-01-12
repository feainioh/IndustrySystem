using System.Windows;
using System.Windows.Controls;
using IndustrySystem.MotionDesigner.Services;
using IndustrySystem.MotionDesigner.ViewModels;

namespace IndustrySystem.MotionDesigner.Views;

/// <summary>
/// DeviceDebugView.xaml 的交互逻辑
/// </summary>
public partial class DeviceDebugView : UserControl
{
    public DeviceDebugView()
    {
        InitializeComponent();
    }
    
    /// <summary>
    /// TreeView 选择变化事件处理
    /// </summary>
    private void DeviceTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is DeviceDebugViewModel viewModel)
        {
            // 只有当选中的是 DeviceItemViewModel 时才更新（跳过分类节点）
            if (e.NewValue is DeviceItemViewModel deviceItem)
            {
                viewModel.SelectedDevice = deviceItem;
            }
        }
    }
}

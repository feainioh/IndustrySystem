using System.Windows;
using System.Windows.Controls;
using IndustrySystem.MotionDesigner.Controls;
using IndustrySystem.MotionDesigner.ViewModels;

namespace IndustrySystem.MotionDesigner.Views;

public partial class DesignerView : UserControl
{
    public DesignerView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 处理节点选中事件
    /// </summary>
    private void OnNodeSelected(object? sender, ActionNodeViewModel node)
    {
        System.Diagnostics.Debug.WriteLine($"[DesignerView] OnNodeSelected: {node.Name}");
        if (DataContext is DesignerViewModel vm)
        {
            vm.SelectedNode = node;
        }
    }

    /// <summary>
    /// 处理清除选择事件
    /// </summary>
    private void OnSelectionCleared(object? sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[DesignerView] OnSelectionCleared");
        if (DataContext is DesignerViewModel vm)
        {
            vm.ClearSelection();
        }
    }

    /// <summary>
    /// 处理连接创建事件
    /// </summary>
    private void OnConnectionCreated(object? sender, (Guid SourceId, Guid TargetId, ActionNodeControl.PortDirection SourceDir, ActionNodeControl.PortDirection TargetDir) args)
    {
        System.Diagnostics.Debug.WriteLine($"[DesignerView] OnConnectionCreated called!");
        System.Diagnostics.Debug.WriteLine($"[DesignerView]   Source: {args.SourceId}, Target: {args.TargetId}");
        System.Diagnostics.Debug.WriteLine($"[DesignerView]   SourcePort: {args.SourceDir}, TargetPort: {args.TargetDir}");

        if (DataContext is DesignerViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"[DesignerView] Calling vm.AddConnection...");
            vm.AddConnection(args.SourceId, args.TargetId, (int)args.SourceDir, (int)args.TargetDir);
            System.Diagnostics.Debug.WriteLine($"[DesignerView] AddConnection completed");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DesignerView] ERROR: DataContext is not DesignerViewModel!");
        }
    }

    /// <summary>
    /// 处理项目拖放事件
    /// </summary>
    private void OnItemDropped(object? sender, (IDataObject Data, Point Position) args)
    {
        System.Diagnostics.Debug.WriteLine($"[DesignerView] OnItemDropped");
        if (DataContext is DesignerViewModel vm)
        {
            vm.HandleDrop(args.Data, args.Position);
        }
    }

    /// <summary>
    /// 处理连接路径更新请求事件
    /// </summary>
    private void OnConnectionPathsUpdateRequested(object? sender, EventArgs e)
    {
        if (DataContext is DesignerViewModel vm)
        {
            vm.UpdateConnectionPaths();
        }
    }
}

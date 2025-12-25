using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using IndustrySystem.MotionDesigner.ViewModels;

namespace IndustrySystem.MotionDesigner.Controls;

public partial class ActionNodeControl : UserControl
{
    private Point _dragStart;
    private bool _isDragging;

    public ActionNodeControl()
    {
        InitializeComponent();
        
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
    }

    public event EventHandler<Point>? DragStarted;
    public event EventHandler<Point>? DragMoved;
    public event EventHandler<Point>? DragEnded;
    public event EventHandler<(FrameworkElement Port, PortDirection Direction)>? PortClicked;

    public enum PortDirection
    {
        Top,
        Right,
        Bottom,
        Left
    }

    /// <summary>
    /// Handle port click - called directly from XAML
    /// </summary>
    private void OnPortMouseDown(object sender, MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] OnPortMouseDown called, sender: {sender?.GetType().Name}");
        
        if (sender is not FrameworkElement port)
        {
            System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] ERROR: sender is not FrameworkElement");
            return;
        }

        PortDirection direction;
        if (port == TopPort)
            direction = PortDirection.Top;
        else if (port == RightPort)
            direction = PortDirection.Right;
        else if (port == BottomPort)
            direction = PortDirection.Bottom;
        else if (port == LeftPort)
            direction = PortDirection.Left;
        else
        {
            System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] ERROR: Unknown port");
            return;
        }

        System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] Port clicked: {direction}, Node: {(DataContext as dynamic)?.Name}");
        
        // Invoke the event
        PortClicked?.Invoke(this, (port, direction));
        
        // Mark event as handled to prevent node dragging
        e.Handled = true;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // If this is already handled (by port), don't start dragging
        if (e.Handled)
        {
            System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] MouseLeftButtonDown already handled (port click)");
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"[ActionNodeControl] Starting node drag");
        
        // Start dragging
        _isDragging = true;
        _dragStart = e.GetPosition(this.Parent as IInputElement);
        CaptureMouse();
        
        DragStarted?.Invoke(this, _dragStart);
        e.Handled = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        
        var current = e.GetPosition(this.Parent as IInputElement);
        DragMoved?.Invoke(this, current);
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDragging) return;
        
        _isDragging = false;
        ReleaseMouseCapture();
        
        var current = e.GetPosition(this.Parent as IInputElement);
        DragEnded?.Invoke(this, current);
    }

    /// <summary>
    /// Get the center position of a specific port in canvas coordinates.
    /// Robust: finds ancestor Canvas and uses TransformToVisual; falls back to node position if needed.
    /// </summary>
    public Point GetPortCenter(PortDirection direction)
    {
        FrameworkElement port = direction switch
        {
            PortDirection.Top => TopPort,
            PortDirection.Right => RightPort,
            PortDirection.Bottom => BottomPort,
            PortDirection.Left => LeftPort,
            _ => RightPort
        };

        try
        {
            // Find the nearest Canvas ancestor (the drawing surface)
            var canvas = FindAncestor<Canvas>(this);
            if (canvas != null)
            {
                // Ensure layout has measured sizes
                port.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Loaded);

                var w = (port.ActualWidth > 0) ? port.ActualWidth : port.RenderSize.Width;
                var h = (port.ActualHeight > 0) ? port.ActualHeight : port.RenderSize.Height;
                var center = new Point(w / 2.0, h / 2.0);

                var transform = port.TransformToVisual(canvas);
                var result = transform.Transform(center);
                System.Diagnostics.Debug.WriteLine($"[GetPortCenter] Direction: {direction}, CanvasPos: ({result.X:F1},{result.Y:F1})");
                return result;
            }

            // Fallback: use DataContext node coordinates if available
            if (DataContext is ActionNodeViewModel vm)
            {
                var nodeCenter = new Point(vm.X + vm.Width / 2.0, vm.Y + vm.Height / 2.0);
                System.Diagnostics.Debug.WriteLine($"[GetPortCenter] Fallback to node center: ({nodeCenter.X:F1},{nodeCenter.Y:F1})");
                return nodeCenter;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GetPortCenter] EXCEPTION: {ex.Message}");
        }

        return new Point(0, 0);
    }

    // Keep backward compatibility
    public Point GetInputPortCenter() => GetPortCenter(PortDirection.Left);
    public Point GetOutputPortCenter() => GetPortCenter(PortDirection.Right);

    private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject
    {
        var current = start;
        while (current != null)
        {
            if (current is T t) return t;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}

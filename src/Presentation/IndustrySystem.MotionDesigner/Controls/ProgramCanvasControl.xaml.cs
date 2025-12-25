using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using IndustrySystem.MotionDesigner.ViewModels;
using static IndustrySystem.MotionDesigner.Controls.ActionNodeControl;

namespace IndustrySystem.MotionDesigner.Controls;

/// <summary>
/// Program Canvas Control - Handles nodes, connections, minimap, and zoom controls
/// </summary>
public partial class ProgramCanvasControl : UserControl
{
    private Point _selectionStart;
    private bool _isSelecting;
    // Panning
    private bool _isPanning;
    private Point _panStart;
    private double _panStartHOffset;
    private double _panStartVOffset;
    
    // Node dragging
    private ActionNodeViewModel? _draggingNode;
    private Point _dragOffset;
    
    // Connection drawing
    private bool _isDrawingConnection;
    private ActionNodeViewModel? _connectionSourceNode;
    private PortDirection _connectionSourceDirection;
    private ConnectionLineControl? _temporaryConnection;

    // Connection selection
    private ConnectionLineControl? _selectedConnection;
    private readonly List<ConnectionLineControl> _connectionControls = new();

    // Minimap
    private bool _isMinimapDragging;
    private bool _isMinimapVisible = true;
    private const double MinimapScale = 0.05; // 5% of actual size

    public ProgramCanvasControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    #region Dependency Properties

    public static readonly DependencyProperty ZoomLevelProperty =
        DependencyProperty.Register(
            nameof(ZoomLevel), 
            typeof(double), 
            typeof(ProgramCanvasControl),
            new PropertyMetadata(1.0, OnZoomLevelChanged));

    public double ZoomLevel
    {
        get => (double)GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgramCanvasControl control)
        {
            control.UpdateMinimap();
        }
    }

    #endregion

    #region Events

    public event EventHandler<ActionNodeViewModel>? NodeSelected;
    public event EventHandler? SelectionCleared;
    public event EventHandler<(Guid SourceId, Guid TargetId, PortDirection SourceDir, PortDirection TargetDir)>? ConnectionCreated;
    public event EventHandler<Guid>? ConnectionDeleted;
    public event EventHandler<(IDataObject Data, Point Position)>? ItemDropped;
    public event EventHandler? ConnectionPathsUpdateRequested;

    #endregion

    #region Initialization

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== ProgramCanvasControl OnLoaded ===");
            System.Diagnostics.Debug.WriteLine($"DataContext type: {DataContext?.GetType().Name ?? "null"}");
            
            if (DataContext is DesignerViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine($"ProgramCanvasControl loaded. Nodes: {vm.Nodes.Count}, Connections: {vm.Connections.Count}");
                
                // Subscribe to Nodes collection changes
                vm.Nodes.CollectionChanged += (s, args) =>
                {
                    try
                    {
                        if (args.NewItems != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Nodes] Collection changed. New items: {args.NewItems.Count}, Total nodes: {vm.Nodes.Count}");
                            Dispatcher.BeginInvoke(
                                new Action(BindNodeControlEvents), 
                                System.Windows.Threading.DispatcherPriority.Loaded);
                            Dispatcher.BeginInvoke(
                                new Action(UpdateMinimap),
                                System.Windows.Threading.DispatcherPriority.Loaded);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Nodes] Error in collection changed: {ex.Message}");
                    }
                };
                
                // Subscribe to Connections collection changes
                vm.Connections.CollectionChanged += (s, args) =>
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[Connections] Collection changed event fired!");
                        if (args.NewItems != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Connections] New items: {args.NewItems.Count}, Total: {vm.Connections.Count}");
                            foreach (var item in args.NewItems)
                            {
                                if (item is ConnectionViewModel conn)
                                {
                                    System.Diagnostics.Debug.WriteLine($"  Connection: {conn.SourceNodeId} -> {conn.TargetNodeId}");
                                    System.Diagnostics.Debug.WriteLine($"    SourcePort: {conn.SourcePortDirection}, TargetPort: {conn.TargetPortDirection}");
                                    System.Diagnostics.Debug.WriteLine($"    PathData: {(string.IsNullOrEmpty(conn.PathData) ? "EMPTY!" : conn.PathData.Substring(0, Math.Min(50, conn.PathData.Length)))}");
                                }
                            }
                        }
                        
                        // Force UI update
                        Dispatcher.BeginInvoke(
                            System.Windows.Threading.DispatcherPriority.Render,
                            new Action(() =>
                            {
                                System.Diagnostics.Debug.WriteLine($"[Connections] Forcing ConnectionsLayer update. ItemsSource count: {vm.Connections.Count}");
                                if (ConnectionsLayer != null)
                                {
                                    ConnectionsLayer.Items.Refresh();
                                }
                            }));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Connections] Error in collection changed: {ex.Message}\n{ex.StackTrace}");
                    }
                };
                
                Dispatcher.BeginInvoke(
                    new Action(BindNodeControlEvents), 
                    System.Windows.Threading.DispatcherPriority.Loaded);
                Dispatcher.BeginInvoke(
                    new Action(UpdateMinimap),
                    System.Windows.Threading.DispatcherPriority.Loaded);
                    
                System.Diagnostics.Debug.WriteLine("=== ProgramCanvasControl subscriptions complete ===");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR: ProgramCanvasControl DataContext is not DesignerViewModel!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"FATAL ERROR in OnLoaded: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void BindNodeControlEvents()
    {
        var nodeControls = FindVisualChildren<ActionNodeControl>(NodesLayer);
        foreach (var control in nodeControls)
        {
            control.DragStarted -= OnNodeDragStarted;
            control.DragMoved -= OnNodeDragMoved;
            control.DragEnded -= OnNodeDragEnded;
            control.PortClicked -= OnPortClicked;
            
            control.DragStarted += OnNodeDragStarted;
            control.DragMoved += OnNodeDragMoved;
            control.DragEnded += OnNodeDragEnded;
            control.PortClicked += OnPortClicked;
        }
    }

    #endregion

    #region Node Drag Handling

    private void OnNodeDragStarted(object? sender, Point position)
    {
        if (sender is ActionNodeControl control && control.DataContext is ActionNodeViewModel node)
        {
            _draggingNode = node;
            _dragOffset = new Point(position.X - node.X, position.Y - node.Y);
            NodeSelected?.Invoke(this, node);
        }
    }

    private void OnNodeDragMoved(object? sender, Point position)
    {
        if (_draggingNode != null)
        {
            _draggingNode.X = Math.Max(0, Math.Min(2820, position.X - _dragOffset.X));
            _draggingNode.Y = Math.Max(0, Math.Min(2940, position.Y - _dragOffset.Y));
            
            ConnectionPathsUpdateRequested?.Invoke(this, EventArgs.Empty);
            UpdateMinimap();
        }
    }

    private void OnNodeDragEnded(object? sender, Point position)
    {
        _draggingNode = null;
    }

    #endregion

    #region Connection Drawing

    private void OnPortClicked(object? sender, (FrameworkElement Port, PortDirection Direction) args)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Port direction: {args.Direction}");
            
            if (sender is not ActionNodeControl control || control.DataContext is not ActionNodeViewModel node)
            {
                System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] ERROR: Invalid sender or DataContext");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Node: {node.Name} ({node.Id})");
            System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Currently drawing: {_isDrawingConnection}");

            if (!_isDrawingConnection)
            {
                // Start drawing connection from this port
                System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Starting connection from node {node.Name}");
                StartDrawingConnection(node, control, args.Direction);
            }
            else
            {
                // Complete connection to this port (only if different node)
                System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Trying to complete connection");
                System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] Source node: {_connectionSourceNode?.Name}, Target node: {node.Name}");
                
                if (_connectionSourceNode != null && _connectionSourceNode.Id != node.Id)
                {
                    System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] ? Completing connection!");
                    CompleteConnection(node, args.Direction);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] ? Cannot connect to same node or source is null");
                }
                CancelDrawingConnection();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PORT CLICKED] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void StartDrawingConnection(ActionNodeViewModel sourceNode, ActionNodeControl sourceControl, PortDirection direction)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[START CONNECTION] Source: {sourceNode.Name}, Direction: {direction}");
            
            _isDrawingConnection = true;
            _connectionSourceNode = sourceNode;
            _connectionSourceDirection = direction;
            
            var startPoint = sourceControl.GetPortCenter(direction);
            System.Diagnostics.Debug.WriteLine($"[START CONNECTION] Start point: ({startPoint.X}, {startPoint.Y})");
            
            // Create temporary connection line control
            _temporaryConnection = new ConnectionLineControl
            {
                StartPoint = startPoint,
                EndPoint = startPoint,
                Stroke = Brushes.Gray,
                StrokeThickness = 2,
                IsTemporary = true,
                IsHitTestVisible = false
            };
            
            // Add to canvas
            DesignerCanvas.Children.Add(_temporaryConnection);
            System.Diagnostics.Debug.WriteLine($"[START CONNECTION] Temporary connection created and added to canvas");
            
            // Capture mouse for tracking
            DesignerCanvas.CaptureMouse();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[START CONNECTION] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        // Handle panning first
        if (_isPanning)
        {
            var pos = e.GetPosition(this);
            var dx = pos.X - _panStart.X;
            var dy = pos.Y - _panStart.Y;

            // Inverse movement: dragging left moves viewport right
            MainScrollViewer?.ScrollToHorizontalOffset(Math.Max(0, _panStartHOffset - dx));
            MainScrollViewer?.ScrollToVerticalOffset(Math.Max(0, _panStartVOffset - dy));
            return;
        }
        
        if (_isDrawingConnection && _temporaryConnection != null)
        {
            var position = e.GetPosition(DesignerCanvas);
            _temporaryConnection.EndPoint = position;
        }
    }

    private void OnCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDrawingConnection)
        {
            var position = e.GetPosition(DesignerCanvas);
            var hitElement = DesignerCanvas.InputHitTest(position) as DependencyObject;
            
            if (hitElement != null)
            {
                var nodeControl = FindParent<ActionNodeControl>(hitElement);
                if (nodeControl?.DataContext is ActionNodeViewModel targetNode && 
                    _connectionSourceNode != null && 
                    targetNode.Id != _connectionSourceNode.Id)
                {
                    // Determine which port was clicked based on position
                    var targetDirection = DeterminePortDirection(nodeControl, position);
                    CompleteConnection(targetNode, targetDirection);
                }
            }
            
            CancelDrawingConnection();
        }
        
        if (_isSelecting)
        {
            _isSelecting = false;
        }
        
        if (_isPanning)
        {
            _isPanning = false;
            DesignerCanvas.ReleaseMouseCapture();
        }
    }

    private PortDirection DeterminePortDirection(ActionNodeControl control, Point clickPosition)
    {
        // Simple heuristic: check which port is closest to click position
        var distances = new[]
        {
            (PortDirection.Top, GetDistance(control.GetPortCenter(PortDirection.Top), clickPosition)),
            (PortDirection.Right, GetDistance(control.GetPortCenter(PortDirection.Right), clickPosition)),
            (PortDirection.Bottom, GetDistance(control.GetPortCenter(PortDirection.Bottom), clickPosition)),
            (PortDirection.Left, GetDistance(control.GetPortCenter(PortDirection.Left), clickPosition))
        };
        
        return distances.OrderBy(d => d.Item2).First().Item1;
    }

    private double GetDistance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    private void CompleteConnection(ActionNodeViewModel targetNode, PortDirection targetDirection)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] Source: {_connectionSourceNode?.Name}, Target: {targetNode.Name}");
            System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] Source Port: {_connectionSourceDirection}, Target Port: {targetDirection}");
            
            if (_connectionSourceNode != null)
            {
                System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] Invoking ConnectionCreated event...");
                ConnectionCreated?.Invoke(this, (
                    _connectionSourceNode.Id, 
                    targetNode.Id,
                    _connectionSourceDirection,
                    targetDirection
                ));
                System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] Event invoked!");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] ERROR: Source node is null!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[COMPLETE CONNECTION] EXCEPTION: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void CancelDrawingConnection()
    {
        System.Diagnostics.Debug.WriteLine($"[CANCEL CONNECTION] Cancelling connection drawing");
        
        // Remove temporary connection from canvas
        if (_temporaryConnection != null)
        {
            DesignerCanvas.Children.Remove(_temporaryConnection);
            _temporaryConnection = null;
        }
        
        _isDrawingConnection = false;
        _connectionSourceNode = null;
        DesignerCanvas.ReleaseMouseCapture();
    }

    #endregion

    #region Canvas Interaction

    private void OnCanvasDrop(object sender, DragEventArgs e)
    {
        var position = e.GetPosition(DesignerCanvas);
        ItemDropped?.Invoke(this, (e.Data, position));
        
        Dispatcher.BeginInvoke(
            new Action(BindNodeControlEvents), 
            System.Windows.Threading.DispatcherPriority.Loaded);
        Dispatcher.BeginInvoke(
            new Action(UpdateMinimap),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void OnCanvasDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
        e.Handled = true;
    }

    private void OnCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == DesignerCanvas || 
            (e.OriginalSource is Rectangle rect && rect.IsHitTestVisible == false))
        {
            SelectionCleared?.Invoke(this, EventArgs.Empty);
            ClearConnectionSelection(); // Clear connection selection (includes VM highlights)

            // If no node is selected in the DataContext, start panning the canvas
            if (DataContext is DesignerViewModel vm && vm.SelectedNode == null)
            {
                _isPanning = true;
                _panStart = e.GetPosition(this);
                _panStartHOffset = MainScrollViewer?.HorizontalOffset ?? 0;
                _panStartVOffset = MainScrollViewer?.VerticalOffset ?? 0;
                DesignerCanvas.CaptureMouse();
            }
            else
            {
                _selectionStart = e.GetPosition(DesignerCanvas);
                _isSelecting = true;
            }
        }
    }

    private void OnConnectionPathMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Find the ConnectionViewModel bound to this Path
        if (sender is DependencyObject dep)
        {
            var container = FindParent<ContentPresenter>(dep);
            if (container?.DataContext is ConnectionViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine($"[UI] Connection path clicked: {vm.Id}");
                if (DataContext is DesignerViewModel dvm)
                {
                    // Clear previous highlights in VM
                    foreach (var c in dvm.Connections)
                        c.IsHighlighted = false;

                    // Highlight this one
                    vm.IsHighlighted = true;
                }

                // Also clear any registered ConnectionLineControl selections
                foreach (var conn in _connectionControls)
                    conn.Deselect();

                // Ensure keyboard focus so Delete key works
                Keyboard.Focus(this);
                e.Handled = true;
            }
        }
    }

    private void OnConnectionPathMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is DependencyObject dep)
        {
            var container = FindParent<ContentPresenter>(dep);
            if (container?.DataContext is ConnectionViewModel vm)
            {
                System.Diagnostics.Debug.WriteLine($"[UI] Connection path right-click: {vm.Id}");
                var menu = new ContextMenu();
                var item = new MenuItem { Header = "Delete Connection" };
                item.Click += (s, args) =>
                {
                    // Raise delete event to be handled by parent (DesignerView)
                    ConnectionDeleted?.Invoke(this, vm.Id);
                };
                menu.Items.Add(item);
                menu.IsOpen = true;
                e.Handled = true;
            }
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e == null) return;
        if (e.Key == Key.Delete)
        {
            // If we have a registered UI connection selected, delete it
            if (_selectedConnection != null)
            {
                System.Diagnostics.Debug.WriteLine($"[KEY] Delete pressed - deleting selected connection (UI): {_selectedConnection.ConnectionId}");
                DeleteSelectedConnection();
                e.Handled = true;
                return;
            }
            
            // Otherwise delete highlighted connection in ViewModel (VM-driven mode)
            if (DataContext is DesignerViewModel dvm)
            {
                var highlighted = dvm.Connections.FirstOrDefault(c => c.IsHighlighted);
                if (highlighted != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[KEY] Delete pressed - deleting highlighted VM connection: {highlighted.Id}");
                    ConnectionDeleted?.Invoke(this, highlighted.Id);
                    e.Handled = true;
                }
            }
        }
    }

    #endregion

    #region Minimap

    private void UpdateMinimap()
    {
        if (DataContext is not DesignerViewModel vm || !_isMinimapVisible) return;

        MinimapCanvas.Children.Clear();
        
        // Draw nodes on minimap
        foreach (var node in vm.Nodes)
        {
            var rect = new Rectangle
            {
                Width = node.Width * MinimapScale,
                Height = node.Height * MinimapScale,
                Fill = node.IsSelected ? Brushes.Blue : Brushes.Gray,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5
            };
            Canvas.SetLeft(rect, node.X * MinimapScale);
            Canvas.SetTop(rect, node.Y * MinimapScale);
            MinimapCanvas.Children.Add(rect);
        }
        
        // Update viewport rectangle
        UpdateViewportRect();
    }

    private void UpdateViewportRect()
    {
        var scrollViewer = MainScrollViewer;
        if (scrollViewer == null) return;

        var viewportWidth = scrollViewer.ViewportWidth / ZoomLevel * MinimapScale;
        var viewportHeight = scrollViewer.ViewportHeight / ZoomLevel * MinimapScale;
        var offsetX = scrollViewer.HorizontalOffset / ZoomLevel * MinimapScale;
        var offsetY = scrollViewer.VerticalOffset / ZoomLevel * MinimapScale;

        ViewportRect.Width = viewportWidth;
        ViewportRect.Height = viewportHeight;
        Canvas.SetLeft(ViewportRect, offsetX);
        Canvas.SetTop(ViewportRect, offsetY);
        
        if (!MinimapCanvas.Children.Contains(ViewportRect))
        {
            MinimapCanvas.Children.Add(ViewportRect);
        }
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        UpdateViewportRect();
    }

    private void OnMinimapMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isMinimapDragging = true;
        MinimapCanvas.CaptureMouse();
        OnMinimapMouseMove(sender, e);
    }

    private void OnMinimapMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMinimapDragging && e.LeftButton != MouseButtonState.Pressed) return;

        var pos = e.GetPosition(MinimapCanvas);
        var scrollX = (pos.X / MinimapScale) * ZoomLevel - MainScrollViewer.ViewportWidth / 2;
        var scrollY = (pos.Y / MinimapScale) * ZoomLevel - MainScrollViewer.ViewportHeight / 2;

        MainScrollViewer.ScrollToHorizontalOffset(Math.Max(0, scrollX));
        MainScrollViewer.ScrollToVerticalOffset(Math.Max(0, scrollY));
    }

    private void OnToggleMinimapClick(object sender, RoutedEventArgs e)
    {
        _isMinimapVisible = !_isMinimapVisible;
        MinimapBorder.Visibility = _isMinimapVisible ? Visibility.Visible : Visibility.Collapsed;
        
        if (ToggleMinimapButton.Content is MaterialDesignThemes.Wpf.PackIcon icon)
        {
            icon.Kind = _isMinimapVisible ? MaterialDesignThemes.Wpf.PackIconKind.EyeOff : MaterialDesignThemes.Wpf.PackIconKind.Eye;
        }
    }

    #endregion

    #region Zoom Controls

    private void OnZoomInClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is DesignerViewModel vm)
        {
            vm.ZoomLevel += 0.1;
        }
    }

    private void OnZoomOutClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is DesignerViewModel vm)
        {
            vm.ZoomLevel -= 0.1;
        }
    }

    private void OnZoomResetClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is DesignerViewModel vm)
        {
            vm.ZoomLevel = 1.0;
        }
    }

    private void OnZoomFitClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is DesignerViewModel vm && vm.Nodes.Count > 0)
        {
            var minX = vm.Nodes.Min(n => n.X);
            var minY = vm.Nodes.Min(n => n.Y);
            var maxX = vm.Nodes.Max(n => n.X + n.Width);
            var maxY = vm.Nodes.Max(n => n.Y + n.Height);
            
            var viewWidth = MainScrollViewer.ViewportWidth;
            var viewHeight = MainScrollViewer.ViewportHeight;
            var padding = 50.0;
            
            var contentWidth = maxX - minX + padding * 2;
            var contentHeight = maxY - minY + padding * 2;
            
            var scaleX = viewWidth / contentWidth;
            var scaleY = viewHeight / contentHeight;
            
            vm.ZoomLevel = Math.Min(scaleX, scaleY);
        }
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) yield break;
        
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
                yield return t;
            
            foreach (var childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    #endregion

    #region Connection Management

    /// <summary>
    /// Register a connection control for selection management
    /// </summary>
    public void RegisterConnection(ConnectionLineControl connection)
    {
        if (!_connectionControls.Contains(connection))
        {
            _connectionControls.Add(connection);
            connection.Selected += OnConnectionSelected;
            connection.DeleteRequested += OnConnectionDeleteRequested;
        }
    }

    /// <summary>
    /// Unregister a connection control
    /// </summary>
    public void UnregisterConnection(ConnectionLineControl connection)
    {
        if (_connectionControls.Contains(connection))
        {
            connection.Selected -= OnConnectionSelected;
            connection.DeleteRequested -= OnConnectionDeleteRequested;
            _connectionControls.Remove(connection);
        }
    }

    /// <summary>
    /// Handle connection selection
    /// </summary>
    private void OnConnectionSelected(object? sender, EventArgs e)
    {
        if (sender is not ConnectionLineControl selected)
            return;

        // Deselect all other connections
        foreach (var conn in _connectionControls)
        {
            if (conn != selected)
            {
                conn.Deselect();
            }
        }

        _selectedConnection = selected;
        System.Diagnostics.Debug.WriteLine($"[CONNECTION SELECTED] ID: {selected.ConnectionId}");
    }

    /// <summary>
    /// Handle connection delete request
    /// </summary>
    private void OnConnectionDeleteRequested(object? sender, EventArgs e)
    {
        if (sender is not ConnectionLineControl connection)
            return;

        System.Diagnostics.Debug.WriteLine($"[CONNECTION DELETE] Requesting delete for: {connection.ConnectionId}");
        
        // Remove from canvas
        DesignerCanvas.Children.Remove(connection);
        UnregisterConnection(connection);

        // Clear selection if this was selected
        if (_selectedConnection == connection)
        {
            _selectedConnection = null;
        }

        // Notify parent
        ConnectionDeleted?.Invoke(this, connection.ConnectionId);
    }

    /// <summary>
    /// Clear all connection selections
    /// </summary>
    public void ClearConnectionSelection()
    {
        foreach (var conn in _connectionControls)
        {
            conn.Deselect();
        }
        _selectedConnection = null;
        // Clear VM highlights as well
        if (DataContext is DesignerViewModel dvm)
        {
            foreach (var c in dvm.Connections)
                c.IsHighlighted = false;
        }
    }

    /// <summary>
    /// Delete selected connection
    /// </summary>
    public void DeleteSelectedConnection()
    {
        if (_selectedConnection != null)
        {
            OnConnectionDeleteRequested(_selectedConnection, EventArgs.Empty);
        }
    }

    #endregion

    private void OnCanvasMouseWheel(object sender, MouseWheelEventArgs e)
    {

    }
}

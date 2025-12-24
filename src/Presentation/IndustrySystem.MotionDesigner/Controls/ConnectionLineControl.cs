using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace IndustrySystem.MotionDesigner.Controls;

/// <summary>
/// Connection Line Control - Represents a connection between two nodes
/// </summary>
public class ConnectionLineControl : Canvas
{
    private Path? _connectionPath;
    private Path? _arrowPath;
    private Path? _hitTestPath; // 用于鼠标交互的透明路径

    public ConnectionLineControl()
    {
        // Create visual elements
        _connectionPath = new Path
        {
            Fill = Brushes.Transparent
        };

        _arrowPath = new Path();

        // Create hit test path (wider transparent path for easier clicking)
        _hitTestPath = new Path
        {
            Fill = Brushes.Transparent,
            Stroke = Brushes.Transparent,
            StrokeThickness = 10, // Wider for easier selection
            Cursor = Cursors.Hand
        };

        // Add paths (hit test path first for proper layering)
        Children.Add(_hitTestPath);
        Children.Add(_connectionPath);
        Children.Add(_arrowPath);

        // Bind events
        Loaded += OnLoaded;
        _hitTestPath.MouseEnter += OnMouseEnter;
        _hitTestPath.MouseLeave += OnMouseLeave;
        _hitTestPath.MouseLeftButtonDown += OnMouseLeftButtonDown;
        _hitTestPath.MouseRightButtonDown += OnMouseRightButtonDown;
    }

    #region Events

    /// <summary>
    /// Raised when the connection is selected
    /// </summary>
    public event EventHandler? Selected;

    /// <summary>
    /// Raised when delete is requested
    /// </summary>
    public event EventHandler? DeleteRequested;

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty StartPointProperty =
        DependencyProperty.Register(
            nameof(StartPoint),
            typeof(Point),
            typeof(ConnectionLineControl),
            new PropertyMetadata(new Point(0, 0), OnPointsChanged));

    public static readonly DependencyProperty EndPointProperty =
        DependencyProperty.Register(
            nameof(EndPoint),
            typeof(Point),
            typeof(ConnectionLineControl),
            new PropertyMetadata(new Point(0, 0), OnPointsChanged));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(
            nameof(Stroke),
            typeof(Brush),
            typeof(ConnectionLineControl),
            new PropertyMetadata(Brushes.Blue, OnStrokeChanged));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(ConnectionLineControl),
            new PropertyMetadata(2.0, OnStrokeChanged));

    public static readonly DependencyProperty IsTemporaryProperty =
        DependencyProperty.Register(
            nameof(IsTemporary),
            typeof(bool),
            typeof(ConnectionLineControl),
            new PropertyMetadata(false, OnTemporaryChanged));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(ConnectionLineControl),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty ConnectionIdProperty =
        DependencyProperty.Register(
            nameof(ConnectionId),
            typeof(Guid),
            typeof(ConnectionLineControl),
            new PropertyMetadata(Guid.Empty));

    public Point StartPoint
    {
        get => (Point)GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public Point EndPoint
    {
        get => (Point)GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public Brush Stroke
    {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public bool IsTemporary
    {
        get => (bool)GetValue(IsTemporaryProperty);
        set => SetValue(IsTemporaryProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public Guid ConnectionId
    {
        get => (Guid)GetValue(ConnectionIdProperty);
        set => SetValue(ConnectionIdProperty, value);
    }

    #endregion

    #region Event Handlers

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdatePathGeometry();
    }

    private void OnMouseEnter(object? sender, MouseEventArgs e)
    {
        if (!IsTemporary && !IsSelected && _connectionPath != null)
        {
            // Highlight on hover
            _connectionPath.StrokeThickness = StrokeThickness + 1;
            _connectionPath.Opacity = 0.8;
        }
    }

    private void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        if (!IsSelected && _connectionPath != null)
        {
            // Remove highlight
            _connectionPath.StrokeThickness = StrokeThickness;
            _connectionPath.Opacity = 1.0;
        }
    }

    private void OnMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!IsTemporary)
        {
            IsSelected = true;
            Selected?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }

    private void OnMouseRightButtonDown(object? sender, MouseButtonEventArgs e)
    {
        if (!IsTemporary)
        {
            // Show context menu or delete
            var contextMenu = new ContextMenu();
            
            var deleteItem = new MenuItem
            {
                Header = "Delete Connection",
                Icon = new System.Windows.Controls.Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri("pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesignTheme.Defaults.xaml"))
                }
            };
            deleteItem.Click += (s, args) =>
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
            };
            
            contextMenu.Items.Add(deleteItem);
            contextMenu.IsOpen = true;
            contextMenu.PlacementTarget = this;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            
            e.Handled = true;
        }
    }

    #endregion

    #region Property Changed Handlers

    private static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionLineControl control)
        {
            control.UpdatePathGeometry();
        }
    }

    private static void OnStrokeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionLineControl control)
        {
            control.UpdateStrokeProperties();
        }
    }

    private static void OnTemporaryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionLineControl control)
        {
            control.UpdateTemporaryStyle();
        }
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionLineControl control)
        {
            control.UpdateSelectionStyle();
        }
    }

    #endregion

    #region Update Methods

    private void UpdateStrokeProperties()
    {
        if (_connectionPath != null)
        {
            _connectionPath.Stroke = IsSelected ? Brushes.Orange : Stroke;
            _connectionPath.StrokeThickness = IsSelected ? StrokeThickness + 2 : StrokeThickness;
        }
        if (_arrowPath != null)
        {
            _arrowPath.Fill = IsSelected ? Brushes.Orange : Stroke;
        }
    }

    private void UpdateTemporaryStyle()
    {
        if (_connectionPath != null)
        {
            _connectionPath.StrokeDashArray = IsTemporary ? new DoubleCollection { 4, 2 } : null;
        }
    }

    private void UpdateSelectionStyle()
    {
        if (_connectionPath != null)
        {
            if (IsSelected)
            {
                _connectionPath.Stroke = Brushes.Orange;
                _connectionPath.StrokeThickness = StrokeThickness + 3;
                
                if (_arrowPath != null)
                {
                    _arrowPath.Fill = Brushes.Orange;
                }
            }
            else
            {
                _connectionPath.Stroke = Stroke;
                _connectionPath.StrokeThickness = StrokeThickness;
                
                if (_arrowPath != null)
                {
                    _arrowPath.Fill = Stroke;
                }
            }
        }
    }

    private void UpdatePathGeometry()
    {
        if (_connectionPath == null) return;

        var start = StartPoint;
        var end = EndPoint;

        // Calculate control points for smooth Bezier curve
        var controlPointOffset = Math.Max(Math.Abs(end.X - start.X) / 2, 50);
        var cp1 = new Point(start.X + controlPointOffset, start.Y);
        var cp2 = new Point(end.X - controlPointOffset, end.Y);

        // Create Bezier curve path
        var pathFigure = new PathFigure { StartPoint = start };
        pathFigure.Segments.Add(new BezierSegment(cp1, cp2, end, true));

        var pathGeometry = new PathGeometry();
        pathGeometry.Figures.Add(pathFigure);

        // Update all paths with same geometry
        _connectionPath.Data = pathGeometry;
        _connectionPath.Stroke = IsSelected ? Brushes.Orange : Stroke;
        _connectionPath.StrokeThickness = IsSelected ? StrokeThickness + 2 : StrokeThickness;

        if (_hitTestPath != null)
        {
            _hitTestPath.Data = pathGeometry;
        }

        // Update arrow
        if (_arrowPath != null)
        {
            UpdateArrowGeometry(end, cp2);
        }

        // Update styles
        UpdateTemporaryStyle();
    }

    private void UpdateArrowGeometry(Point endPoint, Point controlPoint)
    {
        if (_arrowPath == null) return;

        // Calculate arrow direction
        var dx = endPoint.X - controlPoint.X;
        var dy = endPoint.Y - controlPoint.Y;
        var angle = Math.Atan2(dy, dx);

        // Arrow size
        var arrowSize = IsSelected ? 10.0 : 8.0;

        // Calculate arrow points
        var p1 = new Point(
            endPoint.X - arrowSize * Math.Cos(angle - Math.PI / 6),
            endPoint.Y - arrowSize * Math.Sin(angle - Math.PI / 6));

        var p2 = new Point(
            endPoint.X - arrowSize * Math.Cos(angle + Math.PI / 6),
            endPoint.Y - arrowSize * Math.Sin(angle + Math.PI / 6));

        // Create arrow path
        var pathFigure = new PathFigure { StartPoint = endPoint };
        pathFigure.Segments.Add(new LineSegment(p1, true));
        pathFigure.Segments.Add(new LineSegment(p2, true));
        pathFigure.IsClosed = true;

        var pathGeometry = new PathGeometry();
        pathGeometry.Figures.Add(pathFigure);

        _arrowPath.Data = pathGeometry;
        _arrowPath.Fill = IsSelected ? Brushes.Orange : Stroke;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Deselect this connection
    /// </summary>
    public void Deselect()
    {
        IsSelected = false;
    }

    #endregion
}

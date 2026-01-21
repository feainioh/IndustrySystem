using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Events;
using IndustrySystem.MotionDesigner.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

public class ActionToolItem
{
    public ActionType ActionType { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string IconKind { get; set; } = string.Empty;
}


public class DesignerViewModel : BindableBase
{
    private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IMotionProgramAppService _programService;
    private readonly IMotionProgramExecutor _executor;
    private readonly IEventAggregator _eventAggregator;
    private readonly IDeviceConfigService _configService;

    private MotionProgramDto? _currentProgram;
    private DeviceConfigDto? _currentConfig;
    private ActionNodeViewModel? _selectedNode;
    private ConnectionViewModel? _selectedConnection;
    private string _executionState = "Idle";
    private double _executionProgress;
    private bool _isRunning;
    private bool _hasSelectedNode;

    // 缩放相关
    private double _zoomLevel = 1.0;
    private const double MinZoom = 0.25;
    private const double MaxZoom = 3.0;
    private const double ZoomStep = 0.1;

    public ObservableCollection<ActionNodeViewModel> Nodes { get; } = new();
    public ObservableCollection<ConnectionViewModel> Connections { get; } = new();

    public ObservableCollection<ActionToolItem> MotorActions { get; } = new();
    public ObservableCollection<ActionToolItem> IoActions { get; } = new();
    public ObservableCollection<ActionToolItem> RobotActions { get; } = new();
    public ObservableCollection<ActionToolItem> FlowActions { get; } = new();
    public ObservableCollection<ActionToolItem> LogicActions { get; } = new();

    public Array ErrorHandlingOptions => Enum.GetValues(typeof(ErrorHandling));

    public ActionNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode != null) _selectedNode.IsSelected = false;
            if (SetProperty(ref _selectedNode, value))
            {
                if (_selectedNode != null)
                {
                    _selectedNode.IsSelected = true;
                    // When a node is selected, clear any selected connection
                    if (_selectedConnection != null)
                    {
                        _selectedConnection.IsHighlighted = false;
                        _selectedConnection = null;
                        RaisePropertyChanged(nameof(SelectedLine));
                    }
                }
                HasSelectedNode = _selectedNode != null;
            }
        }
    }

    public ConnectionViewModel? SelectedLine
    {
        get => _selectedConnection;
        set
        {
            if (_selectedConnection != null)
            {
                _selectedConnection.IsHighlighted = false;
            }

            if (SetProperty(ref _selectedConnection, value))
            {
                if (_selectedConnection != null)
                {
                    // Ensure only one connection is highlighted
                    foreach (var c in Connections)
                    {
                        if (c != _selectedConnection)
                            c.IsHighlighted = false;
                    }

                    _selectedConnection.IsHighlighted = true;

                    // When a connection is selected, clear node selection
                    if (_selectedNode != null)
                    {
                        _selectedNode.IsSelected = false;
                        _selectedNode = null;
                        HasSelectedNode = false;
                        RaisePropertyChanged(nameof(SelectedNode));
                    }
                }
            }
        }
    }
    public bool HasSelectedNode { get => _hasSelectedNode; set => SetProperty(ref _hasSelectedNode, value); }
    public string ExecutionState { get => _executionState; set => SetProperty(ref _executionState, value); }
    public double ExecutionProgress { get => _executionProgress; set => SetProperty(ref _executionProgress, value); }
    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }

    // 缩放属性
    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            if (SetProperty(ref _zoomLevel, Math.Clamp(value, MinZoom, MaxZoom)))
            {
                RaisePropertyChanged(nameof(ZoomPercentage));
            }
        }
    }

    public string ZoomPercentage => $"{ZoomLevel * 100:F0}%";

    public ICommand NewProgramCommand { get; }
    public ICommand OpenProgramCommand { get; }
    public ICommand SaveProgramCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand AddNodeCommand { get; }
    public ICommand DeleteSelectedCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand StepCommand { get; }
    public ICommand ImportConfigCommand { get; } // 新增

    // 缩放命令
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ZoomResetCommand { get; }
    public ICommand ZoomFitCommand { get; }

    public DesignerViewModel(
        IMotionProgramAppService programService, 
        IMotionProgramExecutor executor,
        IEventAggregator eventAggregator,
        IDeviceConfigService configService)
    {
        _programService = programService;
        _executor = executor;
        _eventAggregator = eventAggregator;
        _configService = configService;

        NewProgramCommand = new DelegateCommand(NewProgram);
        OpenProgramCommand = new DelegateCommand(async () => await OpenProgramAsync());
        SaveProgramCommand = new DelegateCommand(async () => await SaveProgramAsync());
        ExportCommand = new DelegateCommand(async () => await ExportToJsonAsync());
        AddNodeCommand = new DelegateCommand<ActionToolItem>(AddNode);
        DeleteSelectedCommand = new DelegateCommand(DeleteSelected);
        ClearAllCommand = new DelegateCommand(ClearAll);
        RunCommand = new DelegateCommand(async () => await RunProgramAsync());
        PauseCommand = new DelegateCommand(async () => await _executor.PauseAsync());
        StopCommand = new DelegateCommand(async () => await _executor.StopAsync());
        StepCommand = new DelegateCommand(async () => await _executor.StepAsync());
        ImportConfigCommand = new DelegateCommand(ImportConfig); // 新增

        // 初始化缩放命令
        ZoomInCommand = new DelegateCommand(() => ZoomLevel += ZoomStep);
        ZoomOutCommand = new DelegateCommand(() => ZoomLevel -= ZoomStep);
        ZoomResetCommand = new DelegateCommand(() => ZoomLevel = 1.0);
        ZoomFitCommand = new DelegateCommand(FitToPage);

        InitializeToolbox();

        _executor.ProgressChanged += OnProgressChanged;
        _executor.NodeExecuted += OnNodeExecuted;
        _executor.ProgramCompleted += OnProgramCompleted;
        
        // 订阅事件
        _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Subscribe(OnConfigImported);
        _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Subscribe(OnConfigCreated);
        _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Subscribe(OnConfigLoaded);
        _eventAggregator.GetEvent<PositionUpdatedEvent>().Subscribe(OnPositionUpdated);
        _eventAggregator.GetEvent<PositionAddedEvent>().Subscribe(OnPositionAdded);
        _eventAggregator.GetEvent<PositionDeletedEvent>().Subscribe(OnPositionDeleted);
        _eventAggregator.GetEvent<DeviceAddedEvent>().Subscribe(OnDeviceAdded);
    }

    private void InitializeToolbox()
    {
        MotorActions.Add(new ActionToolItem { ActionType = ActionType.MotorMoveAbsolute, DisplayName = "Motor Absolute", IconKind = "AxisArrow" });
        MotorActions.Add(new ActionToolItem { ActionType = ActionType.MotorMoveRelative, DisplayName = "Motor Relative", IconKind = "AxisArrowInfo" });
        MotorActions.Add(new ActionToolItem { ActionType = ActionType.MotorHome, DisplayName = "Motor Home", IconKind = "Home" });
        MotorActions.Add(new ActionToolItem { ActionType = ActionType.MotorStop, DisplayName = "Motor Stop", IconKind = "Stop" });
        MotorActions.Add(new ActionToolItem { ActionType = ActionType.WaitMotorDone, DisplayName = "Wait Done", IconKind = "TimerSand" });

        IoActions.Add(new ActionToolItem { ActionType = ActionType.IoOutput, DisplayName = "IO Output", IconKind = "ExportVariant" });
        IoActions.Add(new ActionToolItem { ActionType = ActionType.WaitIoInput, DisplayName = "Wait IO Input", IconKind = "ImportVariant" });

        RobotActions.Add(new ActionToolItem { ActionType = ActionType.RobotMoveTo, DisplayName = "Robot Move", IconKind = "Robot" });
        RobotActions.Add(new ActionToolItem { ActionType = ActionType.RobotRunProgram, DisplayName = "Run Program", IconKind = "RobotIndustrial" });

        // Basic Flow Control
        FlowActions.Add(new ActionToolItem { ActionType = ActionType.Delay, DisplayName = "Delay", IconKind = "ClockOutline" });
        FlowActions.Add(new ActionToolItem { ActionType = ActionType.SetVariable, DisplayName = "Set Variable", IconKind = "Variable" });
        FlowActions.Add(new ActionToolItem { ActionType = ActionType.Log, DisplayName = "Log", IconKind = "TextBoxOutline" });
        FlowActions.Add(new ActionToolItem { ActionType = ActionType.Alarm, DisplayName = "Alarm", IconKind = "AlertCircle" });
        FlowActions.Add(new ActionToolItem { ActionType = ActionType.CallSubProgram, DisplayName = "Call SubProgram", IconKind = "FunctionVariant" });

        // Logic Control - If/Else
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.IfStart, DisplayName = "If", IconKind = "CodeBraces" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.ElseIf, DisplayName = "Else If", IconKind = "CodeBrackets" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Else, DisplayName = "Else", IconKind = "CodeBrackets" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.IfEnd, DisplayName = "End If", IconKind = "CodeBraces" });

        // Logic Control - Loops
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.WhileStart, DisplayName = "While", IconKind = "Sync" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.WhileEnd, DisplayName = "End While", IconKind = "SyncOff" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.LoopStart, DisplayName = "Loop Start", IconKind = "Repeat" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.LoopEnd, DisplayName = "Loop End", IconKind = "RepeatOff" });

        // Logic Control - Flow
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Break, DisplayName = "Break", IconKind = "ExitRun" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Continue, DisplayName = "Continue", IconKind = "SkipNext" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Return, DisplayName = "Return", IconKind = "KeyboardReturn" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Condition, DisplayName = "Condition", IconKind = "HelpRhombus" });

        // Logic Control - Switch
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Switch, DisplayName = "Switch", IconKind = "FormatListBulleted" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Case, DisplayName = "Case", IconKind = "CheckboxMarked" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.Default, DisplayName = "Default", IconKind = "CheckboxBlankOutline" });
        LogicActions.Add(new ActionToolItem { ActionType = ActionType.SwitchEnd, DisplayName = "End Switch", IconKind = "FormatListBulleted" });
    }

    public void AddNode(ActionToolItem? item)
    {
        if (item == null) return;
        var node = ActionNodeViewModel.CreateDefault(item.ActionType, 100 + Nodes.Count * 30, 100 + Nodes.Count * 130);
        Nodes.Add(node);
        SelectedNode = node;
    }

    public void HandleDrop(IDataObject data, Point position)
    {
        if (data.GetData(typeof(ActionToolItem)) is ActionToolItem item)
        {
            var node = ActionNodeViewModel.CreateDefault(item.ActionType, position.X, position.Y);
            Nodes.Add(node);
            SelectedNode = node;
        }
    }

    public void ClearSelection() => SelectedNode = null;

    /// <summary>
    /// Add connection between nodes with port directions
    /// </summary>
    public void AddConnection(Guid sourceNodeId, Guid targetNodeId, int sourcePortDir = 1, int targetPortDir = 3)
    {
        _logger.Info($"AddConnection called: {sourceNodeId} -> {targetNodeId}, SourcePort={sourcePortDir}, TargetPort={targetPortDir}");

        // Check if connection already exists
        if (Connections.Any(c => c.SourceNodeId == sourceNodeId && c.TargetNodeId == targetNodeId))
        {
            _logger.Warn("Connection already exists, skipping");
            return;
        }

        // Check for cycles
        if (WouldCreateCycle(sourceNodeId, targetNodeId))
        {
            _logger.Warn("Cannot create connection: would create a cycle");
            return;
        }

        var connection = new ConnectionViewModel
        {
            Id = Guid.NewGuid(),
            SourceNodeId = sourceNodeId,
            TargetNodeId = targetNodeId,
            SourcePortDirection = sourcePortDir,  // 0=Top, 1=Right, 2=Bottom, 3=Left
            TargetPortDirection = targetPortDir
        };

        Connections.Add(connection);
        SelectedLine = connection;
        _logger.Info($"Connection added to collection. Total connections: {Connections.Count}");

        UpdateConnectionPath(connection);
        _logger.Info($"Connection path updated. PathData: {connection.PathData?.Substring(0, Math.Min(50, connection.PathData?.Length ?? 0))}...");
    }

    /// <summary>
    /// 检查添加连接是否会创建环路
    /// </summary>
    private bool WouldCreateCycle(Guid sourceNodeId, Guid targetNodeId)
    {
        var visited = new HashSet<Guid>();
        var queue = new Queue<Guid>();
        queue.Enqueue(targetNodeId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == sourceNodeId)
                return true;

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            foreach (var conn in Connections.Where(c => c.SourceNodeId == current))
            {
                queue.Enqueue(conn.TargetNodeId);
            }
        }

        return false;
    }

    /// <summary>
    /// 更新所有连接线路径
    /// </summary>
    public void UpdateConnectionPaths()
    {
        // 更新执行顺序
        UpdateExecutionOrders();

        foreach (var connection in Connections)
        {
            UpdateConnectionPath(connection);
        }
    }

    /// <summary>
    /// 更新执行顺序编号
    /// </summary>
    private void UpdateExecutionOrders()
    {
        int order = 1;
        var processed = new HashSet<Guid>();

        // 找到起始节点（没有入边的节点）
        var startNodes = Nodes.Where(n => !Connections.Any(c => c.TargetNodeId == n.Id)).ToList();

        foreach (var startNode in startNodes)
        {
            AssignOrderDFS(startNode.Id, ref order, processed);
        }
    }

    private void AssignOrderDFS(Guid nodeId, ref int order, HashSet<Guid> processed)
    {
        if (processed.Contains(nodeId))
            return;

        processed.Add(nodeId);

        // 为从该节点出发的所有连接分配顺序
        foreach (var conn in Connections.Where(c => c.SourceNodeId == nodeId))
        {
            conn.ExecutionOrder = order++;
            AssignOrderDFS(conn.TargetNodeId, ref order, processed);
        }
    }

    /// <summary>
    /// 更新单个连接线路径（带箭头）
    /// </summary>
    /// <param name="connection"></param>
    private void UpdateConnectionPath(ConnectionViewModel connection)
    {
        var sourceNode = Nodes.FirstOrDefault(n => n.Id == connection.SourceNodeId);
        var targetNode = Nodes.FirstOrDefault(n => n.Id == connection.TargetNodeId);

        if (sourceNode == null || targetNode == null)
        {
            _logger.Warn($"UpdateConnectionPath: Node not found. SourceNode={sourceNode != null}, TargetNode={targetNode != null}");
            return;
        }

        _logger.Debug($"UpdateConnectionPath: Source({sourceNode.X}, {sourceNode.Y}), Target({targetNode.X}, {targetNode.Y})");

        var culture = CultureInfo.InvariantCulture;

        // Calculate port positions based on direction
        // 0=Top, 1=Right, 2=Bottom, 3=Left
        var (startX, startY) = GetPortPosition(sourceNode, connection.SourcePortDirection);
        var (endX, endY) = GetPortPosition(targetNode, connection.TargetPortDirection);

        _logger.Debug($"Port positions: Start({startX:F1}, {startY:F1}), End({endX:F1}, {endY:F1})");

        // Calculate control points for Bezier curve based on port directions
        var controlPointOffset = 80.0;
        string pathData;
        double arrowAngle;

        // Determine arrow angle based on target port direction
        arrowAngle = connection.TargetPortDirection switch
        {
            0 => 270,  // Top
            1 => 0,    // Right
            2 => 90,   // Bottom
            3 => 180,  // Left
            _ => 0
        };

        // Create smooth Bezier curve based on port directions
        var (cp1X, cp1Y) = GetControlPoint(startX, startY, connection.SourcePortDirection, controlPointOffset);
        var (cp2X, cp2Y) = GetControlPoint(endX, endY, connection.TargetPortDirection, controlPointOffset);

        pathData = string.Format(culture,
            "M {0:F1},{1:F1} C {2:F1},{3:F1} {4:F1},{5:F1} {6:F1},{7:F1}",
            startX, startY,
            cp1X, cp1Y,
            cp2X, cp2Y,
            endX, endY);

        connection.PathData = pathData;
        // write absolute points for UI bindings
        connection.StartPoint = new System.Windows.Point(startX, startY);
        connection.EndPoint = new System.Windows.Point(endX, endY);
        connection.ArrowX = endX;
        connection.ArrowY = endY;
        connection.ArrowAngle = arrowAngle;

        _logger.Debug($"PathData generated: {pathData}");

        // Generate arrow path
        var arrowSize = 8.0;
        var rad = arrowAngle * Math.PI / 180;
        var ax1 = endX - arrowSize * Math.Cos(rad - Math.PI / 6);
        var ay1 = endY - arrowSize * Math.Sin(rad - Math.PI / 6);
        var ax2 = endX - arrowSize * Math.Cos(rad + Math.PI / 6);
        var ay2 = endY - arrowSize * Math.Sin(rad + Math.PI / 6);

        connection.ArrowData = string.Format(culture,
            "M {0:F1},{1:F1} L {2:F1},{3:F1} L {4:F1},{5:F1} Z",
            endX, endY, ax1, ay1, ax2, ay2);

        _logger.Debug($"ArrowData generated: {connection.ArrowData}");
    }

    /// <summary>
    /// Get port position based on node and port direction
    /// </summary>
    private (double X, double Y) GetPortPosition(ActionNodeViewModel node, int direction)
    {
        return direction switch
        {
            0 => (node.X + node.Width / 2, node.Y),           // Top
            1 => (node.X + node.Width, node.Y + node.Height / 2),  // Right
            2 => (node.X + node.Width / 2, node.Y + node.Height),  // Bottom
            3 => (node.X, node.Y + node.Height / 2),          // Left
            _ => (node.X + node.Width, node.Y + node.Height / 2)   // Default: Right
        };
    }

    /// <summary>
    /// Get control point for Bezier curve based on port direction
    /// </summary>
    private (double X, double Y) GetControlPoint(double x, double y, int direction, double offset)
    {
        return direction switch
        {
            0 => (x, y - offset),           // Top: control point above
            1 => (x + offset, y),           // Right: control point to the right
            2 => (x, y + offset),           // Bottom: control point below
            3 => (x - offset, y),           // Left: control point to the left
            _ => (x + offset, y)            // Default: Right
        };
    }

    /// <summary>
    /// 删除节点之间的连接
    /// </summary>
    public void RemoveConnection(Guid connectionId)
    {
        var connection = Connections.FirstOrDefault(c => c.Id == connectionId);
        if (connection != null)
        {
            Connections.Remove(connection);
        }
    }

    /// <summary>
    /// 自动缩放以适应所有节点
    /// </summary>
    private void FitToPage()
    {
        if (Nodes.Count == 0)
        {
            ZoomLevel = 1.0;
            return;
        }

        // 计算所有节点的边界
        var minX = Nodes.Min(n => n.X);
        var minY = Nodes.Min(n => n.Y);
        var maxX = Nodes.Max(n => n.X + n.Width);
        var maxY = Nodes.Max(n => n.Y + n.Height);

        // 假设可视区域为 800x600（实际应该从 View 获取）
        var viewWidth = 800.0;
        var viewHeight = 600.0;
        var padding = 50.0;

        var contentWidth = maxX - minX + padding * 2;
        var contentHeight = maxY - minY + padding * 2;

        var scaleX = viewWidth / contentWidth;
        var scaleY = viewHeight / contentHeight;

        ZoomLevel = Math.Min(scaleX, scaleY);
    }

    private void DeleteSelected()
    {
        // If a node is selected, delete the node and all its connections
        if (SelectedNode != null)
        {
            var toRemove = Connections.Where(c => c.SourceNodeId == SelectedNode.Id || c.TargetNodeId == SelectedNode.Id).ToList();
            foreach (var conn in toRemove)
            {
                Connections.Remove(conn);
            }

            Nodes.Remove(SelectedNode);
            SelectedNode = null;
            return;
        }

        // Otherwise, if a connection line is selected, delete only that connection
        if (SelectedLine != null)
        {
            Connections.Remove(SelectedLine);
            SelectedLine = null;
        }
    }

    private void ClearAll()
    {
        if (MessageBox.Show("Clear all nodes?", "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
        Nodes.Clear();
        Connections.Clear();
        SelectedNode = null;
    }

    private void NewProgram()
    {
        if (Nodes.Count > 0 && MessageBox.Show("Save current program?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        {
            _ = SaveProgramAsync();
        }
        Nodes.Clear();
        Connections.Clear();
        SelectedNode = null;
        _currentProgram = null;
    }

    private async Task OpenProgramAsync()
    {
        var dialog = new OpenFileDialog { Filter = "Motion Program (*.json)|*.json", Title = "Open Program" };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var json = await File.ReadAllTextAsync(dialog.FileName);
            var program = await _programService.ImportFromJsonAsync(json);
            LoadProgramToCanvas(program);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open program");
            MessageBox.Show($"Failed to open: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task SaveProgramAsync()
    {
        var dialog = new SaveFileDialog { Filter = "Motion Program (*.json)|*.json", FileName = _currentProgram?.Name ?? "Program" };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var request = CreateSaveRequest(Path.GetFileNameWithoutExtension(dialog.FileName));
            _currentProgram = await _programService.SaveAsync(request);
            var json = await _programService.ExportToJsonAsync(_currentProgram.Id);
            await File.WriteAllTextAsync(dialog.FileName, json);
            MessageBox.Show("Saved!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save");
            MessageBox.Show($"Failed to save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExportToJsonAsync()
    {
        var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = _currentProgram?.Name ?? "Program" };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var request = CreateSaveRequest(Path.GetFileNameWithoutExtension(dialog.FileName));
            var program = await _programService.SaveAsync(request);
            var json = await _programService.ExportToJsonAsync(program.Id);
            await File.WriteAllTextAsync(dialog.FileName, json);
            MessageBox.Show("Exported!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to export");
            MessageBox.Show($"Failed to export: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private SaveMotionProgramRequest CreateSaveRequest(string name)
    {
        return new SaveMotionProgramRequest(
            _currentProgram?.Id, name, "", "Default", false,
            Nodes.Select(n => new ActionNodeDto(n.Id, n.Name, n.ActionType, n.Parameters, n.X, n.Y, n.Width, n.Height,
                n.Description, n.IsEnabled, n.TimeoutMs, n.RetryCount, n.ErrorHandling)).ToList(),
            Connections.Select(c => new NodeConnectionDto(c.Id, c.SourceNodeId, c.TargetNodeId, 0, 0, "", "")).ToList(),
            new Dictionary<string, object>()
        );
    }

    private void LoadProgramToCanvas(MotionProgramDto program)
    {
        Nodes.Clear();
        Connections.Clear();

        foreach (var node in program.Nodes)
        {
            Nodes.Add(new ActionNodeViewModel
            {
                Id = node.Id,
                Name = node.Name,
                ActionType = node.ActionType,
                Parameters = node.Parameters,
                X = node.X,
                Y = node.Y,
                Width = node.Width,
                Height = node.Height,
                Description = node.Description,
                IsEnabled = node.IsEnabled,
                TimeoutMs = node.TimeoutMs,
                RetryCount = node.RetryCount,
                ErrorHandling = node.ErrorHandling
            });
        }

        foreach (var conn in program.Connections)
        {
            var source = Nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId);
            var target = Nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
            if (source != null && target != null)
            {
                var connection = new ConnectionViewModel { Id = conn.Id, SourceNodeId = conn.SourceNodeId, TargetNodeId = conn.TargetNodeId };
                UpdateConnectionPath(connection, source, target);
                Connections.Add(connection);
            }
        }
        _currentProgram = program;
    }

    private void UpdateConnectionPath(ConnectionViewModel conn, ActionNodeViewModel source, ActionNodeViewModel target)
    {
        var sx = source.X + source.Width;
        var sy = source.Y + source.Height / 2;
        var ex = target.X;
        var ey = target.Y + target.Height / 2;
        var offset = Math.Abs(ex - sx) / 2;
        conn.PathData = $"M {sx},{sy} C {sx + offset},{sy} {ex - offset},{ey} {ex},{ey}";
    }

    private async Task RunProgramAsync()
    {
        if (Nodes.Count == 0) { MessageBox.Show("Add nodes first", "Info"); return; }

        try
        {
            var request = CreateSaveRequest(_currentProgram?.Name ?? "Temp");
            var program = await _programService.SaveAsync(request);
            await _executor.LoadProgramAsync(program);
            IsRunning = true;
            ExecutionState = "Running";
            await _executor.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to run");
            MessageBox.Show($"Failed: {ex.Message}", "Error");
            IsRunning = false;
            ExecutionState = "Error";
        }
    }

    private void OnProgressChanged(object? sender, ExecutionProgress e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            ExecutionState = e.State.ToString();
            ExecutionProgress = e.ProgressPercent;
            foreach (var node in Nodes) node.IsExecuting = node.Id == e.CurrentNodeId;
        });
    }

    private void OnNodeExecuted(object? sender, NodeExecutionResult e)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var node = Nodes.FirstOrDefault(n => n.Id == e.NodeId);
            if (node != null) node.HasError = !e.Success;
        });
    }

    private void OnProgramCompleted(object? sender, bool success)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            ExecutionState = success ? "Completed" : "Stopped";
            foreach (var node in Nodes) node.IsExecuting = false;
            if (success) MessageBox.Show("Program completed!", "Info");
        });
    }
    
    #region Configuration Sync Event Handlers
    
    /// <summary>
    /// Import hardware configuration
    /// </summary>
    private async void ImportConfig()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Import Hardware Configuration"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var config = await _configService.ImportFromFileAsync(dialog.FileName);
                _currentConfig = config;
                
                // Publish events
                _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
                _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
                {
                    Config = config,
                    FilePath = dialog.FileName,
                    Source = "DesignerView-Import"
                });
                
                _logger.Info($"Configuration imported successfully from {dialog.FileName}");
                MessageBox.Show("Configuration imported successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to import configuration");
                MessageBox.Show($"Failed to import configuration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    /// <summary>
    /// Handle configuration imported event
    /// </summary>
    private void OnConfigImported(DeviceConfigDto config)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _currentConfig = config;
            _logger.Info($"Received config imported event");
            // Update designer with new configuration
            // TODO: Refresh device list in action node parameters
        });
    }
    
    /// <summary>
    /// Handle configuration created event
    /// </summary>
    private void OnConfigCreated(DeviceConfigDto config)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _currentConfig = config;
            _logger.Info($"Received config created event");
            // Initialize designer with new configuration
        });
    }
    
    /// <summary>
    /// Handle configuration loaded event
    /// </summary>
    private void OnConfigLoaded(ConfigLoadedEventArgs args)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _currentConfig = args.Config;
            _logger.Info($"Received config loaded event from {args.Source}");
            // Process configuration loading
        });
    }
    
    /// <summary>
    /// Handle position updated event
    /// </summary>
    private void OnPositionUpdated(PositionUpdatedEventArgs args)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.Info($"Received position updated: {args.DeviceId}/{args.PositionName}");
            // Update nodes that use this position
            UpdateNodePositions(args.DeviceId, args.PositionName, args.Position);
        });
    }
    
    /// <summary>
    /// Handle position added event
    /// </summary>
    private void OnPositionAdded(PositionPointViewModel position)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.Info($"Received position added: {position.DeviceName}/{position.PositionName}");
            // Refresh position options for the device
        });
    }
    
    /// <summary>
    /// Handle position deleted event
    /// </summary>
    private void OnPositionDeleted(PositionPointViewModel position)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.Info($"Received position deleted: {position.DeviceName}/{position.PositionName}");
            // Remove position from options and mark nodes using it
        });
    }
    
    /// <summary>
    /// Handle device added event
    /// </summary>
    private void OnDeviceAdded(DeviceAddedEventArgs args)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            _logger.Info($"Received device added: {args.DeviceType} - {args.DeviceName}");
            // Refresh device list in action node parameters
        });
    }
    
    /// <summary>
    /// Update node positions when a position is updated
    /// </summary>
    private void UpdateNodePositions(string deviceId, string positionName, double newPosition)
    {
        foreach (var node in Nodes)
        {
            // Update nodes that use this device and position
            if (node.ActionType == ActionType.MotorMoveAbsolute)
            {
                // Check if this node uses the updated position
                // TODO: Implement position parameter update logic
            }
        }
    }
    
    #endregion
}


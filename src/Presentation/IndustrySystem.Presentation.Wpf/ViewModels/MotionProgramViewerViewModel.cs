using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using System.Windows.Media;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>
/// 程序查看器中的节点显示模型
/// </summary>
public class ProgramNodeDisplayModel : BindableBase
{
    private bool _isExecuting;
    private bool _isCompleted;
    private bool _hasError;
    
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ActionType ActionType { get; set; }
    
    public bool IsExecuting { get => _isExecuting; set => SetProperty(ref _isExecuting, value); }
    public bool IsCompleted { get => _isCompleted; set => SetProperty(ref _isCompleted, value); }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }
    
    public string IconKind => ActionType switch
    {
        ActionType.MotorMoveAbsolute or ActionType.MotorMoveRelative => "AxisArrow",
        ActionType.MotorHome => "Home",
        ActionType.MotorStop => "Stop",
        ActionType.WaitMotorDone => "TimerSand",
        ActionType.IoOutput => "ExportVariant",
        ActionType.WaitIoInput => "ImportVariant",
        ActionType.RobotMoveTo or ActionType.RobotRunProgram => "Robot",
        ActionType.Delay => "ClockOutline",
        ActionType.LoopStart or ActionType.LoopEnd => "Repeat",
        ActionType.Condition => "HelpRhombus",
        ActionType.SetVariable => "Variable",
        ActionType.Log => "TextBoxOutline",
        ActionType.Alarm => "AlertCircle",
        _ => "CogOutline"
    };
    
    public Brush HeaderColor => ActionType switch
    {
        ActionType.MotorMoveAbsolute or ActionType.MotorMoveRelative or ActionType.MotorHome or ActionType.MotorStop or ActionType.WaitMotorDone 
            => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
        ActionType.IoOutput or ActionType.WaitIoInput 
            => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
        ActionType.RobotMoveTo or ActionType.RobotRunProgram 
            => new SolidColorBrush(Color.FromRgb(156, 39, 176)),
        ActionType.Delay => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
        _ => new SolidColorBrush(Color.FromRgb(96, 125, 139))
    };
    
    public Brush Background
    {
        get
        {
            if (IsExecuting) return new SolidColorBrush(Color.FromArgb(30, 33, 150, 243));
            if (HasError) return new SolidColorBrush(Color.FromArgb(30, 244, 67, 54));
            if (IsCompleted) return new SolidColorBrush(Color.FromArgb(30, 76, 175, 80));
            return Brushes.White;
        }
    }
}

/// <summary>
/// 程序查看器/执行器视图模型 - 用于在主应用中加载和执行动作程序
/// </summary>
public class MotionProgramViewerViewModel : BindableBase
{
    private static readonly NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
    
    private readonly IMotionProgramAppService _programService;
    private readonly IMotionProgramExecutor _executor;
    
    private MotionProgramDto? _currentProgram;
    private string _programName = "No Program Loaded";
    private string _executionState = "Idle";
    private double _progress;
    private int _executedCount;
    private int _totalCount;
    private bool _isRunning;
    private bool _canRun;
    private ProgramNodeDisplayModel? _selectedNode;

    public ObservableCollection<ProgramNodeDisplayModel> Nodes { get; } = new();
    public ObservableCollection<KeyValuePair<string, string>> Variables { get; } = new();
    public ObservableCollection<string> ExecutionLogs { get; } = new();

    public string ProgramName { get => _programName; set => SetProperty(ref _programName, value); }
    public string ExecutionState { get => _executionState; set => SetProperty(ref _executionState, value); }
    public double Progress { get => _progress; set => SetProperty(ref _progress, value); }
    public int ExecutedCount { get => _executedCount; set => SetProperty(ref _executedCount, value); }
    public int TotalCount { get => _totalCount; set => SetProperty(ref _totalCount, value); }
    public bool IsRunning { get => _isRunning; set => SetProperty(ref _isRunning, value); }
    public bool CanRun { get => _canRun; set => SetProperty(ref _canRun, value); }
    public ProgramNodeDisplayModel? SelectedNode { get => _selectedNode; set => SetProperty(ref _selectedNode, value); }

    public ICommand LoadCommand { get; }
    public ICommand RunCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }

    public MotionProgramViewerViewModel(IMotionProgramAppService programService, IMotionProgramExecutor executor)
    {
        _programService = programService;
        _executor = executor;
        
        LoadCommand = new DelegateCommand(async () => await LoadProgramAsync());
        RunCommand = new DelegateCommand(async () => await RunProgramAsync());
        PauseCommand = new DelegateCommand(async () => await _executor.PauseAsync());
        StopCommand = new DelegateCommand(async () => await _executor.StopAsync());
        
        _executor.ProgressChanged += OnProgressChanged;
        _executor.NodeExecuted += OnNodeExecuted;
        _executor.ProgramCompleted += OnProgramCompleted;
    }

    /// <summary>
    /// 从文件加载程序
    /// </summary>
    public async Task LoadProgramAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Motion Program (*.json)|*.json",
            Title = "Load Motion Program"
        };
        
        if (dialog.ShowDialog() != true) return;
        
        await LoadProgramFromFileAsync(dialog.FileName);
    }

    /// <summary>
    /// 从指定文件路径加载程序
    /// </summary>
    public async Task LoadProgramFromFileAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var program = JsonSerializer.Deserialize<MotionProgramDto>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            
            if (program != null)
            {
                await LoadProgramAsync(program);
                AddLog($"Loaded program: {program.Name}");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load program from file");
            AddLog($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// 加载程序DTO
    /// </summary>
    public async Task LoadProgramAsync(MotionProgramDto program)
    {
        _currentProgram = program;
        ProgramName = program.Name;
        TotalCount = program.Nodes.Count;
        ExecutedCount = 0;
        Progress = 0;
        
        Nodes.Clear();
        foreach (var node in program.Nodes)
        {
            Nodes.Add(new ProgramNodeDisplayModel
            {
                Id = node.Id,
                Name = node.Name,
                Description = node.Description,
                ActionType = node.ActionType
            });
        }
        
        Variables.Clear();
        foreach (var variable in program.Variables)
        {
            Variables.Add(new KeyValuePair<string, string>(variable.Key, variable.Value?.ToString() ?? ""));
        }
        
        await _executor.LoadProgramAsync(program);
        CanRun = true;
        ExecutionState = "Ready";
        
        _logger.Info($"Program loaded: {program.Name} with {program.Nodes.Count} nodes");
    }

    /// <summary>
    /// 运行程序
    /// </summary>
    public async Task RunProgramAsync()
    {
        if (_currentProgram == null)
        {
            AddLog("No program loaded");
            return;
        }
        
        try
        {
            // 重置状态
            foreach (var node in Nodes)
            {
                node.IsExecuting = false;
                node.IsCompleted = false;
                node.HasError = false;
            }
            
            ExecutionLogs.Clear();
            AddLog($"Starting program: {_currentProgram.Name}");
            
            IsRunning = true;
            CanRun = false;
            ExecutionState = "Running";
            
            await _executor.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to run program");
            AddLog($"Error: {ex.Message}");
            IsRunning = false;
            CanRun = true;
            ExecutionState = "Error";
        }
    }

    private void OnProgressChanged(object? sender, ExecutionProgress e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            ExecutionState = e.State.ToString();
            Progress = e.ProgressPercent;
            ExecutedCount = e.ExecutedCount;
            
            // 更新当前执行节点
            foreach (var node in Nodes)
            {
                if (node.Id == e.CurrentNodeId)
                {
                    node.IsExecuting = true;
                    SelectedNode = node;
                }
                else if (node.IsExecuting)
                {
                    node.IsExecuting = false;
                    node.IsCompleted = true;
                }
            }
            
            // 更新变量
            UpdateVariablesAsync();
        });
    }

    private void OnNodeExecuted(object? sender, NodeExecutionResult e)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            var node = Nodes.FirstOrDefault(n => n.Id == e.NodeId);
            if (node != null)
            {
                node.IsExecuting = false;
                node.IsCompleted = e.Success;
                node.HasError = !e.Success;
            }
            
            AddLog($"[{(e.Success ? "OK" : "ERR")}] {e.NodeName}: {e.Message} ({e.Duration.TotalMilliseconds:F0}ms)");
        });
    }

    private void OnProgramCompleted(object? sender, bool success)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            IsRunning = false;
            CanRun = true;
            ExecutionState = success ? "Completed" : "Stopped";
            
            foreach (var node in Nodes)
            {
                node.IsExecuting = false;
            }
            
            AddLog(success ? "Program completed successfully" : "Program stopped");
        });
    }

    private async void UpdateVariablesAsync()
    {
        try
        {
            var vars = await _executor.GetAllVariablesAsync();
            Variables.Clear();
            foreach (var kvp in vars)
            {
                Variables.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value?.ToString() ?? ""));
            }
        }
        catch { }
    }

    private void AddLog(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        ExecutionLogs.Insert(0, $"[{timestamp}] {message}");
        
        // 限制日志数量
        while (ExecutionLogs.Count > 100)
        {
            ExecutionLogs.RemoveAt(ExecutionLogs.Count - 1);
        }
    }
}

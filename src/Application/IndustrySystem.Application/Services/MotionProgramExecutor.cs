using System.Collections.Concurrent;
using System.Text.Json;
using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 动作程序执行引擎实现
/// </summary>
public class MotionProgramExecutor : IMotionProgramExecutor
{
    private readonly ILogger<MotionProgramExecutor> _logger;
    private readonly IMotionProgramAppService _programService;
    private readonly IHardwareController _hardwareController;
    
    private MotionProgramDto? _currentProgram;
    private readonly ConcurrentDictionary<string, object> _variables = new();
    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseEvent = new(true);
    
    private ProgramExecutionState _state = ProgramExecutionState.Idle;
    private ExecutionProgress? _currentProgress;
    private Guid? _currentNodeId;
    private int _executedCount;
    private DateTime _startTime;

    public ProgramExecutionState State => _state;
    public ExecutionProgress? CurrentProgress => _currentProgress;

    public event EventHandler<ExecutionProgress>? ProgressChanged;
    public event EventHandler<NodeExecutionResult>? NodeExecuted;
    public event EventHandler<bool>? ProgramCompleted;

    public MotionProgramExecutor(
        ILogger<MotionProgramExecutor> logger,
        IMotionProgramAppService programService,
        IHardwareController hardwareController)
    {
        _logger = logger;
        _programService = programService;
        _hardwareController = hardwareController;
    }

    public async Task LoadProgramAsync(MotionProgramDto program)
    {
        _logger.LogInformation("Loading program: {Name}", program.Name);
        
        if (_state == ProgramExecutionState.Running)
        {
            throw new InvalidOperationException("Cannot load program while running");
        }
        
        _currentProgram = program;
        _variables.Clear();
        
        foreach (var variable in program.Variables)
        {
            _variables[variable.Key] = variable.Value;
        }
        
        _state = ProgramExecutionState.Idle;
        UpdateProgress("Program loaded");
    }

    public async Task LoadProgramAsync(Guid programId)
    {
        var program = await _programService.GetAsync(programId)
                      ?? throw new InvalidOperationException($"Program not found: {programId}");
        
        await LoadProgramAsync(program);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_currentProgram == null)
        {
            throw new InvalidOperationException("No program loaded");
        }
        
        if (_state == ProgramExecutionState.Running)
        {
            throw new InvalidOperationException("Program is already running");
        }
        
        _logger.LogInformation("Starting program: {Name}", _currentProgram.Name);
        
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _state = ProgramExecutionState.Running;
        _startTime = DateTime.Now;
        _executedCount = 0;
        _pauseEvent.Set();
        
        try
        {
            await ExecuteProgramAsync(_cts.Token);
            
            _state = ProgramExecutionState.Completed;
            UpdateProgress("Program completed");
            ProgramCompleted?.Invoke(this, true);
            
            _logger.LogInformation("Program completed: {Name}", _currentProgram.Name);
        }
        catch (OperationCanceledException)
        {
            _state = ProgramExecutionState.Stopped;
            UpdateProgress("Program stopped");
            ProgramCompleted?.Invoke(this, false);
            
            _logger.LogInformation("Program stopped: {Name}", _currentProgram.Name);
        }
        catch (Exception ex)
        {
            _state = ProgramExecutionState.Error;
            UpdateProgress($"Error: {ex.Message}", ex);
            ProgramCompleted?.Invoke(this, false);
            
            _logger.LogError(ex, "Program error: {Name}", _currentProgram.Name);
            throw;
        }
    }

    public Task PauseAsync()
    {
        if (_state != ProgramExecutionState.Running)
        {
            return Task.CompletedTask;
        }
        
        _logger.LogInformation("Pausing program");
        _pauseEvent.Reset();
        _state = ProgramExecutionState.Paused;
        UpdateProgress("Program paused");
        
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        if (_state != ProgramExecutionState.Paused)
        {
            return Task.CompletedTask;
        }
        
        _logger.LogInformation("Resuming program");
        _state = ProgramExecutionState.Running;
        _pauseEvent.Set();
        UpdateProgress("Program resumed");
        
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_state != ProgramExecutionState.Running && _state != ProgramExecutionState.Paused)
        {
            return Task.CompletedTask;
        }
        
        _logger.LogInformation("Stopping program");
        _cts?.Cancel();
        _pauseEvent.Set();
        
        return Task.CompletedTask;
    }

    public async Task StepAsync()
    {
        if (_currentProgram == null)
        {
            throw new InvalidOperationException("No program loaded");
        }
        
        if (_state == ProgramExecutionState.Running)
        {
            throw new InvalidOperationException("Cannot step while running");
        }
        
        _logger.LogInformation("Stepping program");
        
        // 找到下一个要执行的节点
        var nextNode = GetNextNode(_currentNodeId);
        if (nextNode == null)
        {
            _logger.LogInformation("No more nodes to execute");
            return;
        }
        
        await ExecuteNodeAsync(nextNode, CancellationToken.None);
    }

    public Task GotoNodeAsync(Guid nodeId)
    {
        if (_currentProgram == null)
        {
            throw new InvalidOperationException("No program loaded");
        }
        
        var node = _currentProgram.Nodes.FirstOrDefault(n => n.Id == nodeId)
                   ?? throw new InvalidOperationException($"Node not found: {nodeId}");
        
        _logger.LogInformation("Goto node: {Name}", node.Name);
        _currentNodeId = nodeId;
        UpdateProgress($"Jumped to: {node.Name}");
        
        return Task.CompletedTask;
    }

    public Task SetVariableAsync(string name, object value)
    {
        _variables[name] = value;
        _logger.LogDebug("Variable set: {Name} = {Value}", name, value);
        return Task.CompletedTask;
    }

    public Task<object?> GetVariableAsync(string name)
    {
        _variables.TryGetValue(name, out var value);
        return Task.FromResult(value);
    }

    public Task<Dictionary<string, object>> GetAllVariablesAsync()
    {
        return Task.FromResult(_variables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    #region Private Methods

    private async Task ExecuteProgramAsync(CancellationToken ct)
    {
        if (_currentProgram == null) return;
        
        // 找到起始节点（没有入边的节点）
        var startNodes = GetStartNodes();
        if (!startNodes.Any())
        {
            _logger.LogWarning("No start nodes found");
            return;
        }
        
        _currentNodeId = null;
        
        foreach (var startNode in startNodes)
        {
            await ExecuteFromNodeAsync(startNode, ct);
        }
    }

    private async Task ExecuteFromNodeAsync(ActionNodeDto node, CancellationToken ct)
    {
        _currentNodeId = node.Id;
        
        while (_currentNodeId.HasValue && !ct.IsCancellationRequested)
        {
            // 检查暂停
            _pauseEvent.Wait(ct);
            ct.ThrowIfCancellationRequested();
            
            var currentNode = _currentProgram!.Nodes.FirstOrDefault(n => n.Id == _currentNodeId);
            if (currentNode == null) break;
            
            // 跳过禁用的节点
            if (!currentNode.IsEnabled)
            {
                _currentNodeId = GetNextNode(_currentNodeId)?.Id;
                continue;
            }
            
            // 执行节点
            await ExecuteNodeAsync(currentNode, ct);
            
            // 获取下一个节点
            var nextNode = GetNextNode(_currentNodeId);
            _currentNodeId = nextNode?.Id;
        }
    }

    private async Task ExecuteNodeAsync(ActionNodeDto node, CancellationToken ct)
    {
        _logger.LogDebug("Executing node: {Name} ({ActionType})", node.Name, node.ActionType);
        
        var startTime = DateTime.Now;
        var success = false;
        var message = "";
        Dictionary<string, object>? outputData = null;
        
        try
        {
            UpdateProgress($"Executing: {node.Name}");
            
            (success, message, outputData) = await ExecuteActionAsync(node, ct);
            
            _executedCount++;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            
            // 错误处理
            switch (node.ErrorHandling)
            {
                case ErrorHandling.Skip:
                    _logger.LogWarning("Node error (skipped): {Name} - {Message}", node.Name, ex.Message);
                    success = true;
                    break;
                    
                case ErrorHandling.Retry:
                    for (int i = 0; i < node.RetryCount && !success; i++)
                    {
                        _logger.LogInformation("Retrying node: {Name} ({Attempt}/{Total})", node.Name, i + 1, node.RetryCount);
                        try
                        {
                            (success, message, outputData) = await ExecuteActionAsync(node, ct);
                        }
                        catch { }
                    }
                    break;
                    
                default:
                    throw;
            }
        }
        
        var duration = DateTime.Now - startTime;
        
        var result = new NodeExecutionResult(
            node.Id,
            node.Name,
            success,
            message,
            duration,
            outputData
        );
        
        NodeExecuted?.Invoke(this, result);
        
        _logger.LogDebug("Node executed: {Name}, Success: {Success}, Duration: {Duration}ms", node.Name, success, duration.TotalMilliseconds);
    }

    private async Task<(bool Success, string Message, Dictionary<string, object>? OutputData)> 
        ExecuteActionAsync(ActionNodeDto node, CancellationToken ct)
    {
        return node.ActionType switch
        {
            ActionType.MotorMoveAbsolute => await ExecuteMotorMoveAsync(node, false, ct),
            ActionType.MotorMoveRelative => await ExecuteMotorMoveAsync(node, true, ct),
            ActionType.MotorHome => await ExecuteMotorHomeAsync(node, ct),
            ActionType.MotorStop => await ExecuteMotorStopAsync(node, ct),
            ActionType.WaitMotorDone => await ExecuteWaitMotorDoneAsync(node, ct),
            ActionType.IoOutput => await ExecuteIoOutputAsync(node, ct),
            ActionType.WaitIoInput => await ExecuteWaitIoInputAsync(node, ct),
            ActionType.RobotMoveTo => await ExecuteRobotMoveAsync(node, ct),
            ActionType.Delay => await ExecuteDelayAsync(node, ct),
            ActionType.SetVariable => ExecuteSetVariable(node),
            ActionType.Log => ExecuteLog(node),
            ActionType.Alarm => ExecuteAlarm(node),
            _ => (true, $"Unknown action type: {node.ActionType}", null)
        };
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteMotorMoveAsync(ActionNodeDto node, bool relative, CancellationToken ct)
    {
        var motorId = GetParam<string>(node, "motorId") ?? "";
        var position = GetParam<double>(node, "position");
        var speed = GetParam<double>(node, "speed");
        var waitDone = GetParam<bool>(node, "waitDone");
        
        await _hardwareController.MoveMotorAsync(motorId, position, speed, relative, waitDone, ct);
        
        return (true, $"Motor {motorId} moved to {position}", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteMotorHomeAsync(ActionNodeDto node, CancellationToken ct)
    {
        var motorId = GetParam<string>(node, "motorId") ?? "";
        await _hardwareController.HomeMotorAsync(motorId, ct);
        return (true, $"Motor {motorId} homed", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteMotorStopAsync(ActionNodeDto node, CancellationToken ct)
    {
        var motorId = GetParam<string>(node, "motorId") ?? "";
        await _hardwareController.StopMotorAsync(motorId, ct);
        return (true, $"Motor {motorId} stopped", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteWaitMotorDoneAsync(ActionNodeDto node, CancellationToken ct)
    {
        var motorId = GetParam<string>(node, "motorId") ?? "";
        var timeout = node.TimeoutMs > 0 ? node.TimeoutMs : 30000;
        var done = await _hardwareController.WaitMotorDoneAsync(motorId, timeout, ct);
        return (done, done ? $"Motor {motorId} done" : $"Motor {motorId} timeout", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteIoOutputAsync(ActionNodeDto node, CancellationToken ct)
    {
        var moduleId = GetParam<string>(node, "moduleId") ?? "";
        var portIndex = GetParam<int>(node, "portIndex");
        var value = GetParam<bool>(node, "value");
        
        await _hardwareController.SetIoOutputAsync(moduleId, portIndex, value, ct);
        return (true, $"IO {moduleId}:{portIndex} = {value}", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteWaitIoInputAsync(ActionNodeDto node, CancellationToken ct)
    {
        var moduleId = GetParam<string>(node, "moduleId") ?? "";
        var portIndex = GetParam<int>(node, "portIndex");
        var expectedValue = GetParam<bool>(node, "expectedValue");
        var timeout = GetParam<int>(node, "timeoutMs");
        if (timeout <= 0) timeout = 10000;
        
        var result = await _hardwareController.WaitIoInputAsync(moduleId, portIndex, expectedValue, timeout, ct);
        return (result, result ? $"IO {moduleId}:{portIndex} = {expectedValue}" : "IO wait timeout", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteRobotMoveAsync(ActionNodeDto node, CancellationToken ct)
    {
        var robotId = GetParam<string>(node, "robotId") ?? "";
        var pointName = GetParam<string>(node, "pointName") ?? "";
        var moveType = GetParam<string>(node, "moveType") ?? "Joint";
        var speedPercent = GetParam<int>(node, "speedPercent");
        if (speedPercent <= 0) speedPercent = 50;
        
        await _hardwareController.MoveRobotAsync(robotId, pointName, new double[6], moveType, speedPercent, ct);
        return (true, $"Robot {robotId} moved to {pointName}", null);
    }

    private async Task<(bool, string, Dictionary<string, object>?)> 
        ExecuteDelayAsync(ActionNodeDto node, CancellationToken ct)
    {
        var delayMs = GetParam<int>(node, "delayMs");
        if (delayMs <= 0) delayMs = 1000;
        
        await Task.Delay(delayMs, ct);
        return (true, $"Delayed {delayMs}ms", null);
    }

    private (bool, string, Dictionary<string, object>?) ExecuteSetVariable(ActionNodeDto node)
    {
        var variableName = GetParam<string>(node, "variableName") ?? "";
        var value = GetParam<string>(node, "value") ?? "";
        
        _variables[variableName] = value;
        return (true, $"Variable {variableName} = {value}", null);
    }

    private (bool, string, Dictionary<string, object>?) ExecuteLog(ActionNodeDto node)
    {
        var message = GetParam<string>(node, "message") ?? "";
        message = ReplaceVariables(message);
        
        _logger.LogInformation("[Program Log] {Message}", message);
        return (true, message, null);
    }

    private (bool, string, Dictionary<string, object>?) ExecuteAlarm(ActionNodeDto node)
    {
        var alarmCode = GetParam<string>(node, "alarmCode") ?? "";
        var message = GetParam<string>(node, "message") ?? "";
        
        _logger.LogWarning("ALARM [{Code}]: {Message}", alarmCode, message);
        return (true, $"Alarm triggered: {alarmCode}", null);
    }

    private T? GetParam<T>(ActionNodeDto node, string key)
    {
        if (!node.Parameters.TryGetValue(key, out var value))
            return default;
        
        if (value is T typed) return typed;
        
        if (value is JsonElement element)
        {
            if (typeof(T) == typeof(string)) return (T)(object)element.GetString()!;
            if (typeof(T) == typeof(int)) return (T)(object)element.GetInt32();
            if (typeof(T) == typeof(double)) return (T)(object)element.GetDouble();
            if (typeof(T) == typeof(bool)) return (T)(object)element.GetBoolean();
        }
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    private string ReplaceVariables(string input)
    {
        foreach (var variable in _variables)
        {
            input = input.Replace($"${{{variable.Key}}}", variable.Value?.ToString() ?? "");
        }
        return input;
    }

    private List<ActionNodeDto> GetStartNodes()
    {
        if (_currentProgram == null) return new();
        
        var targetNodeIds = _currentProgram.Connections.Select(c => c.TargetNodeId).ToHashSet();
        return _currentProgram.Nodes.Where(n => !targetNodeIds.Contains(n.Id)).ToList();
    }

    private ActionNodeDto? GetNextNode(Guid? currentNodeId)
    {
        if (_currentProgram == null || !currentNodeId.HasValue) return null;
        
        var connection = _currentProgram.Connections
            .FirstOrDefault(c => c.SourceNodeId == currentNodeId.Value);
        
        if (connection == null) return null;
        
        return _currentProgram.Nodes.FirstOrDefault(n => n.Id == connection.TargetNodeId);
    }

    private void UpdateProgress(String message, Exception? error = null)
    {
        if (_currentProgram == null) return;
        
        var currentNode = _currentNodeId.HasValue 
            ? _currentProgram.Nodes.FirstOrDefault(n => n.Id == _currentNodeId) 
            : null;
        
        _currentProgress = new ExecutionProgress(
            _currentProgram.Id,
            _currentProgram.Name,
            _state,
            _currentNodeId,
            currentNode?.Name ?? "",
            _executedCount,
            _currentProgram.Nodes.Count,
            _currentProgram.Nodes.Count > 0 
                ? (double)_executedCount / _currentProgram.Nodes.Count * 100 
                : 0,
            message,
            _startTime,
            DateTime.Now - _startTime,
            error
        );
        
        ProgressChanged?.Invoke(this, _currentProgress);
    }

    #endregion
}

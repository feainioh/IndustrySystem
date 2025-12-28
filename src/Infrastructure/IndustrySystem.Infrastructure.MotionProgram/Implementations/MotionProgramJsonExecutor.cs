using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Infrastructure.MotionProgram.Abstractions;
using Microsoft.Extensions.Logging;

namespace IndustrySystem.Infrastructure.MotionProgram.Implementations;

/// <summary>
/// MotionProgram JSON 执行器实现
/// </summary>
public class MotionProgramJsonExecutor : IMotionProgramJsonExecutor
{
    private readonly ILogger<MotionProgramJsonExecutor> _logger;
    private readonly IMotionProgramJsonParser _parser;
    private readonly IMotionProgramExecutor _executor;

    public MotionProgramJsonExecutor(
        ILogger<MotionProgramJsonExecutor> logger,
        IMotionProgramJsonParser parser,
        IMotionProgramExecutor executor)
    {
        _logger = logger;
        _parser = parser;
        _executor = executor;

        // 转发事件
        _executor.ProgressChanged += (sender, progress) => ProgressChanged?.Invoke(this, progress);
        _executor.NodeExecuted += (sender, result) => NodeExecuted?.Invoke(this, result);
        _executor.ProgramCompleted += (sender, success) => ProgramCompleted?.Invoke(this, success);
    }

    public ProgramExecutionState State => _executor.State;
    public ExecutionProgress? CurrentProgress => _executor.CurrentProgress;

    public event EventHandler<ExecutionProgress>? ProgressChanged;
    public event EventHandler<NodeExecutionResult>? NodeExecuted;
    public event EventHandler<bool>? ProgramCompleted;

    public async Task LoadAndExecuteFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从文件加载并执行程序: {FilePath}", filePath);
        
        await LoadFromFileAsync(filePath, cancellationToken);
        await StartAsync(cancellationToken);
    }

    public async Task LoadAndExecuteFromJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从 JSON 加载并执行程序");
        
        await LoadFromJsonAsync(json, cancellationToken);
        await StartAsync(cancellationToken);
    }

    public async Task LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从文件加载程序: {FilePath}", filePath);
        
        var program = await _parser.ParseFromFileAsync(filePath, cancellationToken);
        await _executor.LoadProgramAsync(program);
        
        _logger.LogInformation("程序加载成功: {Name}", program.Name);
    }

    public async Task LoadFromJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("从 JSON 加载程序");
        
        var program = await _parser.ParseFromJsonAsync(json, cancellationToken);
        await _executor.LoadProgramAsync(program);
        
        _logger.LogInformation("程序加载成功: {Name}", program.Name);
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("开始执行程序");
        return _executor.StartAsync(cancellationToken);
    }

    public Task PauseAsync()
    {
        _logger.LogInformation("暂停执行程序");
        return _executor.PauseAsync();
    }

    public Task ResumeAsync()
    {
        _logger.LogInformation("恢复执行程序");
        return _executor.ResumeAsync();
    }

    public Task StopAsync()
    {
        _logger.LogInformation("停止执行程序");
        return _executor.StopAsync();
    }

    public Task StepAsync()
    {
        _logger.LogInformation("单步执行程序");
        return _executor.StepAsync();
    }

    public Task GotoNodeAsync(Guid nodeId)
    {
        _logger.LogInformation("跳转到节点: {NodeId}", nodeId);
        return _executor.GotoNodeAsync(nodeId);
    }

    public Task SetVariableAsync(string name, object value)
    {
        _logger.LogDebug("设置变量: {Name} = {Value}", name, value);
        return _executor.SetVariableAsync(name, value);
    }

    public Task<object?> GetVariableAsync(string name)
    {
        return _executor.GetVariableAsync(name);
    }

    public Task<Dictionary<string, object>> GetAllVariablesAsync()
    {
        return _executor.GetAllVariablesAsync();
    }
}

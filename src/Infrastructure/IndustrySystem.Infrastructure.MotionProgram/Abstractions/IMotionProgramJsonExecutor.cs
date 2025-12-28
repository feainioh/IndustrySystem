using IndustrySystem.Application.Contracts.Dtos.MotionProgram;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Infrastructure.MotionProgram.Abstractions;

/// <summary>
/// MotionProgram JSON 执行器接口
/// </summary>
public interface IMotionProgramJsonExecutor
{
    /// <summary>
    /// 当前执行状态
    /// </summary>
    ProgramExecutionState State { get; }
    
    /// <summary>
    /// 当前执行进度
    /// </summary>
    ExecutionProgress? CurrentProgress { get; }
    
    /// <summary>
    /// 执行进度变化事件
    /// </summary>
    event EventHandler<ExecutionProgress>? ProgressChanged;
    
    /// <summary>
    /// 节点执行完成事件
    /// </summary>
    event EventHandler<NodeExecutionResult>? NodeExecuted;
    
    /// <summary>
    /// 程序执行完成事件
    /// </summary>
    event EventHandler<bool>? ProgramCompleted;
    
    /// <summary>
    /// 从文件路径加载并执行程序
    /// </summary>
    Task LoadAndExecuteFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 从 JSON 字符串加载并执行程序
    /// </summary>
    Task LoadAndExecuteFromJsonAsync(string json, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 加载程序（从文件）
    /// </summary>
    Task LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 加载程序（从 JSON）
    /// </summary>
    Task LoadFromJsonAsync(string json, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 开始执行
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 暂停执行
    /// </summary>
    Task PauseAsync();
    
    /// <summary>
    /// 恢复执行
    /// </summary>
    Task ResumeAsync();
    
    /// <summary>
    /// 停止执行
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// 单步执行
    /// </summary>
    Task StepAsync();
    
    /// <summary>
    /// 跳转到指定节点
    /// </summary>
    Task GotoNodeAsync(Guid nodeId);
    
    /// <summary>
    /// 设置变量值
    /// </summary>
    Task SetVariableAsync(string name, object value);
    
    /// <summary>
    /// 获取变量值
    /// </summary>
    Task<object?> GetVariableAsync(string name);
    
    /// <summary>
    /// 获取所有变量
    /// </summary>
    Task<Dictionary<string, object>> GetAllVariablesAsync();
}

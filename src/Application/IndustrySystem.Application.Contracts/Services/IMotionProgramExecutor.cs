using IndustrySystem.Application.Contracts.Dtos.MotionProgram;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 程序执行状态
/// </summary>
public enum ProgramExecutionState
{
    /// <summary>空闲</summary>
    Idle,
    
    /// <summary>运行中</summary>
    Running,
    
    /// <summary>暂停</summary>
    Paused,
    
    /// <summary>停止</summary>
    Stopped,
    
    /// <summary>完成</summary>
    Completed,
    
    /// <summary>错误</summary>
    Error
}

/// <summary>
/// 执行进度信息
/// </summary>
public record ExecutionProgress(
    Guid ProgramId,
    string ProgramName,
    ProgramExecutionState State,
    Guid? CurrentNodeId,
    string CurrentNodeName,
    int ExecutedCount,
    int TotalCount,
    double ProgressPercent,
    string Message,
    DateTime StartTime,
    TimeSpan ElapsedTime,
    Exception? LastError
);

/// <summary>
/// 节点执行结果
/// </summary>
public record NodeExecutionResult(
    Guid NodeId,
    string NodeName,
    bool Success,
    string Message,
    TimeSpan Duration,
    Dictionary<string, object>? OutputData
);

/// <summary>
/// 程序执行引擎接口
/// </summary>
public interface IMotionProgramExecutor
{
    /// <summary>当前执行状态</summary>
    ProgramExecutionState State { get; }
    
    /// <summary>当前执行进度</summary>
    ExecutionProgress? CurrentProgress { get; }
    
    /// <summary>执行进度变化事件</summary>
    event EventHandler<ExecutionProgress>? ProgressChanged;
    
    /// <summary>节点执行完成事件</summary>
    event EventHandler<NodeExecutionResult>? NodeExecuted;
    
    /// <summary>程序执行完成事件</summary>
    event EventHandler<bool>? ProgramCompleted;
    
    /// <summary>加载程序</summary>
    Task LoadProgramAsync(MotionProgramDto program);
    
    /// <summary>加载程序（通过ID）</summary>
    Task LoadProgramAsync(Guid programId);
    
    /// <summary>开始执行</summary>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>暂停执行</summary>
    Task PauseAsync();
    
    /// <summary>继续执行</summary>
    Task ResumeAsync();
    
    /// <summary>停止执行</summary>
    Task StopAsync();
    
    /// <summary>单步执行</summary>
    Task StepAsync();
    
    /// <summary>跳转到指定节点</summary>
    Task GotoNodeAsync(Guid nodeId);
    
    /// <summary>设置变量值</summary>
    Task SetVariableAsync(string name, object value);
    
    /// <summary>获取变量值</summary>
    Task<object?> GetVariableAsync(string name);
    
    /// <summary>获取所有变量</summary>
    Task<Dictionary<string, object>> GetAllVariablesAsync();
}

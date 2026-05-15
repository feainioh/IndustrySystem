using IndustrySystem.Application.Contracts.Dtos.MotionProgram;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// </summary>
public enum ProgramExecutionState
{
    Idle,
    
    Running,
    
    Paused,
    
    /// <summary>ֹͣ</summary>
    Stopped,
    
    Completed,
    
    Error
}

/// <summary>
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
/// </summary>
public interface IMotionProgramExecutor
{
    ProgramExecutionState State { get; }
    
    ExecutionProgress? CurrentProgress { get; }
    
    event EventHandler<ExecutionProgress>? ProgressChanged;
    
    event EventHandler<NodeExecutionResult>? NodeExecuted;
    
    event EventHandler<bool>? ProgramCompleted;
    
    Task LoadProgramAsync(MotionProgramDto program);
    
    Task LoadProgramAsync(Guid programId);
    
    Task StartAsync(CancellationToken cancellationToken = default);
    
    Task PauseAsync();
    
    Task ResumeAsync();
    
    Task StopAsync();
    
    Task StepAsync();
    
    Task GotoNodeAsync(Guid nodeId);
    
    Task SetVariableAsync(string name, object value);
    
    Task<object?> GetVariableAsync(string name);
    
    Task<Dictionary<string, object>> GetAllVariablesAsync();
}

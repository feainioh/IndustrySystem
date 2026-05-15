namespace IndustrySystem.Application.Contracts.Dtos.MotionProgram;

/// <summary>
/// </summary>
public enum ActionType
{
    // Motor Control
    MotorMoveAbsolute,
    MotorMoveRelative,
    MotorHome,
    MotorStop,
    WaitMotorDone,
    
    // IO Control
    IoOutput,
    WaitIoInput,
    
    // Robot Control
    RobotMoveTo,
    RobotRunProgram,
    
    // Flow Control - Basic
    Delay,
    LoopStart,
    LoopEnd,
    Condition,
    CallSubProgram,
    SetVariable,
    Log,
    Alarm,
    
    // Flow Control - Logic
    IfStart,        // if 块开始
    ElseIf,         // else if 分支
    Else,           // else 分支
    IfEnd,          // if 块结束
    WhileStart,     // while 循环开始
    WhileEnd,       // while 循环结束
    Break,          // 跳出循环
    Continue,       // 进入下一次循环
    Return,         // 返回/结束流程
    Switch,         // switch 分支
    Case,           // case 分支
    Default,        // default 分支
    SwitchEnd,      // switch 块结束
    
    // Parallel
    ParallelStart,  // 并行执行开始
    ParallelEnd,    // 并行执行结束
    ParallelBranch  // 并行分支
}

/// <summary>
/// </summary>
public enum ErrorHandling
{
    Stop,
    Skip,
    Retry,
    Goto
}

/// <summary>
/// </summary>
public record MotionProgramDto(
    Guid Id,
    string Name,
    string Description,
    string Version,
    DateTime CreatedAt,
    DateTime ModifiedAt,
    string CreatedBy,
    string Category,
    bool IsSubProgram,
    List<ActionNodeDto> Nodes,
    List<NodeConnectionDto> Connections,
    Dictionary<string, object> Variables
);

/// <summary>
/// </summary>
public record ActionNodeDto(
    Guid Id,
    string Name,
    ActionType ActionType,
    Dictionary<string, object> Parameters,
    double X,
    double Y,
    double Width,
    double Height,
    string Description,
    bool IsEnabled,
    int TimeoutMs,
    int RetryCount,
    ErrorHandling ErrorHandling
);

/// <summary>
/// </summary>
public record NodeConnectionDto(
    Guid Id,
    Guid SourceNodeId,
    Guid TargetNodeId,
    int SourcePort,
    int TargetPort,
    string Condition,
    string Label
);

/// <summary>
/// </summary>
public record SaveMotionProgramRequest(
    Guid? Id,
    string Name,
    string Description,
    string Category,
    bool IsSubProgram,
    List<ActionNodeDto> Nodes,
    List<NodeConnectionDto> Connections,
    Dictionary<string, object> Variables
);

/// <summary>
/// </summary>
public record MotionProgramListItemDto(
    Guid Id,
    string Name,
    string Description,
    string Version,
    string Category,
    bool IsSubProgram,
    DateTime CreatedAt,
    DateTime ModifiedAt,
    string CreatedBy,
    int NodeCount
);

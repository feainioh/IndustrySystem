namespace IndustrySystem.Application.Contracts.Dtos.MotionProgram;

/// <summary>
/// 动作类型枚举
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
    IfStart,        // if 语句开始
    ElseIf,         // else if 分支
    Else,           // else 分支
    IfEnd,          // if 语句结束
    WhileStart,     // while 循环开始
    WhileEnd,       // while 循环结束
    Break,          // 跳出循环
    Continue,       // 继续下一次循环
    Return,         // 返回/结束程序
    Switch,         // switch 语句
    Case,           // case 分支
    Default,        // default 分支
    SwitchEnd,      // switch 语句结束
    
    // Parallel
    ParallelStart,  // 并行执行开始
    ParallelEnd,    // 并行执行结束
    ParallelBranch  // 并行分支
}

/// <summary>
/// 错误处理方式
/// </summary>
public enum ErrorHandling
{
    Stop,
    Skip,
    Retry,
    Goto
}

/// <summary>
/// 动作程序DTO
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
/// 动作节点DTO
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
/// 节点连接DTO
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
/// 创建/更新动作程序请求
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
/// 程序列表项DTO
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

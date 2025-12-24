namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 硬件控制器接口 - 用于执行引擎调用硬件操作
/// </summary>
public interface IHardwareController
{
    #region 电机控制
    
    /// <summary>移动电机到指定位置</summary>
    Task MoveMotorAsync(
        string motorId, 
        double position, 
        double speed, 
        bool relative = false,
        bool waitDone = true,
        CancellationToken ct = default);
    
    /// <summary>电机回原点</summary>
    Task HomeMotorAsync(string motorId, CancellationToken ct = default);
    
    /// <summary>停止电机</summary>
    Task StopMotorAsync(string motorId, CancellationToken ct = default);
    
    /// <summary>等待电机到位</summary>
    Task<bool> WaitMotorDoneAsync(string motorId, int timeoutMs, CancellationToken ct = default);
    
    /// <summary>获取电机当前位置</summary>
    Task<double> GetMotorPositionAsync(string motorId, CancellationToken ct = default);
    
    /// <summary>获取电机状态</summary>
    Task<MotorStatus> GetMotorStatusAsync(string motorId, CancellationToken ct = default);
    
    #endregion
    
    #region IO控制
    
    /// <summary>设置IO输出</summary>
    Task SetIoOutputAsync(string moduleId, int portIndex, bool value, CancellationToken ct = default);
    
    /// <summary>获取IO输入</summary>
    Task<bool> GetIoInputAsync(string moduleId, int portIndex, CancellationToken ct = default);
    
    /// <summary>等待IO输入状态</summary>
    Task<bool> WaitIoInputAsync(
        string moduleId, 
        int portIndex, 
        bool expectedValue, 
        int timeoutMs,
        CancellationToken ct = default);
    
    /// <summary>获取所有IO状态</summary>
    Task<IoModuleStatus> GetIoStatusAsync(string moduleId, CancellationToken ct = default);
    
    #endregion
    
    #region 机器人控制
    
    /// <summary>移动机器人到指定点位</summary>
    Task MoveRobotAsync(
        string robotId,
        string pointName,
        double[] coordinates,
        string moveType,
        int speedPercent,
        CancellationToken ct = default);
    
    /// <summary>执行机器人程序</summary>
    Task RunRobotProgramAsync(string robotId, string programName, CancellationToken ct = default);
    
    /// <summary>停止机器人</summary>
    Task StopRobotAsync(string robotId, CancellationToken ct = default);
    
    /// <summary>获取机器人状态</summary>
    Task<RobotStatus> GetRobotStatusAsync(string robotId, CancellationToken ct = default);
    
    #endregion
}

/// <summary>电机状态</summary>
public record MotorStatus(
    string MotorId,
    bool IsOnline,
    bool IsMoving,
    bool IsHomed,
    bool HasError,
    double CurrentPosition,
    double TargetPosition,
    double CurrentSpeed,
    string ErrorMessage
);

/// <summary>IO模块状态</summary>
public record IoModuleStatus(
    string ModuleId,
    bool IsOnline,
    bool[] Inputs,
    bool[] Outputs
);

/// <summary>机器人状态</summary>
public record RobotStatus(
    string RobotId,
    bool IsOnline,
    bool IsMoving,
    bool IsProgramRunning,
    bool HasError,
    double[] CurrentPosition,
    string CurrentProgram,
    string ErrorMessage
);

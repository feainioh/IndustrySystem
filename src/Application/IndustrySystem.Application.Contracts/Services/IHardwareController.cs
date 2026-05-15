namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// </summary>
public interface IHardwareController
{
    #region 电机控制
    
    Task MoveMotorAsync(
        string motorId, 
        double position, 
        double speed, 
        bool relative = false,
        bool waitDone = true,
        CancellationToken ct = default);
    
    Task HomeMotorAsync(string motorId, CancellationToken ct = default);
    
    Task StopMotorAsync(string motorId, CancellationToken ct = default);
    
    Task<bool> WaitMotorDoneAsync(string motorId, int timeoutMs, CancellationToken ct = default);
    
    Task<double> GetMotorPositionAsync(string motorId, CancellationToken ct = default);
    
    Task<MotorStatus> GetMotorStatusAsync(string motorId, CancellationToken ct = default);
    
    #endregion
    
    #region IO 控制
    
    Task SetIoOutputAsync(string moduleId, int portIndex, bool value, CancellationToken ct = default);
    
    Task<bool> GetIoInputAsync(string moduleId, int portIndex, CancellationToken ct = default);
    
    Task<bool> WaitIoInputAsync(
        string moduleId, 
        int portIndex, 
        bool expectedValue, 
        int timeoutMs,
        CancellationToken ct = default);
    
    Task<IoModuleStatus> GetIoStatusAsync(string moduleId, CancellationToken ct = default);
    
    #endregion
    
    #region 机器人控制
    
    Task MoveRobotAsync(
        string robotId,
        string pointName,
        double[] coordinates,
        string moveType,
        int speedPercent,
        CancellationToken ct = default);
    
    Task RunRobotProgramAsync(string robotId, string programName, CancellationToken ct = default);
    
    Task StopRobotAsync(string robotId, CancellationToken ct = default);
    
    Task<RobotStatus> GetRobotStatusAsync(string robotId, CancellationToken ct = default);
    
    #endregion
}

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

public record IoModuleStatus(
    string ModuleId,
    bool IsOnline,
    bool[] Inputs,
    bool[] Outputs
);

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

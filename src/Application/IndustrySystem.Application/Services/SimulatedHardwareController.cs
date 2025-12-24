using IndustrySystem.Application.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 硬件控制器模拟实现 - 用于开发测试
/// </summary>
public class SimulatedHardwareController : IHardwareController
{
    private readonly ILogger<SimulatedHardwareController> _logger;
    
    private readonly Dictionary<string, double> _motorPositions = new();
    private readonly Dictionary<string, bool> _motorMoving = new();
    private readonly Dictionary<string, bool[]> _ioOutputs = new();
    private readonly Dictionary<string, bool[]> _ioInputs = new();
    private readonly Random _random = new();

    public SimulatedHardwareController(ILogger<SimulatedHardwareController> logger)
    {
        _logger = logger;
        
        // 初始化一些默认设备
        _motorPositions["Motor1"] = 0;
        _motorPositions["Motor2"] = 0;
        _motorPositions["Motor3"] = 0;
        
        _ioOutputs["IO1"] = new bool[16];
        _ioInputs["IO1"] = new bool[16];
    }

    #region Motor Control

    public async Task MoveMotorAsync(
        string motorId, 
        double position, 
        double speed, 
        bool relative = false,
        bool waitDone = true,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Motor {MotorId} moving to {Position} at speed {Speed}", motorId, position, speed);
        
        var startPos = _motorPositions.GetValueOrDefault(motorId, 0);
        var targetPos = relative ? startPos + position : position;
        
        _motorMoving[motorId] = true;
        
        var distance = Math.Abs(targetPos - startPos);
        var duration = (int)(distance / (speed > 0 ? speed : 100) * 100);
        
        if (waitDone)
        {
            await Task.Delay(Math.Min(duration, 2000), ct);
        }
        
        _motorPositions[motorId] = targetPos;
        _motorMoving[motorId] = false;
        
        _logger.LogInformation("[SIM] Motor {MotorId} reached {Position}", motorId, targetPos);
    }

    public async Task HomeMotorAsync(string motorId, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Motor {MotorId} homing", motorId);
        
        _motorMoving[motorId] = true;
        await Task.Delay(1000, ct);
        _motorPositions[motorId] = 0;
        _motorMoving[motorId] = false;
        
        _logger.LogInformation("[SIM] Motor {MotorId} homed", motorId);
    }

    public Task StopMotorAsync(string motorId, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Motor {MotorId} stopped", motorId);
        _motorMoving[motorId] = false;
        return Task.CompletedTask;
    }

    public async Task<bool> WaitMotorDoneAsync(string motorId, int timeoutMs, CancellationToken ct = default)
    {
        var startTime = DateTime.Now;
        
        while (_motorMoving.GetValueOrDefault(motorId, false))
        {
            if ((DateTime.Now - startTime).TotalMilliseconds > timeoutMs)
            {
                _logger.LogWarning("[SIM] Motor {MotorId} wait timeout", motorId);
                return false;
            }
            
            await Task.Delay(50, ct);
        }
        
        return true;
    }

    public Task<double> GetMotorPositionAsync(string motorId, CancellationToken ct = default)
    {
        return Task.FromResult(_motorPositions.GetValueOrDefault(motorId, 0));
    }

    public Task<MotorStatus> GetMotorStatusAsync(string motorId, CancellationToken ct = default)
    {
        return Task.FromResult(new MotorStatus(
            motorId,
            IsOnline: true,
            IsMoving: _motorMoving.GetValueOrDefault(motorId, false),
            IsHomed: true,
            HasError: false,
            CurrentPosition: _motorPositions.GetValueOrDefault(motorId, 0),
            TargetPosition: _motorPositions.GetValueOrDefault(motorId, 0),
            CurrentSpeed: 0,
            ErrorMessage: ""
        ));
    }

    #endregion

    #region IO Control

    public Task SetIoOutputAsync(string moduleId, int portIndex, bool value, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] IO {ModuleId}:{Port} = {Value}", moduleId, portIndex, value);
        
        if (!_ioOutputs.ContainsKey(moduleId))
        {
            _ioOutputs[moduleId] = new bool[16];
        }
        
        if (portIndex >= 0 && portIndex < _ioOutputs[moduleId].Length)
        {
            _ioOutputs[moduleId][portIndex] = value;
        }
        
        return Task.CompletedTask;
    }

    public Task<bool> GetIoInputAsync(string moduleId, int portIndex, CancellationToken ct = default)
    {
        if (!_ioInputs.ContainsKey(moduleId))
        {
            _ioInputs[moduleId] = new bool[16];
        }
        
        if (_random.NextDouble() < 0.1)
        {
            _ioInputs[moduleId][portIndex] = !_ioInputs[moduleId][portIndex];
        }
        
        return Task.FromResult(
            portIndex >= 0 && portIndex < _ioInputs[moduleId].Length 
                ? _ioInputs[moduleId][portIndex] 
                : false);
    }

    public async Task<bool> WaitIoInputAsync(
        string moduleId, 
        int portIndex, 
        bool expectedValue, 
        int timeoutMs,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Waiting IO {ModuleId}:{Port} = {Value}", moduleId, portIndex, expectedValue);
        
        var startTime = DateTime.Now;
        
        // 模拟：一段时间后信号变为期望值
        await Task.Delay(Math.Min(timeoutMs / 2, 500), ct);
        
        if (!_ioInputs.ContainsKey(moduleId))
        {
            _ioInputs[moduleId] = new bool[16];
        }
        _ioInputs[moduleId][portIndex] = expectedValue;
        
        return true;
    }

    public Task<IoModuleStatus> GetIoStatusAsync(string moduleId, CancellationToken ct = default)
    {
        if (!_ioInputs.ContainsKey(moduleId))
        {
            _ioInputs[moduleId] = new bool[16];
        }
        if (!_ioOutputs.ContainsKey(moduleId))
        {
            _ioOutputs[moduleId] = new bool[16];
        }
        
        return Task.FromResult(new IoModuleStatus(
            moduleId,
            IsOnline: true,
            Inputs: _ioInputs[moduleId],
            Outputs: _ioOutputs[moduleId]
        ));
    }

    #endregion

    #region Robot Control

    public async Task MoveRobotAsync(
        string robotId,
        string pointName,
        double[] coordinates,
        string moveType,
        int speedPercent,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Robot {RobotId} moving to {Point} ({MoveType}) at {Speed}%", 
            robotId, pointName, moveType, speedPercent);
        
        var duration = 2000 * (100 - speedPercent) / 100 + 500;
        await Task.Delay(Math.Min(duration, 3000), ct);
        
        _logger.LogInformation("[SIM] Robot {RobotId} reached {Point}", robotId, pointName);
    }

    public async Task RunRobotProgramAsync(string robotId, string programName, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Robot {RobotId} running program: {Program}", robotId, programName);
        await Task.Delay(2000, ct);
        _logger.LogInformation("[SIM] Robot {RobotId} program completed: {Program}", robotId, programName);
    }

    public Task StopRobotAsync(string robotId, CancellationToken ct = default)
    {
        _logger.LogInformation("[SIM] Robot {RobotId} stopped", robotId);
        return Task.CompletedTask;
    }

    public Task<RobotStatus> GetRobotStatusAsync(string robotId, CancellationToken ct = default)
    {
        return Task.FromResult(new RobotStatus(
            robotId,
            IsOnline: true,
            IsMoving: false,
            IsProgramRunning: false,
            HasError: false,
            CurrentPosition: new double[6],
            CurrentProgram: "",
            ErrorMessage: ""
        ));
    }

    #endregion
}

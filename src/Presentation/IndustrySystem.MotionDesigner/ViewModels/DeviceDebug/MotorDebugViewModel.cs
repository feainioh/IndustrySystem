using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class MotorDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private MotorDto? _selectedMotor;
    private EtherCATMotorDto? _selectedEtherCATMotor;
    private double _motorPosition;
    private double _motorTargetPosition;
    private double _motorSpeed = 100;
    private bool _motorRelative;
    private MotorStatus? _motorStatus;
    private WorkPositionDto? _selectedWorkPosition;
    private string _motorUnit = string.Empty;
    private double _motorJogStep = 1;
    private bool _motorServoEnabled;
    private bool _motorOnline = true;
    private bool _motorHomed;
    private bool _motorHasAlarm;
    private string _motorStatusMessage = string.Empty;
    
    public MotorDto? SelectedMotor
    {
        get => _selectedMotor;
        set
        {
            if (SetProperty(ref _selectedMotor, value))
            {
                OnMotorChanged();
            }
        }
    }
    
    public EtherCATMotorDto? SelectedEtherCATMotor
    {
        get => _selectedEtherCATMotor;
        set
        {
            if (SetProperty(ref _selectedEtherCATMotor, value))
            {
                OnMotorChanged();
            }
        }
    }
    
    public double MotorPosition
    {
        get => _motorPosition;
        set => SetProperty(ref _motorPosition, value);
    }
    
    public double MotorTargetPosition
    {
        get => _motorTargetPosition;
        set => SetProperty(ref _motorTargetPosition, value);
    }
    
    public double MotorSpeed
    {
        get => _motorSpeed;
        set => SetProperty(ref _motorSpeed, value);
    }
    
    public bool MotorRelative
    {
        get => _motorRelative;
        set => SetProperty(ref _motorRelative, value);
    }
    
    public MotorStatus? MotorStatus
    {
        get => _motorStatus;
        set => SetProperty(ref _motorStatus, value);
    }
    
    public WorkPositionDto? SelectedWorkPosition
    {
        get => _selectedWorkPosition;
        set => SetProperty(ref _selectedWorkPosition, value);
    }
    
    public string MotorUnit
    {
        get => _motorUnit;
        set => SetProperty(ref _motorUnit, value);
    }
    
    public double MotorJogStep
    {
        get => _motorJogStep;
        set => SetProperty(ref _motorJogStep, value);
    }
    
    public bool MotorServoEnabled
    {
        get => _motorServoEnabled;
        set => SetProperty(ref _motorServoEnabled, value);
    }
    
    public bool MotorOnline
    {
        get => _motorOnline;
        set => SetProperty(ref _motorOnline, value);
    }
    
    public bool MotorHomed
    {
        get => _motorHomed;
        set => SetProperty(ref _motorHomed, value);
    }
    
    public bool MotorHasAlarm
    {
        get => _motorHasAlarm;
        set => SetProperty(ref _motorHasAlarm, value);
    }
    
    public string MotorStatusMessage
    {
        get => _motorStatusMessage;
        set => SetProperty(ref _motorStatusMessage, value);
    }
    
    public IEnumerable<WorkPositionDto> MotorWorkPositions
    {
        get
        {
            if (SelectedMotor?.WorkPositions != null && SelectedMotor.WorkPositions.Any())
                return SelectedMotor.WorkPositions;
            if (SelectedEtherCATMotor?.WorkPositions != null && SelectedEtherCATMotor.WorkPositions.Any())
                return SelectedEtherCATMotor.WorkPositions;
            return Enumerable.Empty<WorkPositionDto>();
        }
    }
    
    public ICommand MotorMoveCommand { get; }
    public ICommand MotorHomeCommand { get; }
    public ICommand MotorStopCommand { get; }
    public ICommand MotorGetPositionCommand { get; }
    public ICommand MotorJogPositiveCommand { get; }
    public ICommand MotorJogNegativeCommand { get; }
    public ICommand MotorContinuousJogCommand { get; }
    public ICommand MotorClearAlarmCommand { get; }
    public ICommand MotorServoOnCommand { get; }
    public ICommand MotorServoOffCommand { get; }
    public ICommand MotorResetCommand { get; }
    public ICommand MotorMoveToWorkPositionCommand { get; }
    
    public MotorDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        MotorMoveCommand = new DelegateCommand(async () => await MotorMoveAsync());
        MotorHomeCommand = new DelegateCommand(async () => await MotorHomeAsync());
        MotorStopCommand = new DelegateCommand(async () => await MotorStopAsync());
        MotorGetPositionCommand = new DelegateCommand(async () => await MotorGetPositionAsync());
        MotorJogPositiveCommand = new DelegateCommand(async () => await MotorJogAsync(true));
        MotorJogNegativeCommand = new DelegateCommand(async () => await MotorJogAsync(false));
        MotorContinuousJogCommand = new DelegateCommand(async () => await MotorContinuousJogAsync());
        MotorClearAlarmCommand = new DelegateCommand(async () => await MotorClearAlarmAsync());
        MotorServoOnCommand = new DelegateCommand(async () => await MotorServoAsync(true));
        MotorServoOffCommand = new DelegateCommand(async () => await MotorServoAsync(false));
        MotorResetCommand = new DelegateCommand(async () => await MotorResetAsync());
        MotorMoveToWorkPositionCommand = new DelegateCommand(async () => await MotorMoveToWorkPositionAsync());
    }
    
    private void OnMotorChanged()
    {
        MotorUnit = string.Empty;
        
        if (SelectedMotor != null && SelectedMotor.Parameters != null)
        {
            _motorSpeed = SelectedMotor.Parameters.JogSpeed;
            MotorUnit = SelectedMotor.Parameters.Unit;
            RaisePropertyChanged(nameof(MotorSpeed));
            SelectedWorkPosition = SelectedMotor.WorkPositions.FirstOrDefault();
        }
        else if (SelectedEtherCATMotor != null && SelectedEtherCATMotor.Parameters != null)
        {
            _motorSpeed = SelectedEtherCATMotor.Parameters.JogSpeed;
            MotorUnit = SelectedEtherCATMotor.Parameters.Unit;
            RaisePropertyChanged(nameof(MotorSpeed));
            SelectedWorkPosition = SelectedEtherCATMotor.WorkPositions.FirstOrDefault();
        }
        
        RaisePropertyChanged(nameof(MotorWorkPositions));
    }
    
    private async Task<bool> TryGetMotorTargetAsync(Func<string, string, Task> action)
    {
        string? deviceId = null;
        string? motorName = null;

        if (SelectedMotor != null)
        {
            deviceId = SelectedMotor.DeviceId;
            motorName = SelectedMotor.Name;
        }
        else if (SelectedEtherCATMotor != null)
        {
            deviceId = SelectedEtherCATMotor.DeviceId;
            motorName = SelectedEtherCATMotor.Name;
        }

        if (deviceId == null || motorName == null) return false;

        await action(deviceId, motorName);
        return true;
    }
    
    private async Task MotorMoveAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                MotorStatusMessage = $"正在移动电机 {motorName}...";
                await _hardwareController.MoveMotorAsync(
                    deviceId,
                    MotorTargetPosition,
                    MotorSpeed,
                    MotorRelative,
                    true);
                MotorStatusMessage = $"电机 {motorName} 已移动到位置 {MotorTargetPosition}";
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "移动电机失败");
                MotorStatusMessage = $"移动失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorHomeAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                MotorStatusMessage = $"正在回原点 {motorName}...";
                await _hardwareController.HomeMotorAsync(deviceId);
                MotorStatusMessage = $"电机 {motorName} 已回原点";
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "电机回原点失败");
                MotorStatusMessage = $"回原点失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorStopAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                await _hardwareController.StopMotorAsync(deviceId);
                MotorStatusMessage = $"电机 {motorName} 已停止";
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "停止电机失败");
                MotorStatusMessage = $"停止失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorGetPositionAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                MotorPosition = await _hardwareController.GetMotorPositionAsync(deviceId);
                MotorStatusMessage = $"电机 {motorName} 当前位置: {MotorPosition}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取电机位置失败");
                MotorStatusMessage = $"获取位置失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorJogAsync(bool positive)
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                var step = positive ? MotorJogStep : -MotorJogStep;
                MotorStatusMessage = $"正在{(positive ? "正向" : "反向")}JOG {motorName}...";
                await _hardwareController.MoveMotorAsync(deviceId, step, MotorSpeed, true, true);
                MotorStatusMessage = $"{motorName} JOG 完成";
                await RefreshStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "JOG 失败");
                MotorStatusMessage = $"JOG失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorContinuousJogAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                MotorStatusMessage = $"正在持续JOG {motorName} (模拟)...";
                await _hardwareController.MoveMotorAsync(deviceId, MotorRelative ? MotorJogStep : MotorTargetPosition, MotorSpeed, true, false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "持续JOG失败");
                MotorStatusMessage = $"持续JOG失败: {ex.Message}";
            }
        });
    }
    
    private async Task MotorClearAlarmAsync()
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            await Task.Delay(100);
            MotorStatusMessage = $"{motorName} 报警已清除 (模拟)";
        });
    }
    
    private async Task MotorServoAsync(bool enable)
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            MotorServoEnabled = enable;
            await Task.Delay(50);
            MotorStatusMessage = enable ? $"{motorName} 已上使能" : $"{motorName} 已下使能";
        });
    }
    
    private async Task MotorResetAsync()
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            await Task.Delay(80);
            MotorPosition = 0;
            MotorTargetPosition = 0;
            MotorStatusMessage = $"{motorName} 已复位";
        });
    }
    
    private async Task MotorMoveToWorkPositionAsync()
    {
        if (SelectedWorkPosition == null) return;
        MotorTargetPosition = SelectedWorkPosition.Position;
        MotorSpeed = SelectedWorkPosition.Speed;
        await MotorMoveAsync();
    }
    
    private async Task RefreshStatusAsync()
    {
        var deviceId = SelectedMotor?.DeviceId ?? SelectedEtherCATMotor?.DeviceId;
        if (deviceId == null) return;
        
        try
        {
            MotorStatus = await _hardwareController.GetMotorStatusAsync(deviceId);
            MotorPosition = MotorStatus.CurrentPosition;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取电机状态失败");
        }
    }
}

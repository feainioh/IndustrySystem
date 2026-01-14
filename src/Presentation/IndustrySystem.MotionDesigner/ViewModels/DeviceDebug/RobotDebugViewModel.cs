using System;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class RobotDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private JakaRobotDto? _selectedRobot;
    private bool _robotConnected;
    private bool _robotEnabled;
    private bool _robotMoving;
    private bool _robotHasAlarm;
    private string _robotCurrentTask = "空闲";
    private string _robotIp = string.Empty;
    private int _robotPort;
    private int _robotTaskNumber;
    private string _robotStatus = string.Empty;
    
    public JakaRobotDto? SelectedRobot
    {
        get => _selectedRobot;
        set
        {
            if (SetProperty(ref _selectedRobot, value))
            {
                OnRobotChanged();
            }
        }
    }
    
    public bool RobotConnected
    {
        get => _robotConnected;
        set => SetProperty(ref _robotConnected, value);
    }
    
    public bool RobotEnabled
    {
        get => _robotEnabled;
        set => SetProperty(ref _robotEnabled, value);
    }
    
    public bool RobotMoving
    {
        get => _robotMoving;
        set => SetProperty(ref _robotMoving, value);
    }
    
    public bool RobotHasAlarm
    {
        get => _robotHasAlarm;
        set => SetProperty(ref _robotHasAlarm, value);
    }
    
    public string RobotCurrentTask
    {
        get => _robotCurrentTask;
        set => SetProperty(ref _robotCurrentTask, value);
    }
    
    public string RobotIp
    {
        get => _robotIp;
        set => SetProperty(ref _robotIp, value);
    }
    
    public int RobotPort
    {
        get => _robotPort;
        set => SetProperty(ref _robotPort, value);
    }
    
    public int RobotTaskNumber
    {
        get => _robotTaskNumber;
        set => SetProperty(ref _robotTaskNumber, value);
    }
    
    public string RobotStatus
    {
        get => _robotStatus;
        set => SetProperty(ref _robotStatus, value);
    }
    
    public ICommand RobotConnectCommand { get; }
    public ICommand RobotDisconnectCommand { get; }
    public ICommand RobotEnableCommand { get; }
    public ICommand RobotDisableCommand { get; }
    public ICommand RobotClearAlarmCommand { get; }
    public ICommand RobotExecuteTaskCommand { get; }
    public ICommand RobotContinueCommand { get; }
    public ICommand RobotStopCommand { get; }
    public ICommand RobotMoveHomeCommand { get; }
    public ICommand RobotMoveSafeCommand { get; }
    public ICommand RobotPauseCommand { get; }
    public ICommand RobotResumeCommand { get; }
    
    public RobotDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        RobotConnectCommand = new DelegateCommand(async () => await RobotConnectAsync());
        RobotDisconnectCommand = new DelegateCommand(async () => await RobotDisconnectAsync());
        RobotEnableCommand = new DelegateCommand(async () => await RobotEnableAsync(true));
        RobotDisableCommand = new DelegateCommand(async () => await RobotEnableAsync(false));
        RobotClearAlarmCommand = new DelegateCommand(async () => await RobotClearAlarmAsync());
        RobotExecuteTaskCommand = new DelegateCommand(async () => await RobotExecuteTaskAsync());
        RobotContinueCommand = new DelegateCommand(async () => await RobotContinueAsync());
        RobotStopCommand = new DelegateCommand(async () => await RobotStopAsync());
        RobotMoveHomeCommand = new DelegateCommand(async () => await RobotMoveHomeAsync());
        RobotMoveSafeCommand = new DelegateCommand(async () => await RobotMoveSafeAsync());
        RobotPauseCommand = new DelegateCommand(async () => await RobotPauseAsync());
        RobotResumeCommand = new DelegateCommand(async () => await RobotResumeAsync());
    }
    
    private void OnRobotChanged()
    {
        if (SelectedRobot != null)
        {
            RobotIp = SelectedRobot.IpAddress;
            RobotPort = SelectedRobot.Port;
            RobotConnected = false;
            RobotEnabled = false;
            RobotMoving = false;
            RobotHasAlarm = false;
            RobotCurrentTask = "空闲";
            RobotStatus = string.Empty;
        }
    }
    
    private async Task RobotConnectAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(100);
        RobotConnected = true;
        RobotStatus = $"机器人 {SelectedRobot.Name} 已连接 ({RobotIp}:{RobotPort})";
    }
    
    private async Task RobotDisconnectAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(50);
        RobotConnected = false;
        RobotEnabled = false;
        RobotStatus = $"机器人 {SelectedRobot.Name} 已断开";
    }
    
    private async Task RobotEnableAsync(bool enable)
    {
        if (SelectedRobot == null) return;
        await Task.Delay(80);
        RobotEnabled = enable;
        RobotStatus = enable ? $"机器人 {SelectedRobot.Name} 已使能" : $"机器人 {SelectedRobot.Name} 已下使能";
    }
    
    private async Task RobotClearAlarmAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(60);
        RobotHasAlarm = false;
        RobotStatus = $"机器人 {SelectedRobot.Name} 报警已清除";
    }
    
    private async Task RobotExecuteTaskAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = $"任务 {RobotTaskNumber}";
        RobotStatus = $"机器人 {SelectedRobot.Name} 开始执行任务 {RobotTaskNumber}";
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }
    
    private async Task RobotContinueAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedRobot.Name} 继续执行";
    }
    
    private async Task RobotStopAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(50);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
        RobotStatus = $"机器人 {SelectedRobot.Name} 已停止";
    }
    
    private async Task RobotMoveHomeAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = "回原点";
        RobotStatus = $"机器人 {SelectedRobot.Name} 正在回原点";
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }
    
    private async Task RobotMoveSafeAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = "移动到安全位";
        RobotStatus = $"机器人 {SelectedRobot.Name} 正在移动到安全位";
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }
    
    private async Task RobotPauseAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedRobot.Name} 已暂停";
    }
    
    private async Task RobotResumeAsync()
    {
        if (SelectedRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedRobot.Name} 已恢复";
    }
}

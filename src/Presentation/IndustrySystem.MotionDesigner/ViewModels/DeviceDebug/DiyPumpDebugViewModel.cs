using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class DiyPumpDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private DiyPumpDto? _selectedPump;
    private bool _diyPumpConnected;
    private bool _diyPumpServoEnabled;
    private int _diyPumpChannel = 1;
    private string _diyPumpStatus = string.Empty;
    private double _diyPumpCurrentPosition;
    private double _diyPumpTargetPosition;
    private double _diyPumpTarget;
    private double _diyPumpSpeed = 30;
    private double _diyPumpJogStep = 10;
    private bool _diyPumpRelative;
    private bool _diyPumpIsRunning;
    
    public ReadOnlyCollection<int> DiyPumpChannelOptions { get; } = new(new[] { 1, 2, 3, 4 });
    
    public DiyPumpDto? SelectedPump
    {
        get => _selectedPump;
        set
        {
            if (SetProperty(ref _selectedPump, value))
            {
                OnPumpChanged();
            }
        }
    }
    
    public bool DiyPumpConnected
    {
        get => _diyPumpConnected;
        set => SetProperty(ref _diyPumpConnected, value);
    }
    
    public bool DiyPumpServoEnabled
    {
        get => _diyPumpServoEnabled;
        set => SetProperty(ref _diyPumpServoEnabled, value);
    }
    
    public int DiyPumpChannel
    {
        get => _diyPumpChannel;
        set => SetProperty(ref _diyPumpChannel, value);
    }
    
    public string DiyPumpStatus
    {
        get => _diyPumpStatus;
        set => SetProperty(ref _diyPumpStatus, value);
    }
    
    public double DiyPumpCurrentPosition
    {
        get => _diyPumpCurrentPosition;
        set => SetProperty(ref _diyPumpCurrentPosition, value);
    }
    
    public double DiyPumpTargetPosition
    {
        get => _diyPumpTargetPosition;
        set => SetProperty(ref _diyPumpTargetPosition, value);
    }
    
    public double DiyPumpTarget
    {
        get => _diyPumpTarget;
        set => SetProperty(ref _diyPumpTarget, value);
    }
    
    public double DiyPumpSpeed
    {
        get => _diyPumpSpeed;
        set => SetProperty(ref _diyPumpSpeed, value);
    }
    
    public double DiyPumpJogStep
    {
        get => _diyPumpJogStep;
        set => SetProperty(ref _diyPumpJogStep, value);
    }
    
    public bool DiyPumpRelative
    {
        get => _diyPumpRelative;
        set => SetProperty(ref _diyPumpRelative, value);
    }
    
    public bool DiyPumpIsRunning
    {
        get => _diyPumpIsRunning;
        set => SetProperty(ref _diyPumpIsRunning, value);
    }
    
    public ICommand DiyPumpConnectCommand { get; }
    public ICommand DiyPumpServoOnCommand { get; }
    public ICommand DiyPumpServoOffCommand { get; }
    public ICommand DiyPumpClearAlarmCommand { get; }
    public ICommand DiyPumpResetCommand { get; }
    public ICommand DiyPumpSwitchChannelCommand { get; }
    public ICommand DiyPumpMoveCommand { get; }
    public ICommand DiyPumpHomeCommand { get; }
    public ICommand DiyPumpStopCommand { get; }
    public ICommand DiyPumpJogPositiveCommand { get; }
    public ICommand DiyPumpJogNegativeCommand { get; }
    public ICommand DiyPumpQuickMoveCommand { get; }
    
    public DiyPumpDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        DiyPumpConnectCommand = new DelegateCommand(async () => await DiyPumpConnectAsync());
        DiyPumpServoOnCommand = new DelegateCommand(async () => await DiyPumpServoAsync(true));
        DiyPumpServoOffCommand = new DelegateCommand(async () => await DiyPumpServoAsync(false));
        DiyPumpClearAlarmCommand = new DelegateCommand(async () => await DiyPumpClearAlarmAsync());
        DiyPumpResetCommand = new DelegateCommand(async () => await DiyPumpResetAsync());
        DiyPumpSwitchChannelCommand = new DelegateCommand(async () => await DiyPumpSwitchChannelAsync());
        DiyPumpMoveCommand = new DelegateCommand(async () => await DiyPumpMoveAsync());
        DiyPumpHomeCommand = new DelegateCommand(async () => await DiyPumpHomeAsync());
        DiyPumpStopCommand = new DelegateCommand(async () => await DiyPumpStopAsync());
        DiyPumpJogPositiveCommand = new DelegateCommand(async () => await DiyPumpJogAsync(true));
        DiyPumpJogNegativeCommand = new DelegateCommand(async () => await DiyPumpJogAsync(false));
        DiyPumpQuickMoveCommand = new DelegateCommand<string>(async angle => await DiyPumpQuickMoveAsync(angle));
    }
    
    private void OnPumpChanged()
    {
        if (SelectedPump != null)
        {
            DiyPumpChannel = 1;
            DiyPumpConnected = false;
            DiyPumpServoEnabled = false;
            DiyPumpStatus = string.Empty;
        }
    }
    
    private async Task DiyPumpConnectAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        DiyPumpConnected = true;
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 已连接";
    }
    
    private async Task DiyPumpServoAsync(bool enable)
    {
        if (SelectedPump == null) return;
        await Task.Delay(50);
        DiyPumpServoEnabled = enable;
        DiyPumpStatus = enable ? $"自定义泵 {SelectedPump.Name} 已上使能" : $"自定义泵 {SelectedPump.Name} 已下使能";
    }
    
    private async Task DiyPumpClearAlarmAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(60);
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 报警已清除";
    }
    
    private async Task DiyPumpResetAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 已归零";
    }
    
    private async Task DiyPumpSwitchChannelAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(70);
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 切换到通道 {DiyPumpChannel}";
    }
    
    private async Task DiyPumpMoveAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(100);
        DiyPumpIsRunning = true;
        if (DiyPumpRelative)
        {
            DiyPumpCurrentPosition += DiyPumpTarget;
        }
        else
        {
            DiyPumpCurrentPosition = DiyPumpTarget;
        }
        DiyPumpTargetPosition = DiyPumpTarget;
        DiyPumpIsRunning = false;
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 移动到 {DiyPumpCurrentPosition}°";
    }
    
    private async Task DiyPumpHomeAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(100);
        DiyPumpCurrentPosition = 0;
        DiyPumpTargetPosition = 0;
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 已回零";
    }
    
    private async Task DiyPumpStopAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(50);
        DiyPumpIsRunning = false;
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} 已停止";
    }
    
    private async Task DiyPumpJogAsync(bool positive)
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        var step = positive ? DiyPumpJogStep : -DiyPumpJogStep;
        DiyPumpCurrentPosition += step;
        DiyPumpStatus = $"自定义泵 {SelectedPump.Name} JOG {(positive ? "+" : "-")}{Math.Abs(step)}°";
    }
    
    private async Task DiyPumpQuickMoveAsync(string? angle)
    {
        if (SelectedPump == null || string.IsNullOrEmpty(angle)) return;
        if (double.TryParse(angle, out var targetAngle))
        {
            DiyPumpTarget = targetAngle;
            DiyPumpRelative = false;
            await DiyPumpMoveAsync();
        }
    }
}

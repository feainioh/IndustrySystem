using System;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class PeristalticPumpDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private PeristalticPumpDto? _selectedPump;
    private double _peristalticFlowRate;
    private double _peristalticTotalVolume;
    private double _peristalticPosition;
    private double _peristalticTarget;
    private double _peristalticSpeed = 100;
    private double _peristalticJogStep = 1;
    private bool _peristalticRelative;
    private bool _peristalticServoEnabled;
    private string _peristalticStatus = string.Empty;
    private double _peristalticCurrentFlowRate;
    private bool _peristalticIsRunning;
    private bool _peristalticReverse;
    private bool _peristalticConnected;
    
    public PeristalticPumpDto? SelectedPump
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
    
    public double PeristalticFlowRate
    {
        get => _peristalticFlowRate;
        set => SetProperty(ref _peristalticFlowRate, value);
    }
    
    public double PeristalticTotalVolume
    {
        get => _peristalticTotalVolume;
        set => SetProperty(ref _peristalticTotalVolume, value);
    }
    
    public double PeristalticPosition
    {
        get => _peristalticPosition;
        set => SetProperty(ref _peristalticPosition, value);
    }
    
    public double PeristalticTarget
    {
        get => _peristalticTarget;
        set => SetProperty(ref _peristalticTarget, value);
    }
    
    public double PeristalticSpeed
    {
        get => _peristalticSpeed;
        set => SetProperty(ref _peristalticSpeed, value);
    }
    
    public double PeristalticJogStep
    {
        get => _peristalticJogStep;
        set => SetProperty(ref _peristalticJogStep, value);
    }
    
    public bool PeristalticRelative
    {
        get => _peristalticRelative;
        set => SetProperty(ref _peristalticRelative, value);
    }
    
    public bool PeristalticServoEnabled
    {
        get => _peristalticServoEnabled;
        set => SetProperty(ref _peristalticServoEnabled, value);
    }
    
    public string PeristalticStatus
    {
        get => _peristalticStatus;
        set => SetProperty(ref _peristalticStatus, value);
    }
    
    public double PeristalticCurrentFlowRate
    {
        get => _peristalticCurrentFlowRate;
        set => SetProperty(ref _peristalticCurrentFlowRate, value);
    }
    
    public bool PeristalticIsRunning
    {
        get => _peristalticIsRunning;
        set => SetProperty(ref _peristalticIsRunning, value);
    }
    
    public bool PeristalticReverse
    {
        get => _peristalticReverse;
        set => SetProperty(ref _peristalticReverse, value);
    }
    
    public bool PeristalticConnected
    {
        get => _peristalticConnected;
        set => SetProperty(ref _peristalticConnected, value);
    }
    
    public ICommand PeristalticStartByVolumeCommand { get; }
    public ICommand PeristalticStopCommand { get; }
    public ICommand PeristalticMoveCommand { get; }
    public ICommand PeristalticJogPositiveCommand { get; }
    public ICommand PeristalticJogNegativeCommand { get; }
    public ICommand PeristalticServoOnCommand { get; }
    public ICommand PeristalticServoOffCommand { get; }
    public ICommand PeristalticClearAlarmCommand { get; }
    public ICommand PeristalticResetCommand { get; }
    public ICommand PeristalticContinuousRunCommand { get; }
    
    public PeristalticPumpDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        PeristalticStartByVolumeCommand = new DelegateCommand(async () => await PeristalticStartByVolumeAsync());
        PeristalticStopCommand = new DelegateCommand(async () => await PeristalticStopAsync());
        PeristalticMoveCommand = new DelegateCommand(async () => await PeristalticMoveAsync());
        PeristalticJogPositiveCommand = new DelegateCommand(async () => await PeristalticJogAsync(true));
        PeristalticJogNegativeCommand = new DelegateCommand(async () => await PeristalticJogAsync(false));
        PeristalticServoOnCommand = new DelegateCommand(async () => await PeristalticServoAsync(true));
        PeristalticServoOffCommand = new DelegateCommand(async () => await PeristalticServoAsync(false));
        PeristalticClearAlarmCommand = new DelegateCommand(async () => await PeristalticClearAlarmAsync());
        PeristalticResetCommand = new DelegateCommand(async () => await PeristalticResetAsync());
        PeristalticContinuousRunCommand = new DelegateCommand(async () => await PeristalticContinuousRunAsync());
    }
    
    private void OnPumpChanged()
    {
        if (SelectedPump != null)
        {
            PeristalticFlowRate = SelectedPump.Parameters?.DefaultFlowRate ?? SelectedPump.MaxFlowRate;
        }
        PeristalticStatus = string.Empty;
    }
    
    private async Task PeristalticMoveAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(120);
        PeristalticPosition = PeristalticRelative ? PeristalticPosition + PeristalticTarget : PeristalticTarget;
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 运行到 {PeristalticPosition} ml";
    }
    
    private async Task PeristalticJogAsync(bool positive)
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        var step = positive ? PeristalticJogStep : -PeristalticJogStep;
        PeristalticPosition += step;
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} JOG {(positive ? "+" : "-")}{Math.Abs(step)} ml";
    }
    
    private async Task PeristalticServoAsync(bool enable)
    {
        if (SelectedPump == null) return;
        await Task.Delay(50);
        PeristalticServoEnabled = enable;
        PeristalticStatus = enable ? $"蠕动泵 {SelectedPump.Name} 已上使能" : $"蠕动泵 {SelectedPump.Name} 已下使能";
    }
    
    private async Task PeristalticClearAlarmAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(60);
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 报警已清除";
    }
    
    private async Task PeristalticResetAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        PeristalticPosition = 0;
        PeristalticTarget = 0;
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 已复位";
    }
    
    private async Task PeristalticStartByVolumeAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(150);
        PeristalticPosition += PeristalticTotalVolume;
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 按量 {PeristalticTotalVolume} ml 泵送完成";
    }
    
    private async Task PeristalticStopAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(60);
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 已停止";
    }
    
    private async Task PeristalticContinuousRunAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(100);
        PeristalticIsRunning = true;
        PeristalticCurrentFlowRate = PeristalticFlowRate;
        PeristalticStatus = $"蠕动泵 {SelectedPump.Name} 持续运行中 (流量: {PeristalticFlowRate} mL/min)";
    }
}

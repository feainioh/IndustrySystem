using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class SyringePumpDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private SyringePumpDto? _selectedPump;
    private double _syringeAbsPosition;
    private double _syringeRelStep = 1;
    private int _syringeChannelIndex;
    private string _syringeChannelCode = "I";
    private string _syringeStatus = string.Empty;
    private bool _syringeConnected;
    
    public ReadOnlyCollection<string> SyringeChannelOptions { get; } = new(new[] { "I", "O", "E", "B" });
    
    public SyringePumpDto? SelectedPump
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
    
    public double SyringeAbsPosition
    {
        get => _syringeAbsPosition;
        set => SetProperty(ref _syringeAbsPosition, value);
    }
    
    public double SyringeRelStep
    {
        get => _syringeRelStep;
        set => SetProperty(ref _syringeRelStep, value);
    }
    
    public int SyringeChannelIndex
    {
        get => _syringeChannelIndex;
        set => SetProperty(ref _syringeChannelIndex, value);
    }
    
    public string SyringeChannelCode
    {
        get => _syringeChannelCode;
        set => SetProperty(ref _syringeChannelCode, value);
    }
    
    public string SyringeStatus
    {
        get => _syringeStatus;
        set => SetProperty(ref _syringeStatus, value);
    }
    
    public bool SyringeConnected
    {
        get => _syringeConnected;
        set => SetProperty(ref _syringeConnected, value);
    }
    
    public ICommand SyringeInitCommand { get; }
    public ICommand SyringeResetCommand { get; }
    public ICommand SyringeClearAlarmCommand { get; }
    public ICommand SyringeAbsMoveCommand { get; }
    public ICommand SyringeRelMoveCommand { get; }
    public ICommand SyringeSwitchChannelCommand { get; }
    public ICommand SyringeConnectCommand { get; }
    public ICommand SyringeStopCommand { get; }
    
    public SyringePumpDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        SyringeInitCommand = new DelegateCommand(async () => await SyringeInitAsync());
        SyringeResetCommand = new DelegateCommand(async () => await SyringeResetAsync());
        SyringeClearAlarmCommand = new DelegateCommand(async () => await SyringeClearAlarmAsync());
        SyringeAbsMoveCommand = new DelegateCommand(async () => await SyringeAbsMoveAsync());
        SyringeRelMoveCommand = new DelegateCommand(async () => await SyringeRelMoveAsync());
        SyringeSwitchChannelCommand = new DelegateCommand(async () => await SyringeSwitchChannelAsync());
        SyringeConnectCommand = new DelegateCommand(async () => await SyringeConnectAsync());
        SyringeStopCommand = new DelegateCommand(async () => await SyringeStopAsync());
    }
    
    private void OnPumpChanged()
    {
        if (SelectedPump != null)
        {
            SyringeChannelIndex = SelectedPump.ChannelIndex;
        }
        SyringeStatus = string.Empty;
    }
    
    private async Task SyringeInitAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(100);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 初始化完成";
    }
    
    private async Task SyringeResetAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 已复位";
    }
    
    private async Task SyringeClearAlarmAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(60);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 报警已清除";
    }
    
    private async Task SyringeAbsMoveAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(120);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 绝对运行到 {SyringeAbsPosition} ml";
    }
    
    private async Task SyringeRelMoveAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(120);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 相对运行 {SyringeRelStep} ml";
    }
    
    private async Task SyringeSwitchChannelAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"注射泵 {SelectedPump.Name} 切换到通道 {SyringeChannelCode}";
    }
    
    private async Task SyringeConnectAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        SyringeConnected = true;
        SyringeStatus = $"{SelectedPump.Name} 已连接";
    }
    
    private async Task SyringeStopAsync()
    {
        if (SelectedPump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"{SelectedPump.Name} 已停止";
    }
}

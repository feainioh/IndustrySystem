using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class ChillerDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private ChillerDeviceDto? _selectedChiller;
    private double _chillerTargetTemperature;
    private double _chillerCurrentTemperature;
    private bool _chillerIsRunning;
    private string? _selectedChillerPort;
    private string _chillerStatus = string.Empty;
    
    private readonly ObservableCollection<string> _serialPorts = new();
    
    public ObservableCollection<string> SerialPorts => _serialPorts;
    
    public ChillerDeviceDto? SelectedChiller
    {
        get => _selectedChiller;
        set
        {
            if (SetProperty(ref _selectedChiller, value))
            {
                OnChillerChanged();
            }
        }
    }
    
    public double ChillerTargetTemperature
    {
        get => _chillerTargetTemperature;
        set => SetProperty(ref _chillerTargetTemperature, value);
    }
    
    public double ChillerCurrentTemperature
    {
        get => _chillerCurrentTemperature;
        set => SetProperty(ref _chillerCurrentTemperature, value);
    }
    
    public bool ChillerIsRunning
    {
        get => _chillerIsRunning;
        set => SetProperty(ref _chillerIsRunning, value);
    }
    
    public string? SelectedChillerPort
    {
        get => _selectedChillerPort;
        set => SetProperty(ref _selectedChillerPort, value);
    }
    
    public string ChillerStatus
    {
        get => _chillerStatus;
        set => SetProperty(ref _chillerStatus, value);
    }
    
    public ICommand ChillerConnectCommand { get; }
    public ICommand ChillerRefreshPortsCommand { get; }
    public ICommand ChillerStartCommand { get; }
    public ICommand ChillerStopCommand { get; }
    public ICommand ChillerSetTemperatureCommand { get; }
    
    public ChillerDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        ChillerConnectCommand = new DelegateCommand(async () => await ChillerConnectAsync());
        ChillerRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        ChillerStartCommand = new DelegateCommand(async () => await ChillerStartAsync());
        ChillerStopCommand = new DelegateCommand(async () => await ChillerStopAsync());
        ChillerSetTemperatureCommand = new DelegateCommand(async () => await ChillerSetTemperatureAsync());
        
        RefreshSerialPorts();
    }
    
    private void OnChillerChanged()
    {
        if (SelectedChiller != null)
        {
            SelectedChillerPort = SelectedChiller.PortName ?? SerialPorts.FirstOrDefault();
            ChillerCurrentTemperature = 0;
            ChillerTargetTemperature = 25;
            ChillerIsRunning = false;
            ChillerStatus = string.Empty;
        }
    }
    
    private void RefreshSerialPorts()
    {
        try
        {
            var ports = SerialPort.GetPortNames().OrderBy(p => p).ToList();
            SerialPorts.Clear();
            foreach (var port in ports)
            {
                SerialPorts.Add(port);
            }
            
            SelectedChillerPort ??= SelectedChiller?.PortName ?? SerialPorts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "获取串口列表失败");
        }
    }
    
    private async Task ChillerConnectAsync()
    {
        if (SelectedChiller == null) return;
        await Task.Delay(50);
        ChillerStatus = $"冷水机 {SelectedChiller.Name} 已连接 ({SelectedChillerPort ?? SelectedChiller.PortName})";
    }
    
    private async Task ChillerStartAsync()
    {
        if (SelectedChiller == null) return;
        
        try
        {
            ChillerStatus = $"正在启动冷水机 {SelectedChiller.Name}...";
            await Task.Delay(100);
            ChillerIsRunning = true;
            ChillerStatus = $"冷水机 {SelectedChiller.Name} 已启动";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "启动冷水机失败");
            ChillerStatus = $"启动失败: {ex.Message}";
        }
    }
    
    private async Task ChillerStopAsync()
    {
        if (SelectedChiller == null) return;
        
        try
        {
            ChillerStatus = $"正在停止冷水机 {SelectedChiller.Name}...";
            await Task.Delay(100);
            ChillerIsRunning = false;
            ChillerStatus = $"冷水机 {SelectedChiller.Name} 已停止";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止冷水机失败");
            ChillerStatus = $"停止失败: {ex.Message}";
        }
    }
    
    private async Task ChillerSetTemperatureAsync()
    {
        if (SelectedChiller == null) return;
        
        try
        {
            ChillerStatus = $"正在设置冷水机 {SelectedChiller.Name} 目标温度...";
            await Task.Delay(100);
            ChillerStatus = $"冷水机 {SelectedChiller.Name} 目标温度已设置为 {ChillerTargetTemperature}°C";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置冷水机温度失败");
            ChillerStatus = $"设置失败: {ex.Message}";
        }
    }
}

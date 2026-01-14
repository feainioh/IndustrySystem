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

public class WeightSensorDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private WeighingSensorDto? _selectedSensor;
    private bool _weightSensorConnected;
    private bool _weightSensorStable;
    private bool _weightSensorZeroed;
    private double _weightSensorCurrentWeight;
    private string? _selectedWeightSensorPort;
    private bool _weightSensorAutoRead;
    private int _weightSensorReadInterval = 500;
    private double _weightSensorTargetWeight;
    private double _weightSensorTolerance = 0.1;
    private double _weightSensorCalibrationWeight = 100;
    private string _weightSensorStatus = string.Empty;
    
    private readonly ObservableCollection<string> _serialPorts = new();
    
    public ObservableCollection<string> SerialPorts => _serialPorts;
    
    public WeighingSensorDto? SelectedSensor
    {
        get => _selectedSensor;
        set
        {
            if (SetProperty(ref _selectedSensor, value))
            {
                OnSensorChanged();
            }
        }
    }
    
    public bool WeightSensorConnected
    {
        get => _weightSensorConnected;
        set => SetProperty(ref _weightSensorConnected, value);
    }
    
    public bool WeightSensorStable
    {
        get => _weightSensorStable;
        set => SetProperty(ref _weightSensorStable, value);
    }
    
    public bool WeightSensorZeroed
    {
        get => _weightSensorZeroed;
        set => SetProperty(ref _weightSensorZeroed, value);
    }
    
    public double WeightSensorCurrentWeight
    {
        get => _weightSensorCurrentWeight;
        set => SetProperty(ref _weightSensorCurrentWeight, value);
    }
    
    public string? SelectedWeightSensorPort
    {
        get => _selectedWeightSensorPort;
        set => SetProperty(ref _selectedWeightSensorPort, value);
    }
    
    public bool WeightSensorAutoRead
    {
        get => _weightSensorAutoRead;
        set => SetProperty(ref _weightSensorAutoRead, value);
    }
    
    public int WeightSensorReadInterval
    {
        get => _weightSensorReadInterval;
        set => SetProperty(ref _weightSensorReadInterval, value);
    }
    
    public double WeightSensorTargetWeight
    {
        get => _weightSensorTargetWeight;
        set => SetProperty(ref _weightSensorTargetWeight, value);
    }
    
    public double WeightSensorTolerance
    {
        get => _weightSensorTolerance;
        set => SetProperty(ref _weightSensorTolerance, value);
    }
    
    public double WeightSensorCalibrationWeight
    {
        get => _weightSensorCalibrationWeight;
        set => SetProperty(ref _weightSensorCalibrationWeight, value);
    }
    
    public string WeightSensorStatus
    {
        get => _weightSensorStatus;
        set => SetProperty(ref _weightSensorStatus, value);
    }
    
    public ICommand WeightSensorConnectCommand { get; }
    public ICommand WeightSensorDisconnectCommand { get; }
    public ICommand WeightSensorReadCommand { get; }
    public ICommand WeightSensorZeroCommand { get; }
    public ICommand WeightSensorTareCommand { get; }
    public ICommand WeightSensorRefreshPortsCommand { get; }
    public ICommand WeightSensorStartMonitorCommand { get; }
    public ICommand WeightSensorStopMonitorCommand { get; }
    public ICommand WeightSensorCalibrateCommand { get; }
    
    public WeightSensorDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        WeightSensorConnectCommand = new DelegateCommand(async () => await WeightSensorConnectAsync());
        WeightSensorDisconnectCommand = new DelegateCommand(async () => await WeightSensorDisconnectAsync());
        WeightSensorReadCommand = new DelegateCommand(async () => await WeightSensorReadAsync());
        WeightSensorZeroCommand = new DelegateCommand(async () => await WeightSensorZeroAsync());
        WeightSensorTareCommand = new DelegateCommand(async () => await WeightSensorTareAsync());
        WeightSensorRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        WeightSensorStartMonitorCommand = new DelegateCommand(async () => await WeightSensorStartMonitorAsync());
        WeightSensorStopMonitorCommand = new DelegateCommand(async () => await WeightSensorStopMonitorAsync());
        WeightSensorCalibrateCommand = new DelegateCommand(async () => await WeightSensorCalibrateAsync());
        
        RefreshSerialPorts();
    }
    
    private void OnSensorChanged()
    {
        if (SelectedSensor != null)
        {
            SelectedWeightSensorPort = SelectedSensor.PortName ?? SerialPorts.FirstOrDefault();
            WeightSensorConnected = false;
            WeightSensorStable = false;
            WeightSensorZeroed = false;
            WeightSensorCurrentWeight = 0;
            WeightSensorStatus = string.Empty;
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
            
            SelectedWeightSensorPort ??= SelectedSensor?.PortName ?? SerialPorts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "获取串口列表失败");
        }
    }
    
    private async Task WeightSensorConnectAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(80);
        WeightSensorConnected = true;
        WeightSensorStatus = $"称重传感器 {SelectedSensor.Name} 已连接 ({SelectedWeightSensorPort ?? SelectedSensor.PortName})";
    }
    
    private async Task WeightSensorDisconnectAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(50);
        WeightSensorConnected = false;
        WeightSensorStatus = $"称重传感器 {SelectedSensor.Name} 已断开";
    }
    
    private async Task WeightSensorReadAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(80);
        WeightSensorCurrentWeight = Math.Round(Random.Shared.NextDouble() * 100, SelectedSensor.DecimalPlaces);
        WeightSensorStable = true;
        WeightSensorStatus = $"读取重量: {WeightSensorCurrentWeight} g";
    }
    
    private async Task WeightSensorZeroAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(100);
        WeightSensorCurrentWeight = 0;
        WeightSensorZeroed = true;
        WeightSensorStatus = $"称重传感器 {SelectedSensor.Name} 已清零";
    }
    
    private async Task WeightSensorTareAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(80);
        WeightSensorCurrentWeight = 0;
        WeightSensorStatus = $"称重传感器 {SelectedSensor.Name} 已去皮";
    }
    
    private async Task WeightSensorStartMonitorAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(50);
        WeightSensorStatus = $"开始监测目标重量 {WeightSensorTargetWeight} g (偏差: ±{WeightSensorTolerance} g)";
    }
    
    private async Task WeightSensorStopMonitorAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(50);
        WeightSensorStatus = $"停止重量监测";
    }
    
    private async Task WeightSensorCalibrateAsync()
    {
        if (SelectedSensor == null) return;
        await Task.Delay(200);
        WeightSensorStatus = $"称重传感器 {SelectedSensor.Name} 校准完成 (标准砝码: {WeightSensorCalibrationWeight} g)";
    }
}

using System;
using System.Collections.Generic;
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

public class CentrifugalDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private CentrifugalDeviceDto? _selectedDevice;
    private double _centrifugalSpeed;
    private double _centrifugalTime;
    private int _centrifugalRotorPosition = 1;
    private bool _centrifugalConnected;
    private bool _centrifugalRunning;
    private string _centrifugalStatus = string.Empty;
    private string? _selectedCentrifugalPort;
    private bool _centrifugalCompleted;
    
    private readonly ObservableCollection<string> _serialPorts = new();
    
    public ObservableCollection<string> SerialPorts => _serialPorts;
    
    public CentrifugalDeviceDto? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                OnDeviceChanged();
            }
        }
    }
    
    public double CentrifugalSpeed
    {
        get => _centrifugalSpeed;
        set => SetProperty(ref _centrifugalSpeed, value);
    }
    
    public double CentrifugalTime
    {
        get => _centrifugalTime;
        set => SetProperty(ref _centrifugalTime, value);
    }
    
    public int CentrifugalRotorPosition
    {
        get => _centrifugalRotorPosition;
        set => SetProperty(ref _centrifugalRotorPosition, value);
    }
    
    public bool CentrifugalConnected
    {
        get => _centrifugalConnected;
        set => SetProperty(ref _centrifugalConnected, value);
    }
    
    public bool CentrifugalRunning
    {
        get => _centrifugalRunning;
        set => SetProperty(ref _centrifugalRunning, value);
    }
    
    public string CentrifugalStatus
    {
        get => _centrifugalStatus;
        set => SetProperty(ref _centrifugalStatus, value);
    }
    
    public string? SelectedCentrifugalPort
    {
        get => _selectedCentrifugalPort;
        set => SetProperty(ref _selectedCentrifugalPort, value);
    }
    
    public bool CentrifugalCompleted
    {
        get => _centrifugalCompleted;
        set => SetProperty(ref _centrifugalCompleted, value);
    }
    
    public IEnumerable<WorkPositionDto> CentrifugalWorkPositions => SelectedDevice?.WorkPositions ?? Enumerable.Empty<WorkPositionDto>();
    
    public ICommand CentrifugalConnectCommand { get; }
    public ICommand CentrifugalSetSpeedCommand { get; }
    public ICommand CentrifugalSetTimeCommand { get; }
    public ICommand CentrifugalSetRotorPositionCommand { get; }
    public ICommand CentrifugalStartCommand { get; }
    public ICommand CentrifugalStopCommand { get; }
    public ICommand CentrifugalRefreshPortsCommand { get; }
    public ICommand CentrifugalOpenLidCommand { get; }
    public ICommand CentrifugalCloseLidCommand { get; }
    public ICommand CentrifugalClearAlarmCommand { get; }
    
    public CentrifugalDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        CentrifugalConnectCommand = new DelegateCommand(async () => await CentrifugalConnectAsync());
        CentrifugalSetSpeedCommand = new DelegateCommand(async () => await CentrifugalSetSpeedAsync());
        CentrifugalSetTimeCommand = new DelegateCommand(async () => await CentrifugalSetTimeAsync());
        CentrifugalSetRotorPositionCommand = new DelegateCommand(async () => await CentrifugalSetRotorPositionAsync());
        CentrifugalStartCommand = new DelegateCommand(async () => await CentrifugalStartAsync());
        CentrifugalStopCommand = new DelegateCommand(async () => await CentrifugalStopAsync());
        CentrifugalRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        CentrifugalOpenLidCommand = new DelegateCommand(async () => await CentrifugalOpenLidAsync());
        CentrifugalCloseLidCommand = new DelegateCommand(async () => await CentrifugalCloseLidAsync());
        CentrifugalClearAlarmCommand = new DelegateCommand(async () => await CentrifugalClearAlarmAsync());
        
        RefreshSerialPorts();
    }
    
    private void OnDeviceChanged()
    {
        if (SelectedDevice != null)
        {
            CentrifugalSpeed = SelectedDevice.DefaultParameters?.DefaultSpeed ?? SelectedDevice.Parameters?.MaxSpeed ?? 1000;
            CentrifugalTime = SelectedDevice.DefaultParameters?.DefaultTime ?? 60;
            CentrifugalRotorPosition = 1;
            CentrifugalConnected = false;
            CentrifugalRunning = false;
            CentrifugalStatus = string.Empty;
            SelectedCentrifugalPort = SelectedDevice.PortName ?? SerialPorts.FirstOrDefault();
        }
        RaisePropertyChanged(nameof(CentrifugalWorkPositions));
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
            
            SelectedCentrifugalPort ??= SelectedDevice?.PortName ?? SerialPorts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "获取串口列表失败");
        }
    }
    
    private async Task CentrifugalConnectAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(80);
        CentrifugalConnected = true;
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 已连接";
    }
    
    private async Task CentrifugalSetSpeedAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 转速设置为 {CentrifugalSpeed} RPM";
    }
    
    private async Task CentrifugalSetTimeAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 离心时间设置为 {CentrifugalTime} 秒";
    }
    
    private async Task CentrifugalSetRotorPositionAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 转子位置设置为 {CentrifugalRotorPosition}";
    }
    
    private async Task CentrifugalStartAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(100);
        CentrifugalRunning = true;
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 开始离心 (转速: {CentrifugalSpeed} RPM, 时间: {CentrifugalTime}秒, 位置: {CentrifugalRotorPosition})";
    }
    
    private async Task CentrifugalStopAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(80);
        CentrifugalRunning = false;
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 已停止";
    }
    
    private async Task CentrifugalOpenLidAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(100);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 盖子已打开";
    }
    
    private async Task CentrifugalCloseLidAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(100);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 盖子已关闭";
    }
    
    private async Task CentrifugalClearAlarmAsync()
    {
        if (SelectedDevice == null) return;
        await Task.Delay(80);
        CentrifugalStatus = $"离心机 {SelectedDevice.Name} 报警已清除";
    }
}

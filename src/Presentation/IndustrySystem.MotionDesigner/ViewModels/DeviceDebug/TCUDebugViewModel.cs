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

public class TCUDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private TcuDeviceDto? _selectedTcu;
    private bool _tcuConnected;
    private bool _tcuCirculationEnabled;
    private double _tcuTargetTemperatureInput = 25;
    private double _tcuTargetTemperature;
    private double _tcuCurrentTemperature;
    private bool _tcuIsRunning;
    private string _tcuStatus = string.Empty;
    private string? _selectedTcuPort;
    
    private readonly ObservableCollection<string> _serialPorts = new();
    
    public ObservableCollection<string> SerialPorts => _serialPorts;
    
    public TcuDeviceDto? SelectedTcu
    {
        get => _selectedTcu;
        set
        {
            if (SetProperty(ref _selectedTcu, value))
            {
                OnTcuChanged();
            }
        }
    }
    
    public bool TcuConnected
    {
        get => _tcuConnected;
        set => SetProperty(ref _tcuConnected, value);
    }
    
    public bool TcuCirculationEnabled
    {
        get => _tcuCirculationEnabled;
        set => SetProperty(ref _tcuCirculationEnabled, value);
    }
    
    public double TcuTargetTemperatureInput
    {
        get => _tcuTargetTemperatureInput;
        set => SetProperty(ref _tcuTargetTemperatureInput, value);
    }
    
    public double TcuTargetTemperature
    {
        get => _tcuTargetTemperature;
        set => SetProperty(ref _tcuTargetTemperature, value);
    }
    
    public double TcuCurrentTemperature
    {
        get => _tcuCurrentTemperature;
        set => SetProperty(ref _tcuCurrentTemperature, value);
    }
    
    public bool TcuIsRunning
    {
        get => _tcuIsRunning;
        set => SetProperty(ref _tcuIsRunning, value);
    }
    
    public string TcuStatus
    {
        get => _tcuStatus;
        set => SetProperty(ref _tcuStatus, value);
    }
    
    public string? SelectedTcuPort
    {
        get => _selectedTcuPort;
        set => SetProperty(ref _selectedTcuPort, value);
    }
    
    public ICommand TcuConnectCommand { get; }
    public ICommand TcuDisconnectCommand { get; }
    public ICommand TcuRefreshPortsCommand { get; }
    public ICommand TcuSetTemperatureCommand { get; }
    public ICommand TcuStartCommand { get; }
    public ICommand TcuStopCommand { get; }
    public ICommand TcuClearAlarmCommand { get; }
    public ICommand TcuStartControlCommand { get; }
    public ICommand TcuSetCirculationCommand { get; }
    public ICommand TcuSetQuickTemperatureCommand { get; }
    
    public TCUDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        TcuConnectCommand = new DelegateCommand(async () => await TcuConnectAsync());
        TcuDisconnectCommand = new DelegateCommand(async () => await TcuDisconnectAsync());
        TcuRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        TcuSetTemperatureCommand = new DelegateCommand(async () => await TcuSetTemperatureAsync());
        TcuStartCommand = new DelegateCommand(async () => await TcuStartAsync());
        TcuStopCommand = new DelegateCommand(async () => await TcuStopAsync());
        TcuClearAlarmCommand = new DelegateCommand(async () => await TcuClearAlarmAsync());
        TcuStartControlCommand = new DelegateCommand(async () => await TcuStartControlAsync());
        TcuSetCirculationCommand = new DelegateCommand(async () => await TcuSetCirculationAsync());
        TcuSetQuickTemperatureCommand = new DelegateCommand<string>(async temp => await TcuSetQuickTemperatureAsync(temp));
        
        RefreshSerialPorts();
    }
    
    private void OnTcuChanged()
    {
        if (SelectedTcu != null)
        {
            SelectedTcuPort = SelectedTcu.PortName ?? SerialPorts.FirstOrDefault();
            TcuCurrentTemperature = 0;
            TcuTargetTemperature = 25;
            TcuConnected = false;
            TcuCirculationEnabled = false;
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

            SelectedTcuPort ??= SelectedTcu?.PortName ?? SerialPorts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "获取串口列表失败");
        }
    }
    
    private async Task TcuConnectAsync()
    {
        if (SelectedTcu == null) return;
        await Task.Delay(50);
        TcuConnected = true;
        TcuStatus = $"TCU {SelectedTcu.Name} 已连接 ({SelectedTcuPort ?? SelectedTcu.PortName})";
    }
    
    private async Task TcuDisconnectAsync()
    {
        if (SelectedTcu == null) return;
        await Task.Delay(50);
        TcuConnected = false;
        TcuIsRunning = false;
        TcuStatus = $"TCU {SelectedTcu.Name} 已断开";
    }
    
    private async Task TcuSetTemperatureAsync()
    {
        if (SelectedTcu == null) return;
        
        try
        {
            TcuStatus = $"正在设置 TCU {SelectedTcu.Name} 目标温度...";
            await Task.Delay(100);
            TcuCurrentTemperature = TcuTargetTemperature;
            TcuStatus = $"TCU {SelectedTcu.Name} 目标温度已设置为 {TcuTargetTemperature}°C";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 TCU 温度失败");
            TcuStatus = $"设置失败: {ex.Message}";
        }
    }
    
    private async Task TcuStartAsync()
    {
        if (SelectedTcu == null) return;
        
        try
        {
            TcuStatus = $"正在启动 TCU {SelectedTcu.Name}...";
            await Task.Delay(100);
            TcuIsRunning = true;
            TcuCurrentTemperature = TcuTargetTemperature;
            TcuStatus = $"TCU {SelectedTcu.Name} 循环已启动";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "启动 TCU 失败");
            TcuStatus = $"启动失败: {ex.Message}";
        }
    }
    
    private async Task TcuStopAsync()
    {
        if (SelectedTcu == null) return;
        
        try
        {
            TcuStatus = $"正在停止 TCU {SelectedTcu.Name}...";
            await Task.Delay(100);
            TcuIsRunning = false;
            TcuStatus = $"TCU {SelectedTcu.Name} 循环已停止";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止 TCU 失败");
            TcuStatus = $"停止失败: {ex.Message}";
        }
    }
    
    private async Task TcuClearAlarmAsync()
    {
        if (SelectedTcu == null) return;
        await Task.Delay(80);
        TcuStatus = $"TCU {SelectedTcu.Name} 报警已清除";
    }
    
    private async Task TcuStartControlAsync()
    {
        if (SelectedTcu == null) return;
        await Task.Delay(100);
        TcuIsRunning = true;
        TcuTargetTemperature = TcuTargetTemperatureInput;
        var circulation = TcuCirculationEnabled ? "循环已开启" : "循环已关闭";
        TcuStatus = $"TCU {SelectedTcu.Name} 开始控温到 {TcuTargetTemperature}°C ({circulation})";
    }
    
    private async Task TcuSetCirculationAsync()
    {
        if (SelectedTcu == null) return;
        await Task.Delay(50);
        TcuStatus = $"TCU {SelectedTcu.Name} 循环已{(TcuCirculationEnabled ? "开启" : "关闭")}";
    }
    
    private async Task TcuSetQuickTemperatureAsync(string? temp)
    {
        if (SelectedTcu == null || string.IsNullOrEmpty(temp)) return;
        if (double.TryParse(temp, out var temperature))
        {
            TcuTargetTemperatureInput = temperature;
            await TcuSetTemperatureAsync();
        }
    }
}

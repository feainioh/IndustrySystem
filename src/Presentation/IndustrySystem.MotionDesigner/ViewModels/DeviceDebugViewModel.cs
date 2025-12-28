using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// 设备调试 ViewModel
/// </summary>
public class DeviceDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    private readonly IDeviceConfigService _configService;
    private readonly IHardwareController _hardwareController;
    
    private DeviceConfigDto? _currentConfig;
    private string _selectedDeviceType = "Motor";
    private object? _selectedDevice;
    private string _statusMessage = "就绪";
    private bool _isDeviceSelected;
    
    public DeviceConfigDto? CurrentConfig
    {
        get => _currentConfig;
        set => SetProperty(ref _currentConfig, value);
    }
    
    public string SelectedDeviceType
    {
        get => _selectedDeviceType;
        set => SetProperty(ref _selectedDeviceType, value);
    }
    
    public object? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            SetProperty(ref _selectedDevice, value);
            IsDeviceSelected = value != null;
        }
    }
    
    public bool IsDeviceSelected
    {
        get => _isDeviceSelected;
        set => SetProperty(ref _isDeviceSelected, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }
    
    // 设备列表
    public ObservableCollection<MotorDto> Motors { get; } = new();
    public ObservableCollection<EtherCATMotorDto> EtherCATMotors { get; } = new();
    public ObservableCollection<SyringePumpDto> SyringePumps { get; } = new();
    public ObservableCollection<PeristalticPumpDto> PeristalticPumps { get; } = new();
    public ObservableCollection<DiyPumpDto> DiyPumps { get; } = new();
    public ObservableCollection<JakaRobotDto> JakaRobots { get; } = new();
    public ObservableCollection<TcuDeviceDto> TcuDevices { get; } = new();
    public ObservableCollection<CentrifugalDeviceDto> CentrifugalDevices { get; } = new();
    public ObservableCollection<WeighingSensorDto> WeighingSensors { get; } = new();
    public ObservableCollection<TwoChannelValveDto> TwoChannelValves { get; } = new();
    public ObservableCollection<ThreeChannelValveDto> ThreeChannelValves { get; } = new();
    public ObservableCollection<EcatIODeviceDto> EcatIODevices { get; } = new();
    
    // 电机调试属性
    private double _motorPosition;
    private double _motorTargetPosition;
    private double _motorSpeed = 100;
    private bool _motorRelative;
    private MotorStatus? _motorStatus;
    
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
    
    // IO 调试属性
    private int _ioChannelIndex;
    private bool _ioOutputValue;
    private bool _ioInputValue;
    
    public int IoChannelIndex
    {
        get => _ioChannelIndex;
        set => SetProperty(ref _ioChannelIndex, value);
    }
    
    public bool IoOutputValue
    {
        get => _ioOutputValue;
        set => SetProperty(ref _ioOutputValue, value);
    }
    
    public bool IoInputValue
    {
        get => _ioInputValue;
        set => SetProperty(ref _ioInputValue, value);
    }
    
    // 命令
    public ICommand ImportConfigCommand { get; }
    public ICommand RefreshDeviceStatusCommand { get; }
    public ICommand MotorMoveCommand { get; }
    public ICommand MotorHomeCommand { get; }
    public ICommand MotorStopCommand { get; }
    public ICommand MotorGetPositionCommand { get; }
    public ICommand IoSetOutputCommand { get; }
    public ICommand IoGetInputCommand { get; }
    
    public DeviceDebugViewModel(IDeviceConfigService configService, IHardwareController hardwareController)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        
        ImportConfigCommand = new DelegateCommand(async () => await ImportConfigAsync());
        RefreshDeviceStatusCommand = new DelegateCommand(async () => await RefreshDeviceStatusAsync());
        MotorMoveCommand = new DelegateCommand(async () => await MotorMoveAsync());
        MotorHomeCommand = new DelegateCommand(async () => await MotorHomeAsync());
        MotorStopCommand = new DelegateCommand(async () => await MotorStopAsync());
        MotorGetPositionCommand = new DelegateCommand(async () => await MotorGetPositionAsync());
        IoSetOutputCommand = new DelegateCommand(async () => await IoSetOutputAsync());
        IoGetInputCommand = new DelegateCommand(async () => await IoGetInputAsync());
    }
    
    private async Task ImportConfigAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "导入设备配置文件"
        };
        
        if (dialog.ShowDialog() != true) return;
        
        try
        {
            StatusMessage = "正在导入配置...";
            var config = await _configService.ImportFromFileAsync(dialog.FileName);
            CurrentConfig = config;
            
            // 更新设备列表
            Motors.Clear();
            EtherCATMotors.Clear();
            SyringePumps.Clear();
            PeristalticPumps.Clear();
            DiyPumps.Clear();
            JakaRobots.Clear();
            TcuDevices.Clear();
            CentrifugalDevices.Clear();
            WeighingSensors.Clear();
            TwoChannelValves.Clear();
            ThreeChannelValves.Clear();
            EcatIODevices.Clear();
            
            foreach (var motor in config.Motors)
                Motors.Add(motor);
            foreach (var motor in config.EtherCATMotors)
                EtherCATMotors.Add(motor);
            foreach (var pump in config.SyringePumps)
                SyringePumps.Add(pump);
            foreach (var pump in config.PeristalticPumps)
                PeristalticPumps.Add(pump);
            foreach (var pump in config.DiyPumps)
                DiyPumps.Add(pump);
            foreach (var robot in config.JakaRobots)
                JakaRobots.Add(robot);
            foreach (var tcu in config.TcuDevices)
                TcuDevices.Add(tcu);
            foreach (var cent in config.CentrifugalDevices)
                CentrifugalDevices.Add(cent);
            foreach (var sensor in config.WeighingSensors)
                WeighingSensors.Add(sensor);
            foreach (var valve in config.TwoChannelValves)
                TwoChannelValves.Add(valve);
            foreach (var valve in config.ThreeChannelValves)
                ThreeChannelValves.Add(valve);
            foreach (var io in config.EcatIODevices)
                EcatIODevices.Add(io);
            
            StatusMessage = $"成功导入配置，共 {Motors.Count + EtherCATMotors.Count} 个电机";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导入设备配置失败");
            StatusMessage = $"导入失败: {ex.Message}";
        }
    }
    
    private async Task RefreshDeviceStatusAsync()
    {
        if (SelectedDevice is MotorDto motor)
        {
            try
            {
                MotorStatus = await _hardwareController.GetMotorStatusAsync(motor.DeviceId);
                MotorPosition = MotorStatus.CurrentPosition;
                StatusMessage = $"电机 {motor.Name} 状态已更新";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "获取电机状态失败");
                StatusMessage = $"获取状态失败: {ex.Message}";
            }
        }
    }
    
    private async Task MotorMoveAsync()
    {
        if (SelectedDevice is not MotorDto motor) return;
        
        try
        {
            StatusMessage = $"正在移动电机 {motor.Name}...";
            await _hardwareController.MoveMotorAsync(
                motor.DeviceId,
                MotorTargetPosition,
                MotorSpeed,
                MotorRelative,
                true);
            StatusMessage = $"电机 {motor.Name} 已移动到位置 {MotorTargetPosition}";
            await RefreshDeviceStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "移动电机失败");
            StatusMessage = $"移动失败: {ex.Message}";
        }
    }
    
    private async Task MotorHomeAsync()
    {
        if (SelectedDevice is not MotorDto motor) return;
        
        try
        {
            StatusMessage = $"正在回原点 {motor.Name}...";
            await _hardwareController.HomeMotorAsync(motor.DeviceId);
            StatusMessage = $"电机 {motor.Name} 已回原点";
            await RefreshDeviceStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "电机回原点失败");
            StatusMessage = $"回原点失败: {ex.Message}";
        }
    }
    
    private async Task MotorStopAsync()
    {
        if (SelectedDevice is not MotorDto motor) return;
        
        try
        {
            await _hardwareController.StopMotorAsync(motor.DeviceId);
            StatusMessage = $"电机 {motor.Name} 已停止";
            await RefreshDeviceStatusAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止电机失败");
            StatusMessage = $"停止失败: {ex.Message}";
        }
    }
    
    private async Task MotorGetPositionAsync()
    {
        if (SelectedDevice is not MotorDto motor) return;
        
        try
        {
            MotorPosition = await _hardwareController.GetMotorPositionAsync(motor.DeviceId);
            StatusMessage = $"电机 {motor.Name} 当前位置: {MotorPosition}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取电机位置失败");
            StatusMessage = $"获取位置失败: {ex.Message}";
        }
    }
    
    private async Task IoSetOutputAsync()
    {
        if (SelectedDevice is not EcatIODeviceDto ioDevice) return;
        
        try
        {
            await _hardwareController.SetIoOutputAsync(ioDevice.DeviceId, IoChannelIndex, IoOutputValue);
            StatusMessage = $"IO {ioDevice.Name} 通道 {IoChannelIndex} 已设置为 {IoOutputValue}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 IO 输出失败");
            StatusMessage = $"设置失败: {ex.Message}";
        }
    }
    
    private async Task IoGetInputAsync()
    {
        if (SelectedDevice is not EcatIODeviceDto ioDevice) return;
        
        try
        {
            IoInputValue = await _hardwareController.GetIoInputAsync(ioDevice.DeviceId, IoChannelIndex);
            StatusMessage = $"IO {ioDevice.Name} 通道 {IoChannelIndex} 输入值: {IoInputValue}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取 IO 输入失败");
            StatusMessage = $"获取失败: {ex.Message}";
        }
    }
}

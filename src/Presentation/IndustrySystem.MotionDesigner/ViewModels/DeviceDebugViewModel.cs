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
    private DeviceItemViewModel? _selectedDevice;
    private string _statusMessage = "就绪";
    private bool _isDeviceSelected;
    private string _searchText = string.Empty;
    
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
    
    public DeviceItemViewModel? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                IsDeviceSelected = value != null;
                UpdateDeviceDetails();
                RaisePropertyChanged(nameof(SelectedMotor));
                RaisePropertyChanged(nameof(SelectedEtherCATMotor));
                RaisePropertyChanged(nameof(SelectedSyringePump));
                RaisePropertyChanged(nameof(SelectedPeristalticPump));
                RaisePropertyChanged(nameof(SelectedDiyPump));
                RaisePropertyChanged(nameof(SelectedJakaRobot));
                RaisePropertyChanged(nameof(SelectedTcuDevice));
                RaisePropertyChanged(nameof(SelectedCentrifugalDevice));
                RaisePropertyChanged(nameof(SelectedWeighingSensor));
                RaisePropertyChanged(nameof(SelectedTwoChannelValve));
                RaisePropertyChanged(nameof(SelectedThreeChannelValve));
                RaisePropertyChanged(nameof(SelectedEcatIODevice));
                RaisePropertyChanged(nameof(SelectedChillerDevice));
                RaisePropertyChanged(nameof(SelectedCustomModbusDevice));
                RaisePropertyChanged(nameof(IsMotorSelected));
                RaisePropertyChanged(nameof(IsEtherCATMotorSelected));
                RaisePropertyChanged(nameof(IsPumpSelected));
                RaisePropertyChanged(nameof(IsRobotSelected));
                RaisePropertyChanged(nameof(IsTcuSelected));
                RaisePropertyChanged(nameof(IsIODeviceSelected));
                RaisePropertyChanged(nameof(IsChillerSelected));
                RaisePropertyChanged(nameof(IsCustomModbusSelected));
            }
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
    
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterDevices();
            }
        }
    }
    
    // 分组设备列表 - 用于 TreeView 显示
    public ObservableCollection<DeviceCategoryViewModel> DeviceCategories { get; } = new();
    
    // 设备列表 - 统一设备项
    public ObservableCollection<DeviceItemViewModel> AllDevices { get; } = new();
    public ObservableCollection<DeviceItemViewModel> FilteredDevices { get; } = new();
    
    // 原有设备列表
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
    public ObservableCollection<ChillerDeviceDto> ChillerDevices { get; } = new();
    public ObservableCollection<CustomModbusDeviceDto> CustomModbusDevices { get; } = new();
    
    // 设备类型判断属性
    public bool IsMotorSelected => SelectedDevice?.OriginalDevice is MotorDto;
    public bool IsEtherCATMotorSelected => SelectedDevice?.OriginalDevice is EtherCATMotorDto;
    public bool IsPumpSelected => SelectedDevice?.OriginalDevice is SyringePumpDto or PeristalticPumpDto or DiyPumpDto;
    public bool IsRobotSelected => SelectedDevice?.OriginalDevice is JakaRobotDto;
    public bool IsTcuSelected => SelectedDevice?.OriginalDevice is TcuDeviceDto;
    public bool IsIODeviceSelected => SelectedDevice?.OriginalDevice is EcatIODeviceDto;
    public bool IsChillerSelected => SelectedDevice?.OriginalDevice is ChillerDeviceDto;
    public bool IsCustomModbusSelected => SelectedDevice?.OriginalDevice is CustomModbusDeviceDto;
    
    // 选中设备的具体类型
    public MotorDto? SelectedMotor => SelectedDevice?.OriginalDevice as MotorDto;
    public EtherCATMotorDto? SelectedEtherCATMotor => SelectedDevice?.OriginalDevice as EtherCATMotorDto;
    public SyringePumpDto? SelectedSyringePump => SelectedDevice?.OriginalDevice as SyringePumpDto;
    public PeristalticPumpDto? SelectedPeristalticPump => SelectedDevice?.OriginalDevice as PeristalticPumpDto;
    public DiyPumpDto? SelectedDiyPump => SelectedDevice?.OriginalDevice as DiyPumpDto;
    public JakaRobotDto? SelectedJakaRobot => SelectedDevice?.OriginalDevice as JakaRobotDto;
    public TcuDeviceDto? SelectedTcuDevice => SelectedDevice?.OriginalDevice as TcuDeviceDto;
    public CentrifugalDeviceDto? SelectedCentrifugalDevice => SelectedDevice?.OriginalDevice as CentrifugalDeviceDto;
    public WeighingSensorDto? SelectedWeighingSensor => SelectedDevice?.OriginalDevice as WeighingSensorDto;
    public TwoChannelValveDto? SelectedTwoChannelValve => SelectedDevice?.OriginalDevice as TwoChannelValveDto;
    public ThreeChannelValveDto? SelectedThreeChannelValve => SelectedDevice?.OriginalDevice as ThreeChannelValveDto;
    public EcatIODeviceDto? SelectedEcatIODevice => SelectedDevice?.OriginalDevice as EcatIODeviceDto;
    public ChillerDeviceDto? SelectedChillerDevice => SelectedDevice?.OriginalDevice as ChillerDeviceDto;
    public CustomModbusDeviceDto? SelectedCustomModbusDevice => SelectedDevice?.OriginalDevice as CustomModbusDeviceDto;
    
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
    
    // TCU 温控属性
    private double _tcuTargetTemperature;
    private double _tcuCurrentTemperature;
    private bool _tcuIsRunning;
    
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
    
    // 冷水机属性
    private double _chillerTargetTemperature;
    private double _chillerCurrentTemperature;
    private bool _chillerIsRunning;
    
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
    
    // 命令
    public ICommand ImportConfigCommand { get; }
    public ICommand RefreshDeviceStatusCommand { get; }
    public ICommand MotorMoveCommand { get; }
    public ICommand MotorHomeCommand { get; }
    public ICommand MotorStopCommand { get; }
    public ICommand MotorGetPositionCommand { get; }
    public ICommand IoSetOutputCommand { get; }
    public ICommand IoGetInputCommand { get; }
    public ICommand TcuStartCommand { get; }
    public ICommand TcuStopCommand { get; }
    public ICommand TcuSetTemperatureCommand { get; }
    public ICommand ChillerStartCommand { get; }
    public ICommand ChillerStopCommand { get; }
    public ICommand ChillerSetTemperatureCommand { get; }
    public ICommand ConnectDeviceCommand { get; }
    public ICommand DisconnectDeviceCommand { get; }
    
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
        TcuStartCommand = new DelegateCommand(async () => await TcuStartAsync());
        TcuStopCommand = new DelegateCommand(async () => await TcuStopAsync());
        TcuSetTemperatureCommand = new DelegateCommand(async () => await TcuSetTemperatureAsync());
        ChillerStartCommand = new DelegateCommand(async () => await ChillerStartAsync());
        ChillerStopCommand = new DelegateCommand(async () => await ChillerStopAsync());
        ChillerSetTemperatureCommand = new DelegateCommand(async () => await ChillerSetTemperatureAsync());
        ConnectDeviceCommand = new DelegateCommand(async () => await ConnectDeviceAsync());
        DisconnectDeviceCommand = new DelegateCommand(async () => await DisconnectDeviceAsync());
    }
    
    private void FilterDevices()
    {
        FilteredDevices.Clear();
        var search = SearchText?.ToLower() ?? string.Empty;
        
        // 过滤平面设备列表
        foreach (var device in AllDevices)
        {
            if (string.IsNullOrEmpty(search) ||
                device.Name.ToLower().Contains(search) ||
                device.DeviceId.ToLower().Contains(search) ||
                device.DeviceType.ToLower().Contains(search) ||
                device.Category.ToLower().Contains(search))
            {
                FilteredDevices.Add(device);
            }
        }
        
        // 更新分类中的设备可见性（用于搜索时展开匹配的分类）
        foreach (var category in DeviceCategories)
        {
            var hasMatch = string.IsNullOrEmpty(search) || 
                category.Devices.Any(d => 
                    d.Name.ToLower().Contains(search) ||
                    d.DeviceId.ToLower().Contains(search) ||
                    d.DeviceType.ToLower().Contains(search));
            
            // 如果有匹配项且正在搜索，展开分类
            if (hasMatch && !string.IsNullOrEmpty(search))
            {
                category.IsExpanded = true;
            }
        }
        
        RaisePropertyChanged(nameof(DeviceCategories));
    }
    
    private void UpdateDeviceDetails()
    {
        if (SelectedDevice == null) return;
        
        // 根据设备类型更新显示的详细信息
        switch (SelectedDevice.OriginalDevice)
        {
            case MotorDto motor:
                if (motor.Parameters != null)
                {
                    MotorSpeed = motor.Parameters.JogSpeed;
                }
                break;
            case EtherCATMotorDto ecatMotor:
                if (ecatMotor.Parameters != null)
                {
                    MotorSpeed = ecatMotor.Parameters.JogSpeed;
                }
                break;
        }
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
            
            // 清空所有列表
            ClearAllDeviceLists();
            
            // 更新设备列表
            PopulateDeviceLists(config);
            
            // 更新统一设备列表
            BuildUnifiedDeviceList(config);
            
            FilterDevices();
            
            StatusMessage = $"成功导入配置，共 {AllDevices.Count} 个设备";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导入设备配置失败");
            StatusMessage = $"导入失败: {ex.Message}";
        }
    }
    
    private void ClearAllDeviceLists()
    {
        AllDevices.Clear();
        FilteredDevices.Clear();
        DeviceCategories.Clear();
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
        ChillerDevices.Clear();
        CustomModbusDevices.Clear();
    }
    
    private void PopulateDeviceLists(DeviceConfigDto config)
    {
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
        foreach (var chiller in config.ChillerDevices)
            ChillerDevices.Add(chiller);
        foreach (var custom in config.CustomModbusDevices)
            CustomModbusDevices.Add(custom);
    }
    
    private void BuildUnifiedDeviceList(DeviceConfigDto config)
    {
        // 清空分组列表
        DeviceCategories.Clear();
        
        // 定义分组顺序和图标
        var categoryDefinitions = new Dictionary<string, (string DisplayName, string Icon, int Order)>
        {
            ["CAN通信主设备"] = ("CAN通信主设备", "Connection", 1),
            ["EtherCAT通信主设备"] = ("EtherCAT通信主设备", "LanConnect", 2),
            ["CAN电机"] = ("CAN电机", "Engine", 3),
            ["EtherCAT电机"] = ("EtherCAT电机", "Engine", 4),
            ["注射泵"] = ("注射泵", "Flask", 5),
            ["蠕动泵"] = ("蠕动泵", "WaterPump", 6),
            ["自定义泵"] = ("自定义泵", "WaterPump", 7),
            ["机器人"] = ("机器人", "Robot", 8),
            ["TCU温控"] = ("TCU温控", "Thermometer", 9),
            ["冷水机"] = ("冷水机", "Snowflake", 10),
            ["离心机"] = ("离心机", "Fan", 11),
            ["称重传感器"] = ("称重传感器", "Scale", 12),
            ["二通阀"] = ("二通阀", "Valve", 13),
            ["三通阀"] = ("三通阀", "Valve", 14),
            ["IO模块"] = ("IO模块", "ChipIo", 15),
            ["扫码枪"] = ("扫码枪", "Barcode", 16),
            ["自定义Modbus设备"] = ("自定义Modbus设备", "Devices", 17),
        };
        
        var categories = new Dictionary<string, DeviceCategoryViewModel>();
        
        // CAN 通信主设备
        foreach (var can in config.CanDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = can.DeviceId,
                Name = can.Name,
                DeviceType = "CAN通信主设备",
                Category = "CAN通信主设备",
                IsEnabled = true,
                IconKind = "Connection",
                OriginalDevice = can
            };
            AllDevices.Add(item);
            AddToCategory(categories, "CAN通信主设备", item, categoryDefinitions);
        }
        
        // EtherCAT 通信主设备
        foreach (var ecat in config.EtherCATDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = ecat.DeviceId,
                Name = ecat.Name,
                DeviceType = "EtherCAT通信主设备",
                Category = "EtherCAT通信主设备",
                IsEnabled = true,
                IconKind = "LanConnect",
                OriginalDevice = ecat
            };
            AllDevices.Add(item);
            AddToCategory(categories, "EtherCAT通信主设备", item, categoryDefinitions);
        }
        
        // CAN 电机
        foreach (var motor in config.Motors)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = motor.DeviceId,
                Name = motor.Name,
                DeviceType = "CAN电机",
                Category = "CAN电机",
                IsEnabled = true,
                IconKind = "Engine",
                OriginalDevice = motor
            };
            AllDevices.Add(item);
            AddToCategory(categories, "CAN电机", item, categoryDefinitions);
        }
        
        // EtherCAT 电机
        foreach (var motor in config.EtherCATMotors)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = motor.DeviceId,
                Name = motor.Name,
                DeviceType = "EtherCAT电机",
                Category = "EtherCAT电机",
                IsEnabled = true,
                IconKind = "Engine",
                OriginalDevice = motor
            };
            AllDevices.Add(item);
            AddToCategory(categories, "EtherCAT电机", item, categoryDefinitions);
        }
        
        // 注射泵
        foreach (var pump in config.SyringePumps)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = pump.DeviceId,
                Name = pump.Name,
                DeviceType = "注射泵",
                Category = "注射泵",
                IsEnabled = true,
                IconKind = "Flask",
                OriginalDevice = pump
            };
            AllDevices.Add(item);
            AddToCategory(categories, "注射泵", item, categoryDefinitions);
        }
        
        // 蠕动泵
        foreach (var pump in config.PeristalticPumps)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = pump.DeviceId,
                Name = pump.Name,
                DeviceType = "蠕动泵",
                Category = "蠕动泵",
                IsEnabled = pump.IsEnabled,
                IconKind = "WaterPump",
                OriginalDevice = pump
            };
            AllDevices.Add(item);
            AddToCategory(categories, "蠕动泵", item, categoryDefinitions);
        }
        
        // 自定义泵
        foreach (var pump in config.DiyPumps)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = pump.DeviceId,
                Name = pump.Name,
                DeviceType = "自定义泵",
                Category = "自定义泵",
                IsEnabled = pump.IsEnabled,
                IconKind = "WaterPump",
                OriginalDevice = pump
            };
            AllDevices.Add(item);
            AddToCategory(categories, "自定义泵", item, categoryDefinitions);
        }
        
        // 机器人
        foreach (var robot in config.JakaRobots)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = robot.DeviceId,
                Name = robot.Name,
                DeviceType = "Jaka机器人",
                Category = "机器人",
                IsEnabled = robot.IsEnabled,
                IconKind = "Robot",
                OriginalDevice = robot
            };
            AllDevices.Add(item);
            AddToCategory(categories, "机器人", item, categoryDefinitions);
        }
        
        // TCU 温控
        foreach (var tcu in config.TcuDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = tcu.DeviceId,
                Name = tcu.Name,
                DeviceType = "TCU温控",
                Category = "TCU温控",
                IsEnabled = tcu.IsEnabled,
                IconKind = "Thermometer",
                OriginalDevice = tcu
            };
            AllDevices.Add(item);
            AddToCategory(categories, "TCU温控", item, categoryDefinitions);
        }
        
        // 冷水机
        foreach (var chiller in config.ChillerDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = chiller.DeviceId,
                Name = chiller.Name,
                DeviceType = "冷水机",
                Category = "冷水机",
                IsEnabled = chiller.IsEnabled,
                IconKind = "Snowflake",
                OriginalDevice = chiller
            };
            AllDevices.Add(item);
            AddToCategory(categories, "冷水机", item, categoryDefinitions);
        }
        
        // 离心机
        foreach (var cent in config.CentrifugalDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = cent.DeviceId,
                Name = cent.Name,
                DeviceType = "离心机",
                Category = "离心机",
                IsEnabled = cent.IsEnabled,
                IconKind = "Fan",
                OriginalDevice = cent
            };
            AllDevices.Add(item);
            AddToCategory(categories, "离心机", item, categoryDefinitions);
        }
        
        // 称重传感器
        foreach (var sensor in config.WeighingSensors)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = sensor.DeviceId,
                Name = sensor.Name,
                DeviceType = "称重传感器",
                Category = "称重传感器",
                IsEnabled = sensor.IsEnabled,
                IconKind = "Scale",
                OriginalDevice = sensor
            };
            AllDevices.Add(item);
            AddToCategory(categories, "称重传感器", item, categoryDefinitions);
        }
        
        // 二通阀
        foreach (var valve in config.TwoChannelValves)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = valve.DeviceId,
                Name = valve.Name,
                DeviceType = "二通阀",
                Category = "二通阀",
                IsEnabled = valve.IsEnabled,
                IconKind = "Valve",
                OriginalDevice = valve
            };
            AllDevices.Add(item);
            AddToCategory(categories, "二通阀", item, categoryDefinitions);
        }
        
        // 三通阀
        foreach (var valve in config.ThreeChannelValves)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = valve.DeviceId,
                Name = valve.Name,
                DeviceType = "三通阀",
                Category = "三通阀",
                IsEnabled = valve.IsEnabled,
                IconKind = "Valve",
                OriginalDevice = valve
            };
            AllDevices.Add(item);
            AddToCategory(categories, "三通阀", item, categoryDefinitions);
        }
        
        // EtherCAT IO - 不在左侧单独显示每个通道，只显示IO设备整体
        foreach (var io in config.EcatIODevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = io.DeviceId,
                Name = io.Name,
                DeviceType = "EtherCAT IO",
                Category = "IO模块",
                IsEnabled = io.IsEnabled,
                IconKind = "ChipIo",
                OriginalDevice = io
            };
            AllDevices.Add(item);
            AddToCategory(categories, "IO模块", item, categoryDefinitions);
        }
        
        // 扫码枪
        foreach (var scanner in config.Scanners)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = scanner.DeviceId,
                Name = scanner.Name,
                DeviceType = "扫码枪",
                Category = "扫码枪",
                IsEnabled = scanner.IsEnabled,
                IconKind = "Barcode",
                OriginalDevice = scanner
            };
            AllDevices.Add(item);
            AddToCategory(categories, "扫码枪", item, categoryDefinitions);
        }
        
        // 自定义Modbus设备
        foreach (var custom in config.CustomModbusDevices)
        {
            var item = new DeviceItemViewModel
            {
                DeviceId = custom.DeviceId,
                Name = custom.Name,
                DeviceType = custom.DeviceType,
                Category = "自定义Modbus设备",
                IsEnabled = custom.IsEnabled,
                IconKind = "Devices",
                OriginalDevice = custom
            };
            AllDevices.Add(item);
            AddToCategory(categories, "自定义Modbus设备", item, categoryDefinitions);
        }
        
        // 按顺序添加到 DeviceCategories
        var orderedCategories = categories.Values
            .OrderBy(c => categoryDefinitions.TryGetValue(c.CategoryName, out var def) ? def.Order : 999)
            .ToList();
        
        foreach (var category in orderedCategories)
        {
            DeviceCategories.Add(category);
        }
    }
    
    private void AddToCategory(
        Dictionary<string, DeviceCategoryViewModel> categories, 
        string categoryName, 
        DeviceItemViewModel item,
        Dictionary<string, (string DisplayName, string Icon, int Order)> categoryDefinitions)
    {
        if (!categories.TryGetValue(categoryName, out var category))
        {
            var icon = categoryDefinitions.TryGetValue(categoryName, out var def) ? def.Icon : "Folder";
            category = new DeviceCategoryViewModel
            {
                CategoryName = categoryName,
                IconKind = icon,
                IsExpanded = true
            };
            categories[categoryName] = category;
        }
        category.Devices.Add(item);
    }
    
    private async Task RefreshDeviceStatusAsync()
    {
        if (SelectedDevice?.OriginalDevice is MotorDto motor)
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
        else if (SelectedDevice?.OriginalDevice is EtherCATMotorDto ecatMotor)
        {
            try
            {
                MotorStatus = await _hardwareController.GetMotorStatusAsync(ecatMotor.DeviceId);
                MotorPosition = MotorStatus.CurrentPosition;
                StatusMessage = $"电机 {ecatMotor.Name} 状态已更新";
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
        string? deviceId = null;
        string? motorName = null;
        
        if (SelectedDevice?.OriginalDevice is MotorDto motor)
        {
            deviceId = motor.DeviceId;
            motorName = motor.Name;
        }
        else if (SelectedDevice?.OriginalDevice is EtherCATMotorDto ecatMotor)
        {
            deviceId = ecatMotor.DeviceId;
            motorName = ecatMotor.Name;
        }
        
        if (deviceId == null) return;
        
        try
        {
            StatusMessage = $"正在移动电机 {motorName}...";
            await _hardwareController.MoveMotorAsync(
                deviceId,
                MotorTargetPosition,
                MotorSpeed,
                MotorRelative,
                true);
            StatusMessage = $"电机 {motorName} 已移动到位置 {MotorTargetPosition}";
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
        string? deviceId = null;
        string? motorName = null;
        
        if (SelectedDevice?.OriginalDevice is MotorDto motor)
        {
            deviceId = motor.DeviceId;
            motorName = motor.Name;
        }
        else if (SelectedDevice?.OriginalDevice is EtherCATMotorDto ecatMotor)
        {
            deviceId = ecatMotor.DeviceId;
            motorName = ecatMotor.Name;
        }
        
        if (deviceId == null) return;
        
        try
        {
            StatusMessage = $"正在回原点 {motorName}...";
            await _hardwareController.HomeMotorAsync(deviceId);
            StatusMessage = $"电机 {motorName} 已回原点";
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
        string? deviceId = null;
        string? motorName = null;
        
        if (SelectedDevice?.OriginalDevice is MotorDto motor)
        {
            deviceId = motor.DeviceId;
            motorName = motor.Name;
        }
        else if (SelectedDevice?.OriginalDevice is EtherCATMotorDto ecatMotor)
        {
            deviceId = ecatMotor.DeviceId;
            motorName = ecatMotor.Name;
        }
        
        if (deviceId == null) return;
        
        try
        {
            await _hardwareController.StopMotorAsync(deviceId);
            StatusMessage = $"电机 {motorName} 已停止";
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
        string? deviceId = null;
        string? motorName = null;
        
        if (SelectedDevice?.OriginalDevice is MotorDto motor)
        {
            deviceId = motor.DeviceId;
            motorName = motor.Name;
        }
        else if (SelectedDevice?.OriginalDevice is EtherCATMotorDto ecatMotor)
        {
            deviceId = ecatMotor.DeviceId;
            motorName = ecatMotor.Name;
        }
        
        if (deviceId == null) return;
        
        try
        {
            MotorPosition = await _hardwareController.GetMotorPositionAsync(deviceId);
            StatusMessage = $"电机 {motorName} 当前位置: {MotorPosition}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "获取电机位置失败");
            StatusMessage = $"获取位置失败: {ex.Message}";
        }
    }
    
    private async Task IoSetOutputAsync()
    {
        if (SelectedDevice?.OriginalDevice is not EcatIODeviceDto ioDevice) return;
        
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
        if (SelectedDevice?.OriginalDevice is not EcatIODeviceDto ioDevice) return;
        
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
    
    private async Task TcuStartAsync()
    {
        if (SelectedDevice?.OriginalDevice is not TcuDeviceDto tcu) return;
        
        try
        {
            StatusMessage = $"正在启动 TCU {tcu.Name}...";
            // TODO: 实现 TCU 启动逻辑
            await Task.Delay(100);
            TcuIsRunning = true;
            StatusMessage = $"TCU {tcu.Name} 已启动";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "启动 TCU 失败");
            StatusMessage = $"启动失败: {ex.Message}";
        }
    }
    
    private async Task TcuStopAsync()
    {
        if (SelectedDevice?.OriginalDevice is not TcuDeviceDto tcu) return;
        
        try
        {
            StatusMessage = $"正在停止 TCU {tcu.Name}...";
            // TODO: 实现 TCU 停止逻辑
            await Task.Delay(100);
            TcuIsRunning = false;
            StatusMessage = $"TCU {tcu.Name} 已停止";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止 TCU 失败");
            StatusMessage = $"停止失败: {ex.Message}";
        }
    }
    
    private async Task TcuSetTemperatureAsync()
    {
        if (SelectedDevice?.OriginalDevice is not TcuDeviceDto tcu) return;
        
        try
        {
            StatusMessage = $"正在设置 TCU {tcu.Name} 目标温度...";
            // TODO: 实现设置温度逻辑
            await Task.Delay(100);
            StatusMessage = $"TCU {tcu.Name} 目标温度已设置为 {TcuTargetTemperature}°C";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 TCU 温度失败");
            StatusMessage = $"设置失败: {ex.Message}";
        }
    }
    
    private async Task ChillerStartAsync()
    {
        if (SelectedDevice?.OriginalDevice is not ChillerDeviceDto chiller) return;
        
        try
        {
            StatusMessage = $"正在启动冷水机 {chiller.Name}...";
            // TODO: 实现冷水机启动逻辑
            await Task.Delay(100);
            ChillerIsRunning = true;
            StatusMessage = $"冷水机 {chiller.Name} 已启动";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "启动冷水机失败");
            StatusMessage = $"启动失败: {ex.Message}";
        }
    }
    
    private async Task ChillerStopAsync()
    {
        if (SelectedDevice?.OriginalDevice is not ChillerDeviceDto chiller) return;
        
        try
        {
            StatusMessage = $"正在停止冷水机 {chiller.Name}...";
            // TODO: 实现冷水机停止逻辑
            await Task.Delay(100);
            ChillerIsRunning = false;
            StatusMessage = $"冷水机 {chiller.Name} 已停止";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "停止冷水机失败");
            StatusMessage = $"停止失败: {ex.Message}";
        }
    }
    
    private async Task ChillerSetTemperatureAsync()
    {
        if (SelectedDevice?.OriginalDevice is not ChillerDeviceDto chiller) return;
        
        try
        {
            StatusMessage = $"正在设置冷水机 {chiller.Name} 目标温度...";
            // TODO: 实现设置温度逻辑
            await Task.Delay(100);
            StatusMessage = $"冷水机 {chiller.Name} 目标温度已设置为 {ChillerTargetTemperature}°C";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置冷水机温度失败");
            StatusMessage = $"设置失败: {ex.Message}";
        }
    }
    
    private async Task ConnectDeviceAsync()
    {
        if (SelectedDevice == null) return;
        
        try
        {
            StatusMessage = $"正在连接 {SelectedDevice.Name}...";
            // TODO: 实现设备连接逻辑
            await Task.Delay(100);
            StatusMessage = $"{SelectedDevice.Name} 已连接";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "连接设备失败");
            StatusMessage = $"连接失败: {ex.Message}";
        }
    }
    
    private async Task DisconnectDeviceAsync()
    {
        if (SelectedDevice == null) return;
        
        try
        {
            StatusMessage = $"正在断开 {SelectedDevice.Name}...";
            // TODO: 实现设备断开逻辑
            await Task.Delay(100);
            StatusMessage = $"{SelectedDevice.Name} 已断开";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "断开设备失败");
            StatusMessage = $"断开失败: {ex.Message}";
        }
    }
}

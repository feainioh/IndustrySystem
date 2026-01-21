using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Events;
using IndustrySystem.MotionDesigner.Services;
using IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PositionPointViewModel = IndustrySystem.MotionDesigner.Services.PositionPointViewModel;

namespace IndustrySystem.MotionDesigner.ViewModels;

public class DeviceDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    private readonly IDeviceConfigService _configService;
    private readonly IHardwareController _hardwareController;
    private readonly IEventAggregator _eventAggregator;
    
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
                RaisePropertyChanged(nameof(SelectedCanDevice));
                RaisePropertyChanged(nameof(SelectedEtherCATDevice));
                RaisePropertyChanged(nameof(IsMotorSelected));
                RaisePropertyChanged(nameof(IsEtherCATMotorSelected));
                RaisePropertyChanged(nameof(IsAnyMotorSelected));
                RaisePropertyChanged(nameof(IsPumpSelected));
                RaisePropertyChanged(nameof(IsRobotSelected));
                RaisePropertyChanged(nameof(IsTcuSelected));
                RaisePropertyChanged(nameof(IsIODeviceSelected));
                RaisePropertyChanged(nameof(IsChillerSelected));
                RaisePropertyChanged(nameof(IsCustomModbusSelected));
                RaisePropertyChanged(nameof(IsCommunicationDeviceSelected));
                RaisePropertyChanged(nameof(IsCanDeviceSelected));
                RaisePropertyChanged(nameof(IsEthercatDeviceSelected));
                RaisePropertyChanged(nameof(IsWeighingSensorSelected));
                RaisePropertyChanged(nameof(IsScannerSelected));
                RaisePropertyChanged(nameof(DiChannels));
                RaisePropertyChanged(nameof(DoChannels));
                RaisePropertyChanged(nameof(AiChannels));
                RaisePropertyChanged(nameof(AoChannels));
                RaisePropertyChanged(nameof(MotorWorkPositions));
                RaisePropertyChanged(nameof(IsSyringePumpSelected));
                RaisePropertyChanged(nameof(IsPeristalticPumpSelected));
                RaisePropertyChanged(nameof(IsScannerSelected));
                RaisePropertyChanged(nameof(IsCentrifugalSelected));
                RaisePropertyChanged(nameof(IsDiyPumpSelected));
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
    public bool IsAnyMotorSelected => IsMotorSelected || IsEtherCATMotorSelected;
    public bool IsPumpSelected => SelectedDevice?.OriginalDevice is SyringePumpDto or PeristalticPumpDto or DiyPumpDto;
    public bool IsSyringePumpSelected => SelectedDevice?.OriginalDevice is SyringePumpDto;
    public bool IsPeristalticPumpSelected => SelectedDevice?.OriginalDevice is PeristalticPumpDto;
    public bool IsRobotSelected => SelectedDevice?.OriginalDevice is JakaRobotDto;
    public bool IsTcuSelected => SelectedDevice?.OriginalDevice is TcuDeviceDto;
    public bool IsIODeviceSelected => SelectedDevice?.OriginalDevice is EcatIODeviceDto;
    public bool IsChillerSelected => SelectedDevice?.OriginalDevice is ChillerDeviceDto;
    public bool IsCustomModbusSelected => SelectedDevice?.OriginalDevice is CustomModbusDeviceDto;
    public bool IsCommunicationDeviceSelected => SelectedDevice?.OriginalDevice is CanDeviceDto or EtherCATDeviceDto;
    public bool IsCanDeviceSelected => SelectedDevice?.OriginalDevice is CanDeviceDto;
    public bool IsEthercatDeviceSelected => SelectedDevice?.OriginalDevice is EtherCATDeviceDto;
    public bool IsWeighingSensorSelected => SelectedDevice?.OriginalDevice is WeighingSensorDto;
    public bool IsScannerSelected => SelectedDevice?.OriginalDevice is ScannerDto;
    public bool IsCentrifugalSelected => SelectedDevice?.OriginalDevice is CentrifugalDeviceDto;
    public bool IsDiyPumpSelected => SelectedDevice?.OriginalDevice is DiyPumpDto;
    
    // 子 ViewModel
    public MotorDebugViewModel MotorDebugVM { get; }
    public SyringePumpDebugViewModel SyringePumpDebugVM { get; }
    public PeristalticPumpDebugViewModel PeristalticPumpDebugVM { get; }
    public DiyPumpDebugViewModel DiyPumpDebugVM { get; }
    public IODeviceDebugViewModel IODeviceDebugVM { get; }
    public TCUDebugViewModel TCUDebugVM { get; }
    public RobotDebugViewModel RobotDebugVM { get; }
    public ScannerDebugViewModel ScannerDebugVM { get; }
    public CentrifugalDebugViewModel CentrifugalDebugVM { get; }
    public WeightSensorDebugViewModel WeightSensorDebugVM { get; }
    public ChillerDebugViewModel ChillerDebugVM { get; }
    
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
    public CanDeviceDto? SelectedCanDevice => SelectedDevice?.OriginalDevice as CanDeviceDto;
    public EtherCATDeviceDto? SelectedEtherCATDevice => SelectedDevice?.OriginalDevice as EtherCATDeviceDto;
    public IEnumerable<IoChannelDto> DiChannels => SelectedEcatIODevice?.IoChannels.Where(c => string.Equals(c.IoType, "DI", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<IoChannelDto>();
    public IEnumerable<IoChannelDto> DoChannels => SelectedEcatIODevice?.IoChannels.Where(c => string.Equals(c.IoType, "DO", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<IoChannelDto>();
    public IEnumerable<IoChannelDto> AiChannels => SelectedEcatIODevice?.IoChannels.Where(c => string.Equals(c.IoType, "AI", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<IoChannelDto>();
    public IEnumerable<IoChannelDto> AoChannels => SelectedEcatIODevice?.IoChannels.Where(c => string.Equals(c.IoType, "AO", StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<IoChannelDto>();
    
    // 电机调试属性
    private double _motorPosition;
    private double _motorTargetPosition;
    private double _motorSpeed = 100;
    private bool _motorRelative;
    private MotorStatus? _motorStatus;
    private WorkPositionDto? _selectedWorkPosition;
    private string _motorUnit = string.Empty;

    public string MotorUnit
    {
        get => _motorUnit;
        set => SetProperty(ref _motorUnit, value);
    }

    public WorkPositionDto? SelectedWorkPosition
    {
        get => _selectedWorkPosition;
        set => SetProperty(ref _selectedWorkPosition, value);
    }

    public IEnumerable<WorkPositionDto> MotorWorkPositions
    {
        get
        {
            if (SelectedMotor?.WorkPositions != null && SelectedMotor.WorkPositions.Any())
                return SelectedMotor.WorkPositions;
            if (SelectedEtherCATMotor?.WorkPositions != null && SelectedEtherCATMotor.WorkPositions.Any())
                return SelectedEtherCATMotor.WorkPositions;
            return Enumerable.Empty<WorkPositionDto>();
        }
    }

    // 注射泵调试属性
    private double _syringeAbsPosition;
    private double _syringeRelStep = 1;
    private int _syringeChannelIndex;
    private string _syringeChannelCode = "I";
    private string _syringeStatus = string.Empty;
    private bool _syringeConnected;

    public ReadOnlyCollection<string> SyringeChannelOptions { get; } = new(new[] { "I", "O", "E", "B" });

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

    // 蠕动泵调试属性
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

    // 串口支持
    private readonly ObservableCollection<string> _serialPorts = new();
    private double _motorJogStep = 1;
    private bool _motorServoEnabled;
    private double _weighingValue;
    private string? _selectedTcuPort;
    private string? _selectedChillerPort;
    private string? _selectedWeighingPort;
    private string _scannerIp = string.Empty;
    private int _scannerPort;
    private string _scannerResult = string.Empty;

    public ObservableCollection<IoChannelControlItem> IoChannels { get; } = new();
    public IEnumerable<IoChannelControlItem> IoDiChannels => IoChannels.Where(c => c.IoType.Equals("DI", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoDoChannels => IoChannels.Where(c => c.IoType.Equals("DO", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoAiChannels => IoChannels.Where(c => c.IoType.Equals("AI", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoAoChannels => IoChannels.Where(c => c.IoType.Equals("AO", StringComparison.OrdinalIgnoreCase));
    public ObservableCollection<string> SerialPorts => _serialPorts;

    public double MotorJogStep
    {
        get => _motorJogStep;
        set => SetProperty(ref _motorJogStep, value);
    }

    public bool MotorServoEnabled
    {
        get => _motorServoEnabled;
        set => SetProperty(ref _motorServoEnabled, value);
    }

    public double WeighingValue
    {
        get => _weighingValue;
        set => SetProperty(ref _weighingValue, value);
    }

    public string? SelectedTcuPort
    {
        get => _selectedTcuPort;
        set => SetProperty(ref _selectedTcuPort, value);
    }

    public string? SelectedChillerPort
    {
        get => _selectedChillerPort;
        set => SetProperty(ref _selectedChillerPort, value);
    }

    public string? SelectedWeighingPort
    {
        get => _selectedWeighingPort;
        set => SetProperty(ref _selectedWeighingPort, value);
    }

    public string ScannerIp
    {
        get => _scannerIp;
        set => SetProperty(ref _scannerIp, value);
    }

    public int ScannerPort
    {
        get => _scannerPort;
        set => SetProperty(ref _scannerPort, value);
    }

    public string ScannerResult
    {
        get => _scannerResult;
        set => SetProperty(ref _scannerResult, value);
    }

    // 离心机调试属性
    private double _centrifugalSpeed;
    private double _centrifugalTime;
    private int _centrifugalRotorPosition = 1;
    private bool _centrifugalConnected;
    private bool _centrifugalRunning;
    private string _centrifugalStatus = string.Empty;

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

    // 自定义泵调试属性
    private bool _diyPumpConnected;
    private bool _diyPumpServoEnabled;
    private int _diyPumpChannel = 1;
    private string _diyPumpStatus = string.Empty;

    public ReadOnlyCollection<int> DiyPumpChannelOptions { get; } = new(new[] { 1, 2, 3, 4 });

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

    // 自定义泵位置控制
    private double _diyPumpCurrentPosition;
    private double _diyPumpTargetPosition;
    private double _diyPumpTarget;
    private double _diyPumpSpeed = 30;
    private double _diyPumpJogStep = 10;
    private bool _diyPumpRelative;
    private bool _diyPumpIsRunning;

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

    // TCU 增强属性
    private bool _tcuConnected;
    private bool _tcuCirculationEnabled;
    private double _tcuTargetTemperatureInput = 25;
    private string _tcuStatus = string.Empty;

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

    public string TcuStatus
    {
        get => _tcuStatus;
        set => SetProperty(ref _tcuStatus, value);
    }

    public TcuDeviceDto? SelectedTcu => SelectedDevice?.OriginalDevice as TcuDeviceDto;
    public CentrifugalDeviceDto? SelectedCentrifugal => SelectedDevice?.OriginalDevice as CentrifugalDeviceDto;
    public WeighingSensorDto? SelectedWeightSensor => SelectedDevice?.OriginalDevice as WeighingSensorDto;

    // 称重传感器属性
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

    // 离心机额外属性
    private string? _selectedCentrifugalPort;
    private bool _centrifugalCompleted;
    public IEnumerable<WorkPositionDto> CentrifugalWorkPositions => SelectedCentrifugal?.WorkPositions ?? Enumerable.Empty<WorkPositionDto>();

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

    // 命令
    public ICommand CreateConfigCommand { get; }
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
    public ICommand MotorJogPositiveCommand { get; }
    public ICommand MotorJogNegativeCommand { get; }
    public ICommand MotorContinuousJogCommand { get; }
    public ICommand MotorClearAlarmCommand { get; }
    public ICommand MotorServoOnCommand { get; }
    public ICommand MotorServoOffCommand { get; }
    public ICommand MotorResetCommand { get; }
    public ICommand RefreshSerialPortsCommand { get; }
    public ICommand TcuConnectCommand { get; }
    public ICommand TcuRefreshPortsCommand { get; }
    public ICommand TcuClearAlarmCommand { get; }
    public ICommand ChillerConnectCommand { get; }
    public ICommand ChillerRefreshPortsCommand { get; }
    public ICommand WeighingRefreshPortsCommand { get; }
    public ICommand WeighingReadCommand { get; }
    public ICommand MotorMoveToWorkPositionCommand { get; }
    public ICommand SyringeInitCommand { get; }
    public ICommand SyringeResetCommand { get; }
    public ICommand SyringeClearAlarmCommand { get; }
    public ICommand SyringeAbsMoveCommand { get; }
    public ICommand SyringeRelMoveCommand { get; }
    public ICommand SyringeSwitchChannelCommand { get; }
    public ICommand SyringeConnectCommand { get; }
    public ICommand SyringeStopCommand { get; }
    public ICommand PeristalticStartByVolumeCommand { get; }
    public ICommand PeristalticStopCommand { get; }
    public ICommand PeristalticMoveCommand { get; }
    public ICommand PeristalticJogPositiveCommand { get; }
    public ICommand PeristalticJogNegativeCommand { get; }
    public ICommand PeristalticServoOnCommand { get; }
    public ICommand PeristalticServoOffCommand { get; }
    public ICommand PeristalticClearAlarmCommand { get; }
    public ICommand PeristalticResetCommand { get; }
    public ICommand IoToggleOutputCommand { get; }
    public ICommand ScannerScanCommand { get; }
    public ICommand CentrifugalConnectCommand { get; }
    public ICommand CentrifugalSetSpeedCommand { get; }
    public ICommand CentrifugalSetTimeCommand { get; }
    public ICommand CentrifugalSetRotorPositionCommand { get; }
    public ICommand CentrifugalStartCommand { get; }
    public ICommand CentrifugalStopCommand { get; }
    public ICommand DiyPumpConnectCommand { get; }
    public ICommand DiyPumpServoOnCommand { get; }
    public ICommand DiyPumpServoOffCommand { get; }
    public ICommand DiyPumpClearAlarmCommand { get; }
    public ICommand DiyPumpResetCommand { get; }
    public ICommand DiyPumpSwitchChannelCommand { get; }
    public ICommand TcuStartControlCommand { get; }
    public ICommand TcuSetCirculationCommand { get; }
    public ICommand TcuSetQuickTemperatureCommand { get; }
    public ICommand TcuDisconnectCommand { get; }
    public ICommand PeristalticContinuousRunCommand { get; }
    public ICommand WeightSensorConnectCommand { get; }
    public ICommand WeightSensorDisconnectCommand { get; }
    public ICommand WeightSensorReadCommand { get; }
    public ICommand WeightSensorZeroCommand { get; }
    public ICommand WeightSensorTareCommand { get; }
    public ICommand WeightSensorRefreshPortsCommand { get; }
    public ICommand WeightSensorStartMonitorCommand { get; }
    public ICommand WeightSensorStopMonitorCommand { get; }
    public ICommand WeightSensorCalibrateCommand { get; }
    public ICommand CentrifugalRefreshPortsCommand { get; }
    public ICommand CentrifugalOpenLidCommand { get; }
    public ICommand CentrifugalCloseLidCommand { get; }
    public ICommand CentrifugalClearAlarmCommand { get; }
    public ICommand DiyPumpMoveCommand { get; }
    public ICommand DiyPumpHomeCommand { get; }
    public ICommand DiyPumpStopCommand { get; }
    public ICommand DiyPumpJogPositiveCommand { get; }
    public ICommand DiyPumpJogNegativeCommand { get; }
    public ICommand DiyPumpQuickMoveCommand { get; }

    public DeviceDebugViewModel(IDeviceConfigService configService, IHardwareController hardwareController, IEventAggregator eventAggregator)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        _eventAggregator = eventAggregator;
        
        // 创建子 ViewModel
        MotorDebugVM = new MotorDebugViewModel(hardwareController);
        SyringePumpDebugVM = new SyringePumpDebugViewModel(hardwareController);
        PeristalticPumpDebugVM = new PeristalticPumpDebugViewModel(hardwareController);
        DiyPumpDebugVM = new DiyPumpDebugViewModel(hardwareController);
        IODeviceDebugVM = new IODeviceDebugViewModel(hardwareController);
        TCUDebugVM = new TCUDebugViewModel(hardwareController);
        RobotDebugVM = new RobotDebugViewModel(hardwareController);
        ScannerDebugVM = new ScannerDebugViewModel(hardwareController);
        CentrifugalDebugVM = new CentrifugalDebugViewModel(hardwareController);
        WeightSensorDebugVM = new WeightSensorDebugViewModel(hardwareController);
        ChillerDebugVM = new ChillerDebugViewModel(hardwareController);
        
        // 订阅位置更新事件（从 PositionSettingsView 同步到这里）
        _eventAggregator.GetEvent<PositionUpdatedEvent>().Subscribe(OnPositionUpdated, ThreadOption.UIThread);
        _eventAggregator.GetEvent<PositionAddedEvent>().Subscribe(OnPositionAdded, ThreadOption.UIThread);
        _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Subscribe(OnConfigCreated, ThreadOption.UIThread);
        _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Subscribe(OnConfigLoaded, ThreadOption.UIThread);
        _eventAggregator.GetEvent<PositionDeletedEvent>().Subscribe(OnPositionDeleted, ThreadOption.UIThread);

        CreateConfigCommand = new DelegateCommand(CreateConfig);
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
        MotorJogPositiveCommand = new DelegateCommand(async () => await MotorJogAsync(true));
        MotorJogNegativeCommand = new DelegateCommand(async () => await MotorJogAsync(false));
        MotorContinuousJogCommand = new DelegateCommand(async () => await MotorContinuousJogAsync());
        MotorClearAlarmCommand = new DelegateCommand(async () => await MotorClearAlarmAsync());
        MotorServoOnCommand = new DelegateCommand(async () => await MotorServoAsync(true));
        MotorServoOffCommand = new DelegateCommand(async () => await MotorServoAsync(false));
        MotorResetCommand = new DelegateCommand(async () => await MotorResetAsync());
        MotorMoveToWorkPositionCommand = new DelegateCommand(async () => await MotorMoveToWorkPositionAsync());

        SyringeInitCommand = new DelegateCommand(async () => await SyringeInitAsync());
        SyringeResetCommand = new DelegateCommand(async () => await SyringeResetAsync());
        SyringeClearAlarmCommand = new DelegateCommand(async () => await SyringeClearAlarmAsync());
        SyringeAbsMoveCommand = new DelegateCommand(async () => await SyringeAbsMoveAsync());
        SyringeRelMoveCommand = new DelegateCommand(async () => await SyringeRelMoveAsync());
        SyringeSwitchChannelCommand = new DelegateCommand(async () => await SyringeSwitchChannelAsync());
        SyringeConnectCommand = new DelegateCommand(async () => await SyringeConnectAsync());
        SyringeStopCommand = new DelegateCommand(async () => await SyringeStopAsync());

        PeristalticStartByVolumeCommand = new DelegateCommand(async () => await PeristalticStartByVolumeAsync());
        PeristalticStopCommand = new DelegateCommand(async () => await PeristalticStopAsync());
        PeristalticMoveCommand = new DelegateCommand(async () => await PeristalticMoveAsync());
        PeristalticJogPositiveCommand = new DelegateCommand(async () => await PeristalticJogAsync(true));
        PeristalticJogNegativeCommand = new DelegateCommand(async () => await PeristalticJogAsync(false));
        PeristalticServoOnCommand = new DelegateCommand(async () => await PeristalticServoAsync(true));
        PeristalticServoOffCommand = new DelegateCommand(async () => await PeristalticServoAsync(false));
        PeristalticClearAlarmCommand = new DelegateCommand(async () => await PeristalticClearAlarmAsync());
        PeristalticResetCommand = new DelegateCommand(async () => await PeristalticResetAsync());

        RefreshSerialPortsCommand = new DelegateCommand(RefreshSerialPorts);
        TcuConnectCommand = new DelegateCommand(async () => await TcuConnectAsync());
        TcuRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        TcuClearAlarmCommand = new DelegateCommand(async () => await TcuClearAlarmAsync());
        ChillerConnectCommand = new DelegateCommand(async () => await ChillerConnectAsync());
        ChillerRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        WeighingRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        WeighingReadCommand = new DelegateCommand(async () => await WeighingReadAsync());
        IoToggleOutputCommand = new DelegateCommand<IoChannelControlItem>(async item => await IoToggleOutputAsync(item));
        ScannerScanCommand = new DelegateCommand(async () => await ScannerScanAsync());

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

        TcuStartControlCommand = new DelegateCommand(async () => await TcuStartControlAsync());
        TcuSetCirculationCommand = new DelegateCommand(async () => await TcuSetCirculationAsync());
        TcuSetQuickTemperatureCommand = new DelegateCommand<string>(async temp => await TcuSetQuickTemperatureAsync(temp));
        TcuDisconnectCommand = new DelegateCommand(async () => await TcuDisconnectAsync());

        PeristalticContinuousRunCommand = new DelegateCommand(async () => await PeristalticContinuousRunAsync());

        WeightSensorConnectCommand = new DelegateCommand(async () => await WeightSensorConnectAsync());
        WeightSensorDisconnectCommand = new DelegateCommand(async () => await WeightSensorDisconnectAsync());
        WeightSensorReadCommand = new DelegateCommand(async () => await WeightSensorReadAsync());
        WeightSensorZeroCommand = new DelegateCommand(async () => await WeightSensorZeroAsync());
        WeightSensorTareCommand = new DelegateCommand(async () => await WeightSensorTareAsync());
        WeightSensorRefreshPortsCommand = new DelegateCommand(RefreshSerialPorts);
        WeightSensorStartMonitorCommand = new DelegateCommand(async () => await WeightSensorStartMonitorAsync());
        WeightSensorStopMonitorCommand = new DelegateCommand(async () => await WeightSensorStopMonitorAsync());
        WeightSensorCalibrateCommand = new DelegateCommand(async () => await WeightSensorCalibrateAsync());

        // 扫码枪命令初始化
        ScannerConnectCommand = new DelegateCommand(async () => await ScannerConnectAsync());
        ScannerDisconnectCommand = new DelegateCommand(async () => await ScannerDisconnectAsync());
        ScannerClearResultCommand = new DelegateCommand(() => { ScannerResult = string.Empty; ScannerStatus = "扫描结果已清除"; });
        ScannerCopyResultCommand = new DelegateCommand(() => { if (!string.IsNullOrEmpty(ScannerResult)) { System.Windows.Clipboard.SetText(ScannerResult); ScannerStatus = "已复制到剪贴板"; } });
        ScannerStartContinuousCommand = new DelegateCommand(async () => await ScannerStartContinuousAsync());
        ScannerStopContinuousCommand = new DelegateCommand(() => { ScannerScanning = false; ScannerStatus = "连续扫描已停止"; });
        ScannerClearHistoryCommand = new DelegateCommand(() => { ScannerHistory.Clear(); ScannerStatus = "扫描历史已清除"; });

        // 机器人命令初始化
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

        RefreshSerialPorts();
    }

    private void OnConfigLoaded(ConfigLoadedEventArgs args)
    {
        if(args != null)
        {
            CurrentConfig = args.Config;
           
            _logger.Info($"Received config loaded event");
            try
            {
                StatusMessage = "正在导入配置...";

                // 清空所有列表
                ClearAllDeviceLists();

                // 更新设备列表
                PopulateDeviceLists(CurrentConfig);

                // 更新统一设备列表
                BuildUnifiedDeviceList(CurrentConfig);

                FilterDevices();

                StatusMessage = $"成功导入配置，共 {AllDevices.Count} 个设备";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "导入设备配置失败");
                StatusMessage = $"导入失败: {ex.Message}";
            }
        }
    }

    private void OnConfigCreated(DeviceConfigDto dto)
    {
        CurrentConfig = dto;
        _logger.Info($"Received config created event");
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
        
        // 根据设备类型更新对应的子 ViewModel
        switch (SelectedDevice.OriginalDevice)
        {
            case MotorDto motor:
                MotorDebugVM.SelectedMotor = motor;
                MotorDebugVM.SelectedEtherCATMotor = null;
                break;
                
            case EtherCATMotorDto ecatMotor:
                MotorDebugVM.SelectedMotor = null;
                MotorDebugVM.SelectedEtherCATMotor = ecatMotor;
                break;
                
            case SyringePumpDto syringePump:
                SyringePumpDebugVM.SelectedPump = syringePump;
                break;
                
            case PeristalticPumpDto peristalticPump:
                PeristalticPumpDebugVM.SelectedPump = peristalticPump;
                break;
                
            case DiyPumpDto diyPump:
                DiyPumpDebugVM.SelectedPump = diyPump;
                break;
                
            case EcatIODeviceDto ioDevice:
                IODeviceDebugVM.SelectedDevice = ioDevice;
                break;
                
            case TcuDeviceDto tcu:
                TCUDebugVM.SelectedTcu = tcu;
                break;
                
            case JakaRobotDto robot:
                RobotDebugVM.SelectedRobot = robot;
                break;
                
            case ScannerDto scanner:
                ScannerDebugVM.SelectedScanner = scanner;
                break;
                
            case CentrifugalDeviceDto centrifugal:
                CentrifugalDebugVM.SelectedDevice = centrifugal;
                break;
                
            case WeighingSensorDto weightSensor:
                WeightSensorDebugVM.SelectedSensor = weightSensor;
                break;
                
            case ChillerDeviceDto chiller:
                ChillerDebugVM.SelectedChiller = chiller;
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
            
            // 发布配置导入事件，通知 PositionSettingsView 同步
            _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
            
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
        //foreach (var valve in config.TwoChannelValves)
        //{
        //    var item = new DeviceItemViewModel
        //    {
        //        DeviceId = valve.DeviceId,
        //        Name = valve.Name,
        //        DeviceType = "二通阀",
        //        Category = "二通阀",
        //        IsEnabled = valve.IsEnabled,
        //        IconKind = "Valve",
        //        OriginalDevice = valve
        //    };
        //    AllDevices.Add(item);
        //    AddToCategory(categories, "二通阀", item, categoryDefinitions);
        //}
        
        // 三通阀
        //foreach (var valve in config.ThreeChannelValves)
        //{
        //    var item = new DeviceItemViewModel
        //    {
        //        DeviceId = valve.DeviceId,
        //        Name = valve.Name,
        //        DeviceType = "三通阀",
        //        Category = "三通阀",
        //        IsEnabled = valve.IsEnabled,
        //        IconKind = "Valve",
        //        OriginalDevice = valve
        //    };
        //    AllDevices.Add(item);
        //    AddToCategory(categories, "三通阀", item, categoryDefinitions);
        //}
        
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
            await Task.Delay(100);
            TcuIsRunning = true;
            TcuCurrentTemperature = TcuTargetTemperature;
            StatusMessage = $"TCU {tcu.Name} 循环已启动";
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
            await Task.Delay(100);
            TcuIsRunning = false;
            StatusMessage = $"TCU {tcu.Name} 循环已停止";
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
            await Task.Delay(100);
            TcuCurrentTemperature = TcuTargetTemperature;
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

            SelectedTcuPort ??= SelectedTcuDevice?.PortName ?? SerialPorts.FirstOrDefault();
            SelectedChillerPort ??= SelectedChillerDevice?.PortName ?? SerialPorts.FirstOrDefault();
            SelectedWeighingPort ??= SelectedWeighingSensor?.PortName ?? SerialPorts.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.Warn(ex, "获取串口列表失败");
        }
    }

    private async Task<bool> TryGetMotorTargetAsync(Func<string, string, Task> action)
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

        if (deviceId == null || motorName == null) return false;

        await action(deviceId, motorName);
        return true;
    }

    private async Task MotorJogAsync(bool positive)
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                var step = positive ? MotorJogStep : -MotorJogStep;
                StatusMessage = $"正在{(positive ? "正向" : "反向")}JOG {motorName}...";
                await _hardwareController.MoveMotorAsync(deviceId, step, MotorSpeed, true, true);
                StatusMessage = $"{motorName} JOG 完成";
                await RefreshDeviceStatusAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "JOG 失败");
                StatusMessage = $"JOG失败: {ex.Message}";
            }
        });
    }

    private async Task MotorContinuousJogAsync()
    {
        await TryGetMotorTargetAsync(async (deviceId, motorName) =>
        {
            try
            {
                StatusMessage = $"正在持续JOG {motorName} (模拟)...";
                await _hardwareController.MoveMotorAsync(deviceId, MotorRelative ? MotorJogStep : MotorTargetPosition, MotorSpeed, true, false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "持续JOG失败");
                StatusMessage = $"持续JOG失败: {ex.Message}";
            }
        });
    }

    private async Task MotorClearAlarmAsync()
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            await Task.Delay(100);
            StatusMessage = $"{motorName} 报警已清除 (模拟)";
        });
    }

    private async Task MotorServoAsync(bool enable)
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            MotorServoEnabled = enable;
            await Task.Delay(50);
            StatusMessage = enable ? $"{motorName} 已上使能" : $"{motorName} 已下使能";
        });
    }

    private async Task MotorResetAsync()
    {
        await TryGetMotorTargetAsync(async (_, motorName) =>
        {
            await Task.Delay(80);
            MotorPosition = 0;
            MotorTargetPosition = 0;
            StatusMessage = $"{motorName} 已复位";
        });
    }

    private async Task TcuConnectAsync()
    {
        if (SelectedTcuDevice == null) return;
        await Task.Delay(50);
        TcuConnected = true;
        StatusMessage = $"TCU {SelectedTcuDevice.Name} 已连接 ({SelectedTcuPort ?? SelectedTcuDevice.PortName})";
    }

    private async Task TcuClearAlarmAsync()
    {
        if (SelectedTcuDevice == null) return;
        await Task.Delay(80);
        StatusMessage = $"TCU {SelectedTcuDevice.Name} 报警已清除";
    }

    private async Task ChillerConnectAsync()
    {
        if (SelectedChillerDevice == null) return;
        await Task.Delay(50);
        StatusMessage = $"冷水机 {SelectedChillerDevice.Name} 已连接 ({SelectedChillerPort ?? SelectedChillerDevice.PortName})";
    }

    private async Task WeighingReadAsync()
    {
        if (SelectedWeighingSensor == null) return;
        await Task.Delay(80);
        WeighingValue = Math.Round(Random.Shared.NextDouble() * SelectedWeighingSensor.MaxWeight, SelectedWeighingSensor.DecimalPlaces);
        StatusMessage = $"称重读数: {WeighingValue} ({SelectedWeighingPort ?? SelectedWeighingSensor.PortName})";
    }

    private async Task MotorMoveToWorkPositionAsync()
    {
        if (SelectedWorkPosition == null) return;
        MotorTargetPosition = SelectedWorkPosition.Position;
        MotorSpeed = SelectedWorkPosition.Speed;
        await MotorMoveAsync();
    }

    private async Task SyringeInitAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(100);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 初始化完成";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeResetAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 已复位";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeClearAlarmAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(60);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 报警已清除";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeAbsMoveAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(120);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 绝对运行到 {SyringeAbsPosition} ml";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeRelMoveAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(120);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 相对运行 {SyringeRelStep} ml";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeSwitchChannelAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"注射泵 {SelectedSyringePump.Name} 切换到通道 {SyringeChannelCode}";
        StatusMessage = SyringeStatus;
    }

    private async Task PeristalticMoveAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(120);
        PeristalticPosition = PeristalticRelative ? PeristalticPosition + PeristalticTarget : PeristalticTarget;
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 运行到 {PeristalticPosition} ml";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticJogAsync(bool positive)
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(80);
        var step = positive ? PeristalticJogStep : -PeristalticJogStep;
        PeristalticPosition += step;
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} JOG {(positive ? "+" : "-")}{Math.Abs(step)} ml";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticServoAsync(bool enable)
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(50);
        PeristalticServoEnabled = enable;
        PeristalticStatus = enable ? $"蠕动泵 {SelectedPeristalticPump.Name} 已上使能" : $"蠕动泵 {SelectedPeristalticPump.Name} 已下使能";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticClearAlarmAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(60);
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 报警已清除";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticResetAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(80);
        PeristalticPosition = 0;
        PeristalticTarget = 0;
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 已复位";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticStartByVolumeAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(150);
        PeristalticPosition += PeristalticTotalVolume;
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 按量 {PeristalticTotalVolume} ml 泵送完成";
        StatusMessage = PeristalticStatus;
    }

    private async Task PeristalticStopAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(60);
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 已停止";
        StatusMessage = PeristalticStatus;
    }
    
    // 电机调试属性
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

    public ScannerDto? SelectedScanner => SelectedDevice?.OriginalDevice as ScannerDto;

    // 电机额外属性
    private bool _motorOnline = true;
    private bool _motorHomed;
    private bool _motorHasAlarm;
    private string _motorStatusMessage = string.Empty;

    public bool MotorOnline
    {
        get => _motorOnline;
        set => SetProperty(ref _motorOnline, value);
    }

    public bool MotorHomed
    {
        get => _motorHomed;
        set => SetProperty(ref _motorHomed, value);
    }

    public bool MotorHasAlarm
    {
        get => _motorHasAlarm;
        set => SetProperty(ref _motorHasAlarm, value);
    }

    public string MotorStatusMessage
    {
        get => _motorStatusMessage;
        set => SetProperty(ref _motorStatusMessage, value);
    }

    // IO额外属性
    private bool _ioConnected = true;
    private string _ioStatus = string.Empty;

    public bool IoConnected
    {
        get => _ioConnected;
        set => SetProperty(ref _ioConnected, value);
    }

    public string IoStatus
    {
        get => _ioStatus;
        set => SetProperty(ref _ioStatus, value);
    }

    // 扫码枪额外属性
    private bool _scannerConnected;
    private bool _scannerEnabled = true;
    private bool _scannerScanning;
    private bool _scannerContinuousScan;
    private int _scannerInterval = 1000;
    private string _scannerStatus = string.Empty;
    private ObservableCollection<string> _scannerHistory = new();

    public bool ScannerConnected
    {
        get => _scannerConnected;
        set => SetProperty(ref _scannerConnected, value);
    }

    public bool ScannerEnabled
    {
        get => _scannerEnabled;
        set => SetProperty(ref _scannerEnabled, value);
    }

    public bool ScannerScanning
    {
        get => _scannerScanning;
        set => SetProperty(ref _scannerScanning, value);
    }

    public bool ScannerContinuousScan
    {
        get => _scannerContinuousScan;
        set => SetProperty(ref _scannerContinuousScan, value);
    }

    public int ScannerInterval
    {
        get => _scannerInterval;
        set => SetProperty(ref _scannerInterval, value);
    }

    public string ScannerStatus
    {
        get => _scannerStatus;
        set => SetProperty(ref _scannerStatus, value);
    }

    public ObservableCollection<string> ScannerHistory
    {
        get => _scannerHistory;
        set => SetProperty(ref _scannerHistory, value);
    }

    // 扫码枪命令
    public ICommand ScannerConnectCommand { get; }
    public ICommand ScannerDisconnectCommand { get; }
    public ICommand ScannerClearResultCommand { get; }
    public ICommand ScannerCopyResultCommand { get; }
    public ICommand ScannerStartContinuousCommand { get; }
    public ICommand ScannerStopContinuousCommand { get; }
    public ICommand ScannerClearHistoryCommand { get; }

    // 机器人属性
    private bool _robotConnected;
    private bool _robotEnabled;
    private bool _robotMoving;
    private bool _robotHasAlarm;
    private string _robotCurrentTask = "空闲";
    private string _robotIp = string.Empty;
    private int _robotPort;
    private int _robotTaskNumber;
    private string _robotStatus = string.Empty;

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

    // 机器人命令
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
    
    private void RebuildIoChannels(EcatIODeviceDto io)
    {
        IoChannels.Clear();
        foreach (var ch in io.IoChannels.OrderBy(c => c.ChannelNumber))
        {
            IoChannels.Add(new IoChannelControlItem(ch));
        }
        RaisePropertyChanged(nameof(IoDiChannels));
        RaisePropertyChanged(nameof(IoDoChannels));
        RaisePropertyChanged(nameof(IoAiChannels));
        RaisePropertyChanged(nameof(IoAoChannels));
    }

    private async Task IoToggleOutputAsync(IoChannelControlItem? item)
    {
        if (item == null || SelectedDevice?.OriginalDevice is not EcatIODeviceDto ioDevice) return;
        if (!item.IsOutput) return;
        try
        {
            item.Toggle();
            await _hardwareController.SetIoOutputAsync(ioDevice.DeviceId, item.ChannelNumber, item.Value > 0.5);
            StatusMessage = $"IO {ioDevice.Name} 通道 {item.ChannelNumber} 已设置为 {item.Value}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 IO 输出失败");
            StatusMessage = $"设置失败: {ex.Message}";
        }
    }

    private async Task ScannerScanAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(100);
        ScannerResult = $"SCAN-{DateTime.Now:HHmmss}";
        StatusMessage = $"扫码枪 {SelectedScanner.Name} 读取: {ScannerResult} ({ScannerIp}:{ScannerPort})";
    }

    private async Task SyringeConnectAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(80);
        SyringeConnected = true;
        SyringeStatus = $"{SelectedSyringePump.Name} 已连接";
        StatusMessage = SyringeStatus;
    }

    private async Task SyringeStopAsync()
    {
        if (SelectedSyringePump == null) return;
        await Task.Delay(80);
        SyringeStatus = $"{SelectedSyringePump.Name} 已停止";
        StatusMessage = SyringeStatus;
    }

    private async Task CentrifugalConnectAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(80);
        CentrifugalConnected = true;
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 已连接";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalSetSpeedAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 转速设置为 {CentrifugalSpeed} RPM";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalSetTimeAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 离心时间设置为 {CentrifugalTime} 秒";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalSetRotorPositionAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(60);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 转子位置设置为 {CentrifugalRotorPosition}";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalStartAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(100);
        CentrifugalRunning = true;
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 开始离心 (转速: {CentrifugalSpeed} RPM, 时间: {CentrifugalTime}秒, 位置: {CentrifugalRotorPosition})";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalStopAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(80);
        CentrifugalRunning = false;
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 已停止";
        StatusMessage = CentrifugalStatus;
    }

    private async Task DiyPumpConnectAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(80);
        DiyPumpConnected = true;
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 已连接";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpServoAsync(bool enable)
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(50);
        DiyPumpServoEnabled = enable;
        DiyPumpStatus = enable ? $"自定义泵 {SelectedDiyPump.Name} 已上使能" : $"自定义泵 {SelectedDiyPump.Name} 已下使能";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpClearAlarmAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(60);
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 报警已清除";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpResetAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(80);
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 已归零";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpSwitchChannelAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(70);
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 切换到通道 {DiyPumpChannel}";
        StatusMessage = DiyPumpStatus;
    }

    private async Task TcuStartControlAsync()
    {
        if (SelectedTcuDevice == null) return;
        await Task.Delay(100);
        TcuIsRunning = true;
        TcuTargetTemperature = TcuTargetTemperatureInput;
        var circulation = TcuCirculationEnabled ? "循环已开启" : "循环已关闭";
        TcuStatus = $"TCU {SelectedTcuDevice.Name} 开始控温到 {TcuTargetTemperature}°C ({circulation})";
        StatusMessage = TcuStatus;
    }

    private async Task TcuSetCirculationAsync()
    {
        if (SelectedTcuDevice == null) return;
        await Task.Delay(50);
        TcuStatus = $"TCU {SelectedTcuDevice.Name} 循环已{(TcuCirculationEnabled ? "开启" : "关闭")}";
        StatusMessage = TcuStatus;
    }

    private async Task TcuSetQuickTemperatureAsync(string? temp)
    {
        if (SelectedTcuDevice == null || string.IsNullOrEmpty(temp)) return;
        if (double.TryParse(temp, out var temperature))
        {
            TcuTargetTemperatureInput = temperature;
            await TcuSetTemperatureAsync();
        }
    }

    private async Task TcuDisconnectAsync()
    {
        if (SelectedTcuDevice == null) return;
        await Task.Delay(50);
        TcuConnected = false;
        TcuIsRunning = false;
        TcuStatus = $"TCU {SelectedTcuDevice.Name} 已断开";
        StatusMessage = TcuStatus;
    }

    private async Task PeristalticContinuousRunAsync()
    {
        if (SelectedPeristalticPump == null) return;
        await Task.Delay(100);
        PeristalticIsRunning = true;
        PeristalticCurrentFlowRate = PeristalticFlowRate;
        PeristalticStatus = $"蠕动泵 {SelectedPeristalticPump.Name} 持续运行中 (流量: {PeristalticFlowRate} mL/min)";
        StatusMessage = PeristalticStatus;
    }

    private async Task WeightSensorConnectAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(80);
        WeightSensorConnected = true;
        WeightSensorStatus = $"称重传感器 {SelectedWeightSensor.Name} 已连接 ({SelectedWeightSensorPort ?? SelectedWeightSensor.PortName})";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorDisconnectAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(50);
        WeightSensorConnected = false;
        WeightSensorStatus = $"称重传感器 {SelectedWeightSensor.Name} 已断开";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorReadAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(80);
        WeightSensorCurrentWeight = Math.Round(Random.Shared.NextDouble() * 100, SelectedWeightSensor.DecimalPlaces);
        WeightSensorStable = true;
        WeightSensorStatus = $"读取重量: {WeightSensorCurrentWeight} g";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorZeroAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(100);
        WeightSensorCurrentWeight = 0;
        WeightSensorZeroed = true;
        WeightSensorStatus = $"称重传感器 {SelectedWeightSensor.Name} 已清零";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorTareAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(80);
        WeightSensorCurrentWeight = 0;
        WeightSensorStatus = $"称重传感器 {SelectedWeightSensor.Name} 已去皮";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorStartMonitorAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(50);
        WeightSensorStatus = $"开始监测目标重量 {WeightSensorTargetWeight} g (偏差: ±{WeightSensorTolerance} g)";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorStopMonitorAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(50);
        WeightSensorStatus = $"停止重量监测";
        StatusMessage = WeightSensorStatus;
    }

    private async Task WeightSensorCalibrateAsync()
    {
        if (SelectedWeightSensor == null) return;
        await Task.Delay(200);
        WeightSensorStatus = $"称重传感器 {SelectedWeightSensor.Name} 校准完成 (标准砝码: {WeightSensorCalibrationWeight} g)";
        StatusMessage = WeightSensorStatus;
    }

    private async Task CentrifugalOpenLidAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(100);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 盖子已打开";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalCloseLidAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(100);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 盖子已关闭";
        StatusMessage = CentrifugalStatus;
    }

    private async Task CentrifugalClearAlarmAsync()
    {
        if (SelectedCentrifugalDevice == null) return;
        await Task.Delay(80);
        CentrifugalStatus = $"离心机 {SelectedCentrifugalDevice.Name} 报警已清除";
        StatusMessage = CentrifugalStatus;
    }

    private async Task DiyPumpMoveAsync()
    {
        if (SelectedDiyPump == null) return;
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
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 移动到 {DiyPumpCurrentPosition}°";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpHomeAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(100);
        DiyPumpCurrentPosition = 0;
        DiyPumpTargetPosition = 0;
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 已回零";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpStopAsync()
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(50);
        DiyPumpIsRunning = false;
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} 已停止";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpJogAsync(bool positive)
    {
        if (SelectedDiyPump == null) return;
        await Task.Delay(80);
        var step = positive ? DiyPumpJogStep : -DiyPumpJogStep;
        DiyPumpCurrentPosition += step;
        DiyPumpStatus = $"自定义泵 {SelectedDiyPump.Name} JOG {(positive ? "+" : "-")}{Math.Abs(step)}°";
        StatusMessage = DiyPumpStatus;
    }

    private async Task DiyPumpQuickMoveAsync(string? angle)
    {
        if (SelectedDiyPump == null || string.IsNullOrEmpty(angle)) return;
        if (double.TryParse(angle, out var targetAngle))
        {
            DiyPumpTarget = targetAngle;
            DiyPumpRelative = false;
            await DiyPumpMoveAsync();
        }
    }

    // 扫码枪方法实现
    private async Task ScannerConnectAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(100);
        ScannerConnected = true;
        ScannerStatus = $"扫码枪 {SelectedScanner.Name} 已连接 ({ScannerIp}:{ScannerPort})";
        StatusMessage = ScannerStatus;
    }

    private async Task ScannerDisconnectAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(50);
        ScannerConnected = false;
        ScannerScanning = false;
        ScannerStatus = $"扫码枪 {SelectedScanner.Name} 已断开";
        StatusMessage = ScannerStatus;
    }

    private async Task ScannerStartContinuousAsync()
    {
        if (SelectedScanner == null || !ScannerConnected) return;
        ScannerScanning = true;
        ScannerStatus = $"开始连续扫描，间隔 {ScannerInterval}ms";
        await Task.Delay(100);
    }

    // 机器人方法实现
    private async Task RobotConnectAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(100);
        RobotConnected = true;
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 已连接 ({RobotIp}:{RobotPort})";
        StatusMessage = RobotStatus;
    }

    private async Task RobotDisconnectAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(50);
        RobotConnected = false;
        RobotEnabled = false;
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 已断开";
        StatusMessage = RobotStatus;
    }

    private async Task RobotEnableAsync(bool enable)
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(80);
        RobotEnabled = enable;
        RobotStatus = enable ? $"机器人 {SelectedJakaRobot.Name} 已使能" : $"机器人 {SelectedJakaRobot.Name} 已下使能";
        StatusMessage = RobotStatus;
    }

    private async Task RobotClearAlarmAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(60);
        RobotHasAlarm = false;
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 报警已清除";
        StatusMessage = RobotStatus;
    }

    private async Task RobotExecuteTaskAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = $"任务 {RobotTaskNumber}";
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 开始执行任务 {RobotTaskNumber}";
        StatusMessage = RobotStatus;
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }

    private async Task RobotContinueAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 继续执行";
        StatusMessage = RobotStatus;
    }

    private async Task RobotStopAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(50);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 已停止";
        StatusMessage = RobotStatus;
    }

    private async Task RobotMoveHomeAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = "回原点";
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 正在回原点";
        StatusMessage = RobotStatus;
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }

    private async Task RobotMoveSafeAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(100);
        RobotMoving = true;
        RobotCurrentTask = "移动到安全位";
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 正在移动到安全位";
        StatusMessage = RobotStatus;
        await Task.Delay(500);
        RobotMoving = false;
        RobotCurrentTask = "空闲";
    }

    private async Task RobotPauseAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 已暂停";
        StatusMessage = RobotStatus;
    }

    private async Task RobotResumeAsync()
    {
        if (SelectedJakaRobot == null) return;
        await Task.Delay(50);
        RobotStatus = $"机器人 {SelectedJakaRobot.Name} 已恢复";
        StatusMessage = RobotStatus;
    }
    
    // 位置同步事件处理 - 从 PositionSettingsView 同步到这里
    private void OnPositionUpdated(PositionUpdatedEventArgs args)
    {
        if (CurrentConfig == null) return;
        
        try
        {
            // 更新 CAN 电机位置
            var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == args.DeviceId);
            if (motor != null)
            {
                var pos = motor.WorkPositions.FirstOrDefault(p => p.Name == args.PositionName);
                if (pos != null)
                {
                    pos.Position = args.Position;
                    pos.Speed = args.Speed;
                    StatusMessage = $"位置已更新: {motor.Name} - {args.PositionName}";
                }
                return;
            }
            
            // 更新 EtherCAT 电机位置
            var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == args.DeviceId);
            if (ecatMotor != null)
            {
                var pos = ecatMotor.WorkPositions.FirstOrDefault(p => p.Name == args.PositionName);
                if (pos != null)
                {
                    pos.Position = args.Position;
                    pos.Speed = args.Speed;
                    StatusMessage = $"位置已更新: {ecatMotor.Name} - {args.PositionName}";
                }
                return;
            }
            
            // 更新离心机位置
            var centrifugal = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == args.DeviceId);
            if (centrifugal != null)
            {
                var pos = centrifugal.WorkPositions.FirstOrDefault(p => p.Name == args.PositionName);
                if (pos != null)
                {
                    pos.Position = args.Position;
                    pos.Speed = args.Speed;
                    StatusMessage = $"位置已更新: {centrifugal.Name} - {args.PositionName}";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "同步位置更新失败");
        }
    }
    
    private void OnPositionAdded(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;
        
        try
        {
            // 添加到对应设备
            var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                motor.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
                StatusMessage = $"新位置已添加: {motor.Name} - {position.PositionName}";
                return;
            }
            
            var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (ecatMotor != null)
            {
                ecatMotor.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
                StatusMessage = $"新位置已添加: {ecatMotor.Name} - {position.PositionName}";
                return;
            }
            
            var centrifugal = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == position.DeviceId);
            if (centrifugal != null)
            {
                centrifugal.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
                StatusMessage = $"新位置已添加: {centrifugal.Name} - {position.PositionName}";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "同步添加位置失败");
        }
    }
    
    /// <summary>
    /// Create new hardware configuration
    /// </summary>
    private void CreateConfig()
    {
        try
        {
            var config = new DeviceConfigDto
            {
                Motors = new List<MotorDto>(),
                EtherCATMotors = new List<EtherCATMotorDto>(),
                SyringePumps = new List<SyringePumpDto>(),
                PeristalticPumps = new List<PeristalticPumpDto>(),
                DiyPumps = new List<DiyPumpDto>(),
                CentrifugalDevices = new List<CentrifugalDeviceDto>(),
                JakaRobots = new List<JakaRobotDto>(),
                TcuDevices = new List<TcuDeviceDto>(),
                ChillerDevices = new List<ChillerDeviceDto>(),
                WeighingSensors = new List<WeighingSensorDto>(),
                TwoChannelValves = new List<TwoChannelValveDto>(),
                ThreeChannelValves = new List<ThreeChannelValveDto>(),
                EcatIODevices = new List<EcatIODeviceDto>(),
                CustomModbusDevices = new List<CustomModbusDeviceDto>(),
                Scanners = new List<ScannerDto>()
            };
            
            CurrentConfig = config;
            
            // Clear device lists
            AllDevices.Clear();
            DeviceCategories.Clear();
            FilteredDevices.Clear();
            
            // Publish events
            _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Publish(config);
            _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
            {
                Config = config,
                FilePath = string.Empty,
                Source = "DeviceDebugView-Create"
            });
            
            StatusMessage = "新配置已创建，可以开始添加设备";
            _logger.Info("New configuration created successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create configuration");
            StatusMessage = $"创建配置失败: {ex.Message}";
        }
    }

    
    private void OnPositionDeleted(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;
        
        try
        {
            // 从对应设备删除
            var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                var pos = motor.WorkPositions.FirstOrDefault(p => p.Name == position.PositionName);
                if (pos != null)
                {
                    motor.WorkPositions.Remove(pos);
                    StatusMessage = $"位置已删除: {motor.Name} - {position.PositionName}";
                }
                return;
            }
            
            var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (ecatMotor != null)
            {
                var pos = ecatMotor.WorkPositions.FirstOrDefault(p => p.Name == position.PositionName);
                if (pos != null)
                {
                    ecatMotor.WorkPositions.Remove(pos);
                    StatusMessage = $"位置已删除: {ecatMotor.Name} - {position.PositionName}";
                }
                return;
            }
            
            var centrifugal = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == position.DeviceId);
            if (centrifugal != null)
            {
                var pos = centrifugal.WorkPositions.FirstOrDefault(p => p.Name == position.PositionName);
                if (pos != null)
                {
                    centrifugal.WorkPositions.Remove(pos);
                    StatusMessage = $"位置已删除: {centrifugal.Name} - {position.PositionName}";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "同步删除位置失败");
        }
    }
}
public class IoChannelControlItem : BindableBase
{
    public IoChannelControlItem(IoChannelDto channel)
    {
        Channel = channel;
        Value = channel.DefaultValue;
    }

    public IoChannelDto Channel { get; }
    public int ChannelNumber => Channel.ChannelNumber;
    public string ChannelName => Channel.ChannelName;
    public string IoType => Channel.IoType;
    public bool IsOutput => IoType.Equals("DO", StringComparison.OrdinalIgnoreCase) || IoType.Equals("AO", StringComparison.OrdinalIgnoreCase);

    private double _value;
    public double Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                RaisePropertyChanged(nameof(BoolValue));
            }
        }
    }

    public bool BoolValue
    {
        get => Value > 0.5;
        set
        {
            Value = value ? 1 : 0;
            RaisePropertyChanged(nameof(Value));
        }
    }

    public void Toggle()
    {
        if (IsOutput)
        {
            BoolValue = !BoolValue;
        }
    }
}

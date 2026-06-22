using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Dialogs;
using IndustrySystem.MotionDesigner.Events;
using IndustrySystem.MotionDesigner.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PositionPointViewModel = IndustrySystem.MotionDesigner.Services.PositionPointViewModel;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// </summary>
public class PositionSettingsViewModel : MotionDesignerBaseViewModel
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IDeviceConfigService _configService;
    private readonly IHardwareController _hardwareController;
    private readonly IEventAggregator _eventAggregator;

    private DeviceConfigDto? _currentConfig;
    private string _statusMessage = "����";
    private string _searchText = string.Empty;
    private PositionPointViewModel? _selectedPosition;
    private string _selectedDeviceFilter = "ȫ��";

    public DeviceConfigDto? CurrentConfig
    {
        get => _currentConfig;
        set => SetProperty(ref _currentConfig, value);
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
                FilterPositions();
            }
        }
    }

    public PositionPointViewModel? SelectedPosition
    {
        get => _selectedPosition;
        set => SetProperty(ref _selectedPosition, value);
    }

    public string SelectedDeviceFilter
    {
        get => _selectedDeviceFilter;
        set
        {
            if (SetProperty(ref _selectedDeviceFilter, value))
            {
                FilterPositions();
            }
        }
    }

    public ObservableCollection<PositionPointViewModel> AllPositions { get; } = new();
    public ObservableCollection<PositionPointViewModel> FilteredPositions { get; } = new();

    public ObservableCollection<string> DeviceFilters { get; } = new() { "ȫ��" };

    private int _totalPositionCount;
    private int _motorPositionCount;
    private int _robotPositionCount;
    private int _modifiedCount;

    public int TotalPositionCount
    {
        get => _totalPositionCount;
        set => SetProperty(ref _totalPositionCount, value);
    }

    public int MotorPositionCount
    {
        get => _motorPositionCount;
        set => SetProperty(ref _motorPositionCount, value);
    }

    public int RobotPositionCount
    {
        get => _robotPositionCount;
        set => SetProperty(ref _robotPositionCount, value);
    }

    public int ModifiedCount
    {
        get => _modifiedCount;
        set => SetProperty(ref _modifiedCount, value);
    }

    public ICommand ImportConfigCommand { get; }
    public ICommand SaveConfigCommand { get; }
    public ICommand ExportConfigCommand { get; }
    public ICommand MoveToPositionCommand { get; }
    public ICommand TeachPositionCommand { get; }
    public ICommand ResetPositionCommand { get; }
    public ICommand AddPositionCommand { get; }
    public ICommand DeletePositionCommand { get; }
    public ICommand AddDeviceCommand { get; }

    public PositionSettingsViewModel(IDeviceConfigService configService, IHardwareController hardwareController, IEventAggregator eventAggregator)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        _eventAggregator = eventAggregator;

        _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Subscribe(OnConfigImported, ThreadOption.UIThread);
        _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Subscribe(OnConfigLoaded, ThreadOption.UIThread);
        _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Subscribe(OnConfigCreated, ThreadOption.UIThread);

        ImportConfigCommand = new DelegateCommand(async () => await ImportConfigAsync());
        SaveConfigCommand = new DelegateCommand(async () => await SaveConfigAsync());
        ExportConfigCommand = new DelegateCommand(async () => await ExportConfigAsync());
        MoveToPositionCommand = new DelegateCommand(async () => await MoveToPositionAsync());
        TeachPositionCommand = new DelegateCommand(async () => await TeachPositionAsync());
        ResetPositionCommand = new DelegateCommand(ResetPosition);
        AddPositionCommand = new DelegateCommand(AddPosition);
        DeletePositionCommand = new DelegateCommand(DeletePosition);
        AddDeviceCommand = new DelegateCommand(AddDevice);
    }

    private void OnConfigCreated(DeviceConfigDto dto)
    {
        _currentConfig = dto;
        _logger.Info($"Received config created event");
        // Initialize designer with new configuration
    }

    private void FilterPositions()
    {
        FilteredPositions.Clear();
        var search = SearchText?.ToLower() ?? string.Empty;

        foreach (var pos in AllPositions)
        {
            var matchesSearch = string.IsNullOrEmpty(search) ||
                pos.DeviceName.ToLower().Contains(search) ||
                pos.PositionName.ToLower().Contains(search) ||
                pos.DeviceId.ToLower().Contains(search);

            var matchesFilter = SelectedDeviceFilter == "ȫ��" ||
                pos.DeviceName == SelectedDeviceFilter ||
                pos.DeviceType == SelectedDeviceFilter;

            if (matchesSearch && matchesFilter)
            {
                FilteredPositions.Add(pos);
            }
        }
    }

    private void UpdateStatistics()
    {
        TotalPositionCount = AllPositions.Count;
        MotorPositionCount = AllPositions.Count(p => p.DeviceType.Contains("���"));
        RobotPositionCount = AllPositions.Count(p => p.DeviceType.Contains("������"));
        ModifiedCount = AllPositions.Count(p => p.IsModified);
    }

    private async Task ImportConfigAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON �ļ� (*.json)|*.json|�����ļ� (*.*)|*.*",
            Title = "�����豸�����ļ�"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            StatusMessage = "���ڵ�������...";
            var config = await _configService.ImportFromFileAsync(dialog.FileName);
            CurrentConfig = config;

            AllPositions.Clear();
            FilteredPositions.Clear();
            DeviceFilters.Clear();
            DeviceFilters.Add("ȫ��");

            foreach (var motor in config.Motors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "CAN���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            foreach (var motor in config.EtherCATMotors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "EtherCAT���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            foreach (var cent in config.CentrifugalDevices)
            {
                DeviceFilters.Add(cent.Name);
                foreach (var pos in cent.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = cent.DeviceId,
                        DeviceName = cent.Name,
                        DeviceType = "���Ļ�",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            FilterPositions();
            UpdateStatistics();

            StatusMessage = $"�ɹ��������ã��� {AllPositions.Count} ��λ�õ�";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "�����豸����ʧ��");
            StatusMessage = $"����ʧ��: {ex.Message}";
        }
    }

    private async Task SaveConfigAsync()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "û�пɱ��������";
            return;
        }

        try
        {
            StatusMessage = "���ڱ�������...";

            foreach (var pos in AllPositions.Where(p => p.IsModified))
            {
                var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == pos.DeviceId);
                if (motor != null)
                {
                    var workPos = motor.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;

                        _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                        {
                            DeviceId = pos.DeviceId,
                            PositionName = pos.PositionName,
                            Position = pos.Position,
                            Speed = pos.Speed
                        });
                    }
                }

                var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == pos.DeviceId);
                if (ecatMotor != null)
                {
                    var workPos = ecatMotor.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;

                        _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                        {
                            DeviceId = pos.DeviceId,
                            PositionName = pos.PositionName,
                            Position = pos.Position,
                            Speed = pos.Speed
                        });
                    }
                }

                var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == pos.DeviceId);
                if (cent != null)
                {
                    var workPos = cent.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;

                        _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                        {
                            DeviceId = pos.DeviceId,
                            PositionName = pos.PositionName,
                            Position = pos.Position,
                            Speed = pos.Speed
                        });
                    }
                }

                pos.IsModified = false;
            }

            await _configService.SaveConfigAsync(CurrentConfig);

            UpdateStatistics();
            StatusMessage = "���ñ���ɹ�";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "��������ʧ��");
            StatusMessage = $"����ʧ��: {ex.Message}";
        }
    }

    private async Task ExportConfigAsync()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "û�пɵ���������";
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "JSON �ļ� (*.json)|*.json|�����ļ� (*.*)|*.*",
            Title = "�����豸�����ļ�",
            FileName = "deviceconfig_export.json"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            StatusMessage = "���ڵ�������...";
            await _configService.ExportToFileAsync(CurrentConfig, dialog.FileName);
            StatusMessage = $"�����ѵ����� {dialog.FileName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "��������ʧ��");
            StatusMessage = $"����ʧ��: {ex.Message}";
        }
    }

    private async Task MoveToPositionAsync()
    {
        if (SelectedPosition == null) return;

        try
        {
            StatusMessage = $"�����ƶ��� {SelectedPosition.PositionName}...";
            await _hardwareController.MoveMotorAsync(
                SelectedPosition.DeviceId,
                SelectedPosition.Position,
                SelectedPosition.Speed,
                false,
                true);
            StatusMessage = $"���ƶ��� {SelectedPosition.PositionName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "�ƶ���λ��ʧ��");
            StatusMessage = $"�ƶ�ʧ��: {ex.Message}";
        }
    }

    private async Task TeachPositionAsync()
    {
        if (SelectedPosition == null) return;

        try
        {
            StatusMessage = $"����ʾ�� {SelectedPosition.PositionName}...";
            var currentPos = await _hardwareController.GetMotorPositionAsync(SelectedPosition.DeviceId);
            SelectedPosition.Position = currentPos;
            SelectedPosition.IsModified = true;
            UpdateStatistics();
            StatusMessage = $"��ʾ�� {SelectedPosition.PositionName} = {currentPos}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ʾ��λ��ʧ��");
            StatusMessage = $"ʾ��ʧ��: {ex.Message}";
        }
    }

    private void ResetPosition()
    {
        if (SelectedPosition == null) return;

        if (CurrentConfig == null) return;

        var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == SelectedPosition.DeviceId);
        if (motor != null)
        {
            var workPos = motor.WorkPositions.FirstOrDefault(w => w.Name == SelectedPosition.PositionName);
            if (workPos != null)
            {
                SelectedPosition.Position = workPos.Position;
                SelectedPosition.Speed = workPos.Speed;
                SelectedPosition.IsModified = false;
                UpdateStatistics();
                StatusMessage = $"������ {SelectedPosition.PositionName}";
                return;
            }
        }

        var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == SelectedPosition.DeviceId);
        if (ecatMotor != null)
        {
            var workPos = ecatMotor.WorkPositions.FirstOrDefault(w => w.Name == SelectedPosition.PositionName);
            if (workPos != null)
            {
                SelectedPosition.Position = workPos.Position;
                SelectedPosition.Speed = workPos.Speed;
                SelectedPosition.IsModified = false;
                UpdateStatistics();
                StatusMessage = $"������ {SelectedPosition.PositionName}";
            }
        }
    }

    private void AddPosition()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "���ȵ��������ļ�";
            return;
        }

        try
        {
            var dialog = new AddPositionDialog(CurrentConfig)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            while (dialog.ShowDialog() == true)
            {
                var newPosition = new PositionPointViewModel
                {
                    DeviceId = dialog.SelectedDeviceId!,
                    DeviceName = dialog.SelectedDeviceName!,
                    DeviceType = dialog.SelectedDeviceType!,
                    PositionName = dialog.PositionName,
                    Position = dialog.PositionValue,
                    Speed = dialog.Speed,
                    IsModified = true
                };

                AllPositions.Add(newPosition);
                FilterPositions();
                UpdateStatistics();
                SelectedPosition = newPosition;

                AddPositionToConfig(newPosition);

                _eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);

                StatusMessage = $"������λ�õ�: {newPosition.PositionName} �� {newPosition.DeviceName}";

                if (!dialog.ContinueAdding)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "����λ�õ�ʧ��");
            StatusMessage = $"����ʧ��: {ex.Message}";
        }
    }

    private void AddPositionToConfig(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;

        if (position.DeviceType.Contains("CAN"))
        {
            var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                motor.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
            }
        }
        else if (position.DeviceType.Contains("EtherCAT"))
        {
            var motor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                motor.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
            }
        }
        else if (position.DeviceType.Contains("���Ļ�"))
        {
            var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == position.DeviceId);
            if (cent != null)
            {
                cent.WorkPositions.Add(new WorkPositionDto
                {
                    Name = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
            }
        }
    }

    private void DeletePosition()
    {
        if (SelectedPosition == null)
        {
            StatusMessage = "����ѡ��Ҫɾ����λ�õ�";
            return;
        }

        var result = System.Windows.MessageBox.Show(
            $"ȷ��Ҫɾ��λ�õ� '{SelectedPosition.PositionName}' ��",
            "ȷ��ɾ��",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result != System.Windows.MessageBoxResult.Yes) return;

        try
        {
            RemovePositionFromConfig(SelectedPosition);

            var posToDelete = SelectedPosition;
            SelectedPosition = null;
            AllPositions.Remove(posToDelete);
            FilterPositions();
            UpdateStatistics();

            _eventAggregator.GetEvent<PositionDeletedEvent>().Publish(posToDelete);

            StatusMessage = $"��ɾ��λ�õ�: {posToDelete.PositionName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ɾ��λ�õ�ʧ��");
            StatusMessage = $"ɾ��ʧ��: {ex.Message}";
        }
    }

    private void RemovePositionFromConfig(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;

        if (position.DeviceType.Contains("CAN"))
        {
            var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                var workPos = motor.WorkPositions.FirstOrDefault(w => w.Name == position.PositionName);
                if (workPos != null)
                {
                    motor.WorkPositions.Remove(workPos);
                }
            }
        }
        else if (position.DeviceType.Contains("EtherCAT"))
        {
            var motor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
            if (motor != null)
            {
                var workPos = motor.WorkPositions.FirstOrDefault(w => w.Name == position.PositionName);
                if (workPos != null)
                {
                    motor.WorkPositions.Remove(workPos);
                }
            }
        }
        else if (position.DeviceType.Contains("���Ļ�"))
        {
            var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == position.DeviceId);
            if (cent != null)
            {
                var workPos = cent.WorkPositions.FirstOrDefault(w => w.Name == position.PositionName);
                if (workPos != null)
                {
                    cent.WorkPositions.Remove(workPos);
                }
            }
        }
    }

    private void OnConfigImported(DeviceConfigDto config)
    {
        try
        {
            CurrentConfig = config;

            AllPositions.Clear();
            FilteredPositions.Clear();
            DeviceFilters.Clear();
            DeviceFilters.Add("ȫ��");

            foreach (var motor in config.Motors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "CAN���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            foreach (var motor in config.EtherCATMotors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "EtherCAT���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            foreach (var cent in config.CentrifugalDevices)
            {
                DeviceFilters.Add(cent.Name);
                foreach (var pos in cent.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = cent.DeviceId,
                        DeviceName = cent.Name,
                        DeviceType = "���Ļ�",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            FilterPositions();
            UpdateStatistics();

            StatusMessage = $"�Ѵӵ��Խ���ͬ�����ã��� {AllPositions.Count} ��λ�õ�";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ͬ������ʧ��");
            StatusMessage = $"ͬ��ʧ��: {ex.Message}";
        }
    }

    /// <summary>
    /// Handle configuration loaded event (from DesignerView or DeviceDebugView)
    /// </summary>
    private void OnConfigLoaded(ConfigLoadedEventArgs args)
    {
        try
        {
            CurrentConfig = args.Config;

            // Clear lists
            AllPositions.Clear();
            FilteredPositions.Clear();
            DeviceFilters.Clear();
            DeviceFilters.Add("ȫ��");

            // Load positions from CAN motors
            foreach (var motor in args.Config.Motors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "CAN���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            // Load positions from EtherCAT motors
            foreach (var motor in args.Config.EtherCATMotors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "EtherCAT���",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            // Load positions from centrifugal devices
            foreach (var cent in args.Config.CentrifugalDevices)
            {
                DeviceFilters.Add(cent.Name);
                foreach (var pos in cent.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = cent.DeviceId,
                        DeviceName = cent.Name,
                        DeviceType = "���Ļ�",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }

            // Update filter and statistics
            FilterPositions();
            UpdateStatistics();

            StatusMessage = $"�����Ѽ��� (��Դ: {args.Source})���� {AllPositions.Count} ��λ�õ�";
            _logger.Info($"Configuration loaded from {args.Source}: {AllPositions.Count} positions");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load configuration");
            StatusMessage = $"��������ʧ��: {ex.Message}";
        }
    }

    private void AddDevice()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "���ȵ��������ļ�";
            return;
        }

        try
        {
            var dialog = new AddDeviceDialog
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                var deviceType = dialog.DeviceType;
                var deviceId = dialog.DeviceId;
                var deviceName = dialog.DeviceName;
                var description = dialog.Description;

                if (IsDeviceIdExists(deviceId))
                {
                    StatusMessage = $"�豸 ID '{deviceId}' �Ѵ���";
                    System.Windows.MessageBox.Show(
                        $"�豸 ID '{deviceId}' �Ѵ��ڣ���ʹ������ ID��",
                        "�豸 ID ��ͻ",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                switch (deviceType)
                {
                    case "CAN ���":
                        AddCanMotor(deviceId, deviceName, description, dialog.CanNodeId ?? 1);
                        break;
                    case "EtherCAT ���":
                        AddEtherCATMotor(deviceId, deviceName, description, dialog.SlaveId ?? 1);
                        break;
                    case "ע���":
                        AddSyringePump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "�䶯��":
                        AddPeristalticPump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "�Զ����":
                        AddDiyPump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "���Ļ�":
                        AddCentrifugalDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "TCU �¿�":
                        AddTcuDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "��ˮ��":
                        AddChillerDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "���ش�����":
                        AddWeighingSensor(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "ɨ��ǹ":
                        AddScanner(deviceId, deviceName, description, dialog.IpAddress, dialog.Port ?? 9000);
                        break;
                    case "Jaka ������":
                        AddJakaRobot(deviceId, deviceName, description, dialog.IpAddress, dialog.Port ?? 10000);
                        break;
                    case "IO �豸":
                        AddEcatIODevice(deviceId, deviceName, description, dialog.SlaveId ?? 1);
                        break;
                }

                if (!DeviceFilters.Contains(deviceName))
                {
                    DeviceFilters.Add(deviceName);
                }

                UpdateStatistics();
                StatusMessage = $"�������豸: {deviceName} ({deviceType})";

                // _eventAggregator.GetEvent<DeviceAddedEvent>().Publish(newDevice);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "�����豸ʧ��");
            StatusMessage = $"�����豸ʧ��: {ex.Message}";
        }
    }

    private bool IsDeviceIdExists(string deviceId)
    {
        if (CurrentConfig == null) return false;

        return CurrentConfig.Motors.Any(m => m.DeviceId == deviceId) ||
               CurrentConfig.EtherCATMotors.Any(m => m.DeviceId == deviceId) ||
               CurrentConfig.SyringePumps.Any(p => p.DeviceId == deviceId) ||
               CurrentConfig.PeristalticPumps.Any(p => p.DeviceId == deviceId) ||
               CurrentConfig.DiyPumps.Any(p => p.DeviceId == deviceId) ||
               CurrentConfig.CentrifugalDevices.Any(c => c.DeviceId == deviceId) ||
               CurrentConfig.TcuDevices.Any(t => t.DeviceId == deviceId) ||
               CurrentConfig.ChillerDevices.Any(c => c.DeviceId == deviceId) ||
               CurrentConfig.WeighingSensors.Any(w => w.DeviceId == deviceId) ||
               CurrentConfig.Scanners.Any(s => s.DeviceId == deviceId) ||
               CurrentConfig.JakaRobots.Any(r => r.DeviceId == deviceId) ||
               CurrentConfig.EcatIODevices.Any(io => io.DeviceId == deviceId);
    }

    private void AddCanMotor(string deviceId, string deviceName, string description, int nodeId)
    {
        var motor = new MotorDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            Type = "CAN Motor",
            AxisId = nodeId,
            CommunicationId = 1,
            DeviceIndex = 0,
            ChannelIndex = 0,
            WorkPositions = new List<WorkPositionDto>(),
            Parameters = new MotorParametersDto
            {
                Unit = "mm",
                JogSpeed = 100
            }
        };

        CurrentConfig.Motors.Add(motor);
    }

    private void AddEtherCATMotor(string deviceId, string deviceName, string description, int slaveId)
    {
        var motor = new EtherCATMotorDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            Type = "EtherCAT Motor",
            AxisNo = slaveId,
            EtherCATDeviceId = $"EtherCAT_{slaveId}",
            WorkPositions = new List<WorkPositionDto>(),
            Parameters = new MotorParametersDto
            {
                Unit = "mm",
                JogSpeed = 100
            }
        };

        CurrentConfig.EtherCATMotors.Add(motor);
    }

    private void AddSyringePump(string deviceId, string deviceName, string description, string? portName)
    {
        var pump = new SyringePumpDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            Type = "SyringePump",
            CommunicationId = 1,
            DeviceIndex = 0,
            ChannelIndex = 1,
            CanFrameId = 0,
            SyringeVolume = 50,
            LiquidOffset = 0
        };

        CurrentConfig.SyringePumps.Add(pump);
    }

    private void AddPeristalticPump(string deviceId, string deviceName, string description, string? portName)
    {
        var pump = new PeristalticPumpDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            Type = "PeristalticPump",
            CommunicationId = 1,
            DeviceIndex = 0,
            ChannelIndex = 0,
            CanFrameId = 0,
            ProductModel = "Generic",
            PumpHeadModel = "YZ15",
            TubeSpec = "15#",
            RotorCount = 1,
            MaxRPM = 600,
            MaxFlowRate = 100,
            LiquidOffset = 0,
            PumpAccuracy = 1.0,
            IsEnabled = true,
            Description = description
        };

        CurrentConfig.PeristalticPumps.Add(pump);
    }

    private void AddDiyPump(string deviceId, string deviceName, string description, string? portName)
    {
        var pump = new DiyPumpDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            PulsePerRevolution = 200,
            GearRatio = 1.0,
            DisplacementPerRevolution = 1.0,
            OffsetPosition = 0,
            CommunicationId = 1,
            MaxRPM = 600,
            IsEnabled = true
        };

        CurrentConfig.DiyPumps.Add(pump);
    }

    private void AddCentrifugalDevice(string deviceId, string deviceName, string description, string? portName)
    {
        var centrifugal = new CentrifugalDeviceDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            Type = "Centrifugal",
            PortName = portName ?? "COM1",
            BaudRate = 9600,
            WorkPositions = new List<WorkPositionDto>()
        };

        CurrentConfig.CentrifugalDevices.Add(centrifugal);
    }

    private void AddTcuDevice(string deviceId, string deviceName, string description, string? portName)
    {
        var tcu = new TcuDeviceDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            PortName = portName ?? "COM1",
            BaudRate = 9600
        };

        CurrentConfig.TcuDevices.Add(tcu);
    }

    private void AddChillerDevice(string deviceId, string deviceName, string description, string? portName)
    {
        var chiller = new ChillerDeviceDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            PortName = portName ?? "COM1",
            BaudRate = 9600
        };

        CurrentConfig.ChillerDevices.Add(chiller);
    }

    private void AddWeighingSensor(string deviceId, string deviceName, string description, string? portName)
    {
        var sensor = new WeighingSensorDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            PortName = portName ?? "COM1",
            BaudRate = 9600,
            MaxWeight = 1000,
            DecimalPlaces = 2
        };

        CurrentConfig.WeighingSensors.Add(sensor);
    }

    private void AddScanner(string deviceId, string deviceName, string description, string? ipAddress, int port)
    {
        var scanner = new ScannerDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            IpAddress = ipAddress ?? "192.168.1.100",
            Port = port
        };

        CurrentConfig.Scanners.Add(scanner);
    }

    private void AddJakaRobot(string deviceId, string deviceName, string description, string? ipAddress, int port)
    {
        var robot = new JakaRobotDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            IpAddress = ipAddress ?? "192.168.1.100",
            Port = port
        };

        CurrentConfig.JakaRobots.Add(robot);
    }

    private void AddEcatIODevice(string deviceId, string deviceName, string description, int slaveId)
    {
        var ioDevice = new EcatIODeviceDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            EtherCATDeviceId = $"EtherCAT_{slaveId}",
            IoChannels = new List<IoChannelDto>()
        };

        CurrentConfig.EcatIODevices.Add(ioDevice);
    }
}





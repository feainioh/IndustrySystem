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
/// 位置点位设置 ViewModel
/// </summary>
public class PositionSettingsViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    private readonly IDeviceConfigService _configService;
    private readonly IHardwareController _hardwareController;
    private readonly IEventAggregator _eventAggregator;
    
    private DeviceConfigDto? _currentConfig;
    private string _statusMessage = "就绪";
    private string _searchText = string.Empty;
    private PositionPointViewModel? _selectedPosition;
    private string _selectedDeviceFilter = "全部";
    
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
    
    // 位置点列表
    public ObservableCollection<PositionPointViewModel> AllPositions { get; } = new();
    public ObservableCollection<PositionPointViewModel> FilteredPositions { get; } = new();
    
    // 设备筛选列表
    public ObservableCollection<string> DeviceFilters { get; } = new() { "全部" };
    
    // 统计信息
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
    
    // 命令
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
        
        // 订阅配置导入事件（从 DeviceDebugView 同步到这里）
        _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Subscribe(OnConfigImported, ThreadOption.UIThread);
        
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
            
            var matchesFilter = SelectedDeviceFilter == "全部" ||
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
        MotorPositionCount = AllPositions.Count(p => p.DeviceType.Contains("电机"));
        RobotPositionCount = AllPositions.Count(p => p.DeviceType.Contains("机器人"));
        ModifiedCount = AllPositions.Count(p => p.IsModified);
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
            
            // 清空列表
            AllPositions.Clear();
            FilteredPositions.Clear();
            DeviceFilters.Clear();
            DeviceFilters.Add("全部");
            
            // 导入 CAN 电机位置
            foreach (var motor in config.Motors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "CAN电机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 导入 EtherCAT 电机位置
            foreach (var motor in config.EtherCATMotors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "EtherCAT电机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 导入离心机位置
            foreach (var cent in config.CentrifugalDevices)
            {
                DeviceFilters.Add(cent.Name);
                foreach (var pos in cent.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = cent.DeviceId,
                        DeviceName = cent.Name,
                        DeviceType = "离心机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 更新筛选和统计
            FilterPositions();
            UpdateStatistics();
            
            StatusMessage = $"成功导入配置，共 {AllPositions.Count} 个位置点";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导入设备配置失败");
            StatusMessage = $"导入失败: {ex.Message}";
        }
    }
    
    private async Task SaveConfigAsync()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "没有可保存的配置";
            return;
        }
        
        try
        {
            StatusMessage = "正在保存配置...";
            
            // 更新配置中的位置点
            foreach (var pos in AllPositions.Where(p => p.IsModified))
            {
                // 更新 CAN 电机
                var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == pos.DeviceId);
                if (motor != null)
                {
                    var workPos = motor.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;
                        
                        // 发布位置更新事件，通知 DeviceDebugView
                        _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                        {
                            DeviceId = pos.DeviceId,
                            PositionName = pos.PositionName,
                            Position = pos.Position,
                            Speed = pos.Speed
                        });
                    }
                }
                
                // 更新 EtherCAT 电机
                var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == pos.DeviceId);
                if (ecatMotor != null)
                {
                    var workPos = ecatMotor.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;
                        
                        // 发布位置更新事件
                        _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                        {
                            DeviceId = pos.DeviceId,
                            PositionName = pos.PositionName,
                            Position = pos.Position,
                            Speed = pos.Speed
                        });
                    }
                }
                
                // 更新离心机
                var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == pos.DeviceId);
                if (cent != null)
                {
                    var workPos = cent.WorkPositions.FirstOrDefault(w => w.Name == pos.PositionName);
                    if (workPos != null)
                    {
                        workPos.Position = pos.Position;
                        workPos.Speed = pos.Speed;
                        
                        // 发布位置更新事件
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
            StatusMessage = "配置保存成功";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存配置失败");
            StatusMessage = $"保存失败: {ex.Message}";
        }
    }
    
    private async Task ExportConfigAsync()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "没有可导出的配置";
            return;
        }
        
        var dialog = new SaveFileDialog
        {
            Filter = "JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "导出设备配置文件",
            FileName = "deviceconfig_export.json"
        };
        
        if (dialog.ShowDialog() != true) return;
        
        try
        {
            StatusMessage = "正在导出配置...";
            await _configService.ExportToFileAsync(CurrentConfig, dialog.FileName);
            StatusMessage = $"配置已导出到 {dialog.FileName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导出配置失败");
            StatusMessage = $"导出失败: {ex.Message}";
        }
    }
    
    private async Task MoveToPositionAsync()
    {
        if (SelectedPosition == null) return;
        
        try
        {
            StatusMessage = $"正在移动到 {SelectedPosition.PositionName}...";
            await _hardwareController.MoveMotorAsync(
                SelectedPosition.DeviceId,
                SelectedPosition.Position,
                SelectedPosition.Speed,
                false,
                true);
            StatusMessage = $"已移动到 {SelectedPosition.PositionName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "移动到位置失败");
            StatusMessage = $"移动失败: {ex.Message}";
        }
    }
    
    private async Task TeachPositionAsync()
    {
        if (SelectedPosition == null) return;
        
        try
        {
            StatusMessage = $"正在示教 {SelectedPosition.PositionName}...";
            var currentPos = await _hardwareController.GetMotorPositionAsync(SelectedPosition.DeviceId);
            SelectedPosition.Position = currentPos;
            SelectedPosition.IsModified = true;
            UpdateStatistics();
            StatusMessage = $"已示教 {SelectedPosition.PositionName} = {currentPos}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "示教位置失败");
            StatusMessage = $"示教失败: {ex.Message}";
        }
    }
    
    private void ResetPosition()
    {
        if (SelectedPosition == null) return;
        
        // 从原始配置恢复位置
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
                StatusMessage = $"已重置 {SelectedPosition.PositionName}";
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
                StatusMessage = $"已重置 {SelectedPosition.PositionName}";
            }
        }
    }
    
    private void AddPosition()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "请先导入配置文件";
            return;
        }
        
        try
        {
            // 打开添加位置对话框
            var dialog = new AddPositionDialog(CurrentConfig)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            
            // 循环添加（支持连续添加）
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
                
                // 同时添加到配置中
                AddPositionToConfig(newPosition);
                
                // 发布位置添加事件，通知 DeviceDebugView
                _eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);
                
                StatusMessage = $"已添加位置点: {newPosition.PositionName} 到 {newPosition.DeviceName}";
                
                // 如果不继续添加，退出循环
                if (!dialog.ContinueAdding)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "添加位置点失败");
            StatusMessage = $"添加失败: {ex.Message}";
        }
    }
    
    private void AddPositionToConfig(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;
        
        // 根据设备类型添加到对应的配置中
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
        else if (position.DeviceType.Contains("离心机"))
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
            StatusMessage = "请先选择要删除的位置点";
            return;
        }
        
        // 确认删除
        var result = System.Windows.MessageBox.Show(
            $"确定要删除位置点 '{SelectedPosition.PositionName}' 吗？",
            "确认删除",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);
        
        if (result != System.Windows.MessageBoxResult.Yes) return;
        
        try
        {
            // 从配置中删除
            RemovePositionFromConfig(SelectedPosition);
            
            // 从列表中删除
            var posToDelete = SelectedPosition;
            SelectedPosition = null;
            AllPositions.Remove(posToDelete);
            FilterPositions();
            UpdateStatistics();
            
            // 发布位置删除事件，通知 DeviceDebugView
            _eventAggregator.GetEvent<PositionDeletedEvent>().Publish(posToDelete);
            
            StatusMessage = $"已删除位置点: {posToDelete.PositionName}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "删除位置点失败");
            StatusMessage = $"删除失败: {ex.Message}";
        }
    }
    
    private void RemovePositionFromConfig(PositionPointViewModel position)
    {
        if (CurrentConfig == null) return;
        
        // 根据设备类型从对应的配置中删除
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
        else if (position.DeviceType.Contains("离心机"))
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
    
    // 事件处理 - 从 DeviceDebugView 同步配置
    private void OnConfigImported(DeviceConfigDto config)
    {
        try
        {
            CurrentConfig = config;
            
            // 清空列表
            AllPositions.Clear();
            FilteredPositions.Clear();
            DeviceFilters.Clear();
            DeviceFilters.Add("全部");
            
            // 加载 CAN 电机位置
            foreach (var motor in config.Motors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "CAN电机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 加载 EtherCAT 电机位置
            foreach (var motor in config.EtherCATMotors)
            {
                DeviceFilters.Add(motor.Name);
                foreach (var pos in motor.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = motor.DeviceId,
                        DeviceName = motor.Name,
                        DeviceType = "EtherCAT电机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 加载离心机位置
            foreach (var cent in config.CentrifugalDevices)
            {
                DeviceFilters.Add(cent.Name);
                foreach (var pos in cent.WorkPositions)
                {
                    AllPositions.Add(new PositionPointViewModel
                    {
                        DeviceId = cent.DeviceId,
                        DeviceName = cent.Name,
                        DeviceType = "离心机",
                        PositionName = pos.Name,
                        Position = pos.Position,
                        Speed = pos.Speed,
                        IsModified = false
                    });
                }
            }
            
            // 更新筛选和统计
            FilterPositions();
            UpdateStatistics();
            
            StatusMessage = $"已从调试界面同步配置，共 {AllPositions.Count} 个位置点";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "同步配置失败");
            StatusMessage = $"同步失败: {ex.Message}";
        }
    }
    
    // 添加新设备
    private void AddDevice()
    {
        if (CurrentConfig == null)
        {
            StatusMessage = "请先导入配置文件";
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
                
                // 检查设备 ID 是否已存在
                if (IsDeviceIdExists(deviceId))
                {
                    StatusMessage = $"设备 ID '{deviceId}' 已存在";
                    System.Windows.MessageBox.Show(
                        $"设备 ID '{deviceId}' 已存在，请使用其他 ID。",
                        "设备 ID 冲突",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }
                
                // 根据设备类型创建设备
                switch (deviceType)
                {
                    case "CAN 电机":
                        AddCanMotor(deviceId, deviceName, description, dialog.CanNodeId ?? 1);
                        break;
                    case "EtherCAT 电机":
                        AddEtherCATMotor(deviceId, deviceName, description, dialog.SlaveId ?? 1);
                        break;
                    case "注射泵":
                        AddSyringePump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "蠕动泵":
                        AddPeristalticPump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "自定义泵":
                        AddDiyPump(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "离心机":
                        AddCentrifugalDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "TCU 温控":
                        AddTcuDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "冷水机":
                        AddChillerDevice(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "称重传感器":
                        AddWeighingSensor(deviceId, deviceName, description, dialog.PortName);
                        break;
                    case "扫码枪":
                        AddScanner(deviceId, deviceName, description, dialog.IpAddress, dialog.Port ?? 9000);
                        break;
                    case "Jaka 机器人":
                        AddJakaRobot(deviceId, deviceName, description, dialog.IpAddress, dialog.Port ?? 10000);
                        break;
                    case "IO 设备":
                        AddEcatIODevice(deviceId, deviceName, description, dialog.SlaveId ?? 1);
                        break;
                }
                
                // 刷新设备筛选列表
                if (!DeviceFilters.Contains(deviceName))
                {
                    DeviceFilters.Add(deviceName);
                }
                
                UpdateStatistics();
                StatusMessage = $"已添加设备: {deviceName} ({deviceType})";
                
                // 发布设备添加事件（如果需要）
                // _eventAggregator.GetEvent<DeviceAddedEvent>().Publish(newDevice);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "添加设备失败");
            StatusMessage = $"添加设备失败: {ex.Message}";
        }
    }
    
    // 检查设备 ID 是否已存在
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
    
    // 添加 CAN 电机
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
    
    // 添加 EtherCAT 电机
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
    
    // 添加注射泵
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
    
    // 添加蠕动泵
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
    
    // 添加自定义泵
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
    
    // 添加离心机
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
    
    // 添加 TCU 设备
    private void AddTcuDevice(string deviceId, string deviceName, string description, string? portName)
    {        var tcu = new TcuDeviceDto
        {
            DeviceId = deviceId,
            Name = deviceName,
            PortName = portName ?? "COM1",
            BaudRate = 9600
        };
        
        CurrentConfig.TcuDevices.Add(tcu);
    }
    
    // 添加冷水机
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
    
    // 添加称重传感器
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
    
    // 添加扫码枪
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
    
    // 添加 Jaka 机器人
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
    
    // 添加 IO 设备
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





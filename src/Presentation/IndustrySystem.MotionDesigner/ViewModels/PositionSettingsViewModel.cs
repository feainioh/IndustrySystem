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
/// 位置点位设置 ViewModel
/// </summary>
public class PositionSettingsViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    
    private readonly IDeviceConfigService _configService;
    private readonly IHardwareController _hardwareController;
    
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
    
    public PositionSettingsViewModel(IDeviceConfigService configService, IHardwareController hardwareController)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        
        ImportConfigCommand = new DelegateCommand(async () => await ImportConfigAsync());
        SaveConfigCommand = new DelegateCommand(async () => await SaveConfigAsync());
        ExportConfigCommand = new DelegateCommand(async () => await ExportConfigAsync());
        MoveToPositionCommand = new DelegateCommand(async () => await MoveToPositionAsync());
        TeachPositionCommand = new DelegateCommand(async () => await TeachPositionAsync());
        ResetPositionCommand = new DelegateCommand(ResetPosition);
        AddPositionCommand = new DelegateCommand(AddPosition);
        DeletePositionCommand = new DelegateCommand(DeletePosition);
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
        // TODO: 打开添加位置对话框
        StatusMessage = "添加位置功能待实现";
    }
    
    private void DeletePosition()
    {
        if (SelectedPosition == null) return;
        
        AllPositions.Remove(SelectedPosition);
        FilteredPositions.Remove(SelectedPosition);
        UpdateStatistics();
        StatusMessage = $"已删除位置点";
    }
}

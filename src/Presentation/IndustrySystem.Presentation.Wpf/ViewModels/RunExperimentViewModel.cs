using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using NLog;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class RunExperimentViewModel : NagetiveViewModel
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    // ===== Services =====
    private readonly IExperimentGroupAppService _groupSvc;
    private readonly IExperimentExecutionService _executionSvc;
    private readonly IExperimentAppService _experimentSvc;
    private readonly IAlarmAppService _alarmSvc;
    private readonly IExternalDataSyncAppService? _syncSvc;
    private readonly ICommunicationAppService? _commSvc;
    private readonly IHardwareController? _hwController;
    private readonly IDialogService _dialogService;

    // ===== Execution State =====
    private RunState _state = RunState.Idle;
    private Guid? _executionId;

    // ===== Observable Collections =====
    private readonly List<ExperimentGroupDto> _allGroups = new();
    public ObservableCollection<ExperimentGroupDto> FilteredGroups { get; } = new();
    public ObservableCollection<ExperimentGroupDto> SelectedGroups { get; } = new();
    public ObservableCollection<ExperimentSummaryDto> GroupExperiments { get; } = new();
    public ObservableCollection<ExperimentStepDto> Steps { get; } = new();
    public ObservableCollection<KeyMetricDto> Metrics { get; } = new();
    public ObservableCollection<AlarmDto> RecentAlarms { get; } = new();
    public ObservableCollection<DeviceStatusItem> DeviceItems { get; } = new();

    // ===== Group Search =====
    private string _groupSearchText = string.Empty;
    public string GroupSearchText
    {
        get => _groupSearchText;
        set
        {
            if (SetProperty(ref _groupSearchText, value))
                ApplyGroupFilter();
        }
    }

    // ===== Search Selection (preview, before confirmation) =====
    private ExperimentGroupDto? _searchSelection;
    public ExperimentGroupDto? SearchSelection
    {
        get => _searchSelection;
        set => SetProperty(ref _searchSelection, value);
    }

    // ===== Active Group (currently monitored/executed, selected from DataGrid) =====
    private ExperimentGroupDto? _activeGroup;
    public ExperimentGroupDto? ActiveGroup
    {
        get => _activeGroup;
        set
        {
            if (SetProperty(ref _activeGroup, value))
            {
                CurrentGroupName = value?.Name ?? Resources.Strings.Status_NotSelected;
                RaisePropertyChanged(nameof(CanStart));
                RefreshGroupExperiments();
                RefreshSteps();
            }
        }
    }

    // ===== Mode Toggle =====
    private bool _isOnlineMode;
    public bool IsOnlineMode
    {
        get => _isOnlineMode;
        set
        {
            if (SetProperty(ref _isOnlineMode, value))
            {
                _logger.Info(value ? "Switched to online mode" : "Switched to standalone mode");
            }
        }
    }

    // ===== Status Bar =====
    private bool _isStatusBarExpanded = true;
    public bool IsStatusBarExpanded
    {
        get => _isStatusBarExpanded;
        set => SetProperty(ref _isStatusBarExpanded, value);
    }

    private bool _modbusConnected;
    public bool ModbusConnected
    {
        get => _modbusConnected;
        set => SetProperty(ref _modbusConnected, value);
    }

    private string _modbusStatusText = "未连接";
    public string ModbusStatusText
    {
        get => _modbusStatusText;
        set => SetProperty(ref _modbusStatusText, value);
    }

    private ExternalSyncRuntimeStatusDto? _syncStatus;
    public ExternalSyncRuntimeStatusDto? SyncStatus
    {
        get => _syncStatus;
        set => SetProperty(ref _syncStatus, value);
    }

    private string _syncStatusText = "已禁用";
    public string SyncStatusText
    {
        get => _syncStatusText;
        set => SetProperty(ref _syncStatusText, value);
    }

    private string _deviceSummary = "0台在线";
    public string DeviceSummary
    {
        get => _deviceSummary;
        set => SetProperty(ref _deviceSummary, value);
    }

    private int _apiEndpointCount;
    public int ApiEndpointCount
    {
        get => _apiEndpointCount;
        set => SetProperty(ref _apiEndpointCount, value);
    }

    private string _apiLastSyncTime = "-";
    public string ApiLastSyncTime
    {
        get => _apiLastSyncTime;
        set => SetProperty(ref _apiLastSyncTime, value);
    }

    private string _apiLastError = "-";
    public string ApiLastError
    {
        get => _apiLastError;
        set => SetProperty(ref _apiLastError, value);
    }

    // ===== Monitor Display =====
    private string _currentGroupName = Resources.Strings.Status_NotSelected;
    private string _runStatus = Resources.Strings.Run_State_Idle;
    private string? _statusMessage;
    private int _progress;
    private string _elapsedTime = "00:00:00";
    private string _currentStepProgress = "0/0";

    public string CurrentGroupName { get => _currentGroupName; private set => SetProperty(ref _currentGroupName, value); }
    public string RunStatus { get => _runStatus; private set => SetProperty(ref _runStatus, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }
    public string ElapsedTime { get => _elapsedTime; private set => SetProperty(ref _elapsedTime, value); }
    public string CurrentStepProgress { get => _currentStepProgress; private set => SetProperty(ref _currentStepProgress, value); }

    // ===== CanExecute Guards =====
    public bool CanStart => (_state is RunState.Idle or RunState.Stopped or RunState.Completed) && ActiveGroup != null;
    public bool CanPause => _state == RunState.Running;
    public bool CanStop => _state is RunState.Running or RunState.Paused;
    public bool CanReset => _state is RunState.Stopped or RunState.Completed;
    public bool CanAddToExecutionList => SearchSelection != null && !SelectedGroups.Contains(SearchSelection);

    // ===== Commands =====
    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ToggleStatusBarCommand { get; }
    public ICommand AddToExecutionListCommand { get; }
    public ICommand RemoveFromListCommand { get; }

    public RunExperimentViewModel(
        IExperimentGroupAppService groupSvc,
        IExperimentExecutionService executionSvc,
        IExperimentAppService experimentSvc,
        IAlarmAppService alarmSvc,
        IDialogService dialogService,
        IExternalDataSyncAppService? syncSvc = null,
        ICommunicationAppService? commSvc = null,
        IHardwareController? hwController = null)
    {
        _groupSvc = groupSvc;
        _executionSvc = executionSvc;
        _experimentSvc = experimentSvc;
        _alarmSvc = alarmSvc;
        _dialogService = dialogService;
        _syncSvc = syncSvc;
        _commSvc = commSvc;
        _hwController = hwController;

        StartCommand = new AsyncDelegateCommand(StartAsync);
        PauseCommand = new AsyncDelegateCommand(PauseAsync);
        StopCommand = new AsyncDelegateCommand(StopAsync);
        ResetCommand = new DelegateCommand(ResetExecution, () => CanReset);
        ToggleStatusBarCommand = new DelegateCommand(() => IsStatusBarExpanded = !IsStatusBarExpanded);
        AddToExecutionListCommand = new AsyncDelegateCommand(AddToExecutionListAsync, () => CanAddToExecutionList)
            .ObservesProperty(() => SearchSelection);
        RemoveFromListCommand = new DelegateCommand<ExperimentGroupDto?>(RemoveFromList);

        _ = RefreshLoopAsync();
        _ = LoadGroupsAsync();
        LoadMockMetrics();
        _logger.Info(Resources.Strings.Log_RunExperimentViewModel_Initialized);
    }

    protected override async Task OnRefreshAsync()
    {
        await LoadGroupsAsync();
        _logger.Info("Experiment groups refreshed.");
    }

    // ===== Group Loading & Search =====
    private async Task LoadGroupsAsync()
    {
        try
        {
            var list = await _groupSvc.GetListAsync();
            _allGroups.Clear();
            _allGroups.AddRange(list);
            ApplyGroupFilter();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load experiment groups.");
        }
    }

    private void ApplyGroupFilter()
    {
        var search = GroupSearchText?.Trim() ?? string.Empty;
        IEnumerable<ExperimentGroupDto> filtered = _allGroups;

        if (!string.IsNullOrEmpty(search))
        {
            filtered = _allGroups.Where(g =>
                (g.Name?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (g.GroupCode?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (g.Description?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        FilteredGroups.Clear();
        foreach (var g in filtered) FilteredGroups.Add(g);

        if (SearchSelection != null && !FilteredGroups.Contains(SearchSelection))
            SearchSelection = null;
    }

    private async void RefreshGroupExperiments()
    {
        GroupExperiments.Clear();
        if (ActiveGroup == null || ActiveGroup.StepExperimentIds.Count == 0) return;

        try
        {
            var allExperiments = await _experimentSvc.GetListAsync();
            var idSet = new HashSet<Guid>(ActiveGroup.StepExperimentIds);
            foreach (var exp in allExperiments.Where(e => idSet.Contains(e.Id)))
                GroupExperiments.Add(exp);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load group experiments.");
        }
    }

    // ===== Execution List Management =====
    private async Task AddToExecutionListAsync()
    {
        if (SearchSelection == null || SelectedGroups.Contains(SearchSelection)) return;

        var confirmed = await Task.Run(() =>
        {
            var msg = string.Format("是否将实验组「{0}」加入运行列表？\n\n该组包含 {1} 个实验步骤，运行后将自动依次执行。",
                SearchSelection.Name, SearchSelection.StepExperimentIds.Count);
            return System.Windows.MessageBox.Show(msg, "确认运行实验组",
                System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Question);
        });

        if (confirmed != System.Windows.MessageBoxResult.OK || SearchSelection == null) return;

        SelectedGroups.Add(SearchSelection);
        if (ActiveGroup == null)
            ActiveGroup = SearchSelection;
        _logger.Info(string.Format("Added group to execution list: {0}", SearchSelection.Name));
    }

    private void RemoveFromList(ExperimentGroupDto? group)
    {
        if (group == null) return;
        SelectedGroups.Remove(group);
        if (ActiveGroup == group)
            ActiveGroup = SelectedGroups.FirstOrDefault();
    }

    // ===== Execution Commands =====
    private async Task StartAsync()
    {
        if (ActiveGroup == null) return;
        try
        {
            _logger.Info(string.Format("Starting group: {0}", ActiveGroup.Name));
            _executionId = await _executionSvc.StartGroupAsync(ActiveGroup.Id);
            _state = RunState.Running;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start group execution.");
        }
    }

    private async Task PauseAsync()
    {
        if (_executionId == null) return;
        try
        {
            await _executionSvc.PauseGroupAsync(_executionId.Value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to pause group execution.");
        }
    }

    private async Task StopAsync()
    {
        if (_executionId == null) return;
        try
        {
            await _executionSvc.StopGroupAsync(_executionId.Value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop group execution.");
        }
    }

    private void ResetExecution()
    {
        _executionId = null;
        _state = RunState.Idle;
        Progress = 0;
        RunStatus = Resources.Strings.Run_State_Idle;
        StatusMessage = null;
        ElapsedTime = "00:00:00";
        RaiseCanExecuteChanged();
    }

    // ===== Mock Data =====
    private void LoadMockMetrics()
    {
        Metrics.Clear();
        Metrics.Add(new KeyMetricDto(Resources.Strings.Metric_PressureSensorA, "0.82", "MPa"));
        Metrics.Add(new KeyMetricDto(Resources.Strings.Metric_TemperatureProbeT1, "24.60", "°C"));
        Metrics.Add(new KeyMetricDto(Resources.Strings.Metric_FlowMeterF1, "1.22", "L/min"));
        Metrics.Add(new KeyMetricDto(Resources.Strings.Metric_ConductivityMeter, "12.23", "μS/cm"));
    }

    private void RefreshSteps()
    {
        Steps.Clear();
        if (ActiveGroup == null) return;

        for (var i = 0; i < ActiveGroup.StepExperimentIds.Count; i++)
        {
            var exp = GroupExperiments.FirstOrDefault(e => e.Id == ActiveGroup.StepExperimentIds[i]);
            var name = exp?.Name ?? $"步骤{i + 1}";
            var stepState = i == 0 ? StepState.Running :
                            _state == RunState.Running && i < _progress / 20 ? StepState.Completed :
                            StepState.Pending;
            Steps.Add(new ExperimentStepDto(i, name, stepState));
        }
        CurrentStepProgress = Steps.Count > 0
            ? string.Format(Resources.Strings.Run_StepProgressFormat, Math.Min(_progress / 20 + 1, Steps.Count), Steps.Count)
            : "0/0";
    }

    // ===== Refresh Loop =====
    private async Task RefreshLoopAsync()
    {
        while (true)
        {
            try
            {
                // Poll execution status
                if (_executionId.HasValue)
                {
                    var status = await _executionSvc.GetGroupStatusAsync(_executionId.Value);
                    _state = status.State;
                    RunStatus = ToLocalizedRunState(status.State);
                    Progress = status.State == RunState.Running ? Math.Min(100, status.CurrentStepIndex * 20) : _progress;
                    StatusMessage = status.Message;
                    CurrentStepProgress = $"{status.CurrentStepIndex + 1}/{status.TotalSteps}";

                    if (status.State == RunState.Running)
                        ElapsedTime = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss");
                }

                // Poll external sync status
                if (_syncSvc != null)
                {
                    SyncStatus = await _syncSvc.GetStatusAsync();
                    SyncStatusText = SyncStatus.IsRunning
                        ? $"已处理 {SyncStatus.TotalProcessedCount} 条"
                        : SyncStatus.Enabled ? "等待中" : "已禁用";
                    ApiEndpointCount = SyncStatus.EndpointCount;
                    ApiLastSyncTime = SyncStatus.LastSyncedAt?.LocalDateTime.ToString("HH:mm:ss") ?? "-";
                    ApiLastError = SyncStatus.LastError ?? "-";
                }

                // Poll alarms
                try
                {
                    var alarms = await _alarmSvc.GetActiveAsync();
                    RecentAlarms.Clear();
                    foreach (var a in alarms.Take(5)) RecentAlarms.Add(a);
                }
                catch { /* ignore alarm polling errors */ }

                // Poll device status (simulated)
                RefreshDeviceStatus();

                RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Refresh loop error.");
            }

            await Task.Delay(1000);
        }
    }

    private void RefreshDeviceStatus()
    {
        DeviceItems.Clear();
        if (_hwController == null) return;

        try
        {
            // Add a few simulated device status entries
            DeviceItems.Add(new DeviceStatusItem("电机A", true, "就绪", "Motor"));
            DeviceItems.Add(new DeviceStatusItem("IO模块", true, "4路输入", "IO"));
            DeviceItems.Add(new DeviceStatusItem("传感器", true, "数据采集中", "Sensor"));
        }
        catch
        {
            // Non-critical
        }

        var online = DeviceItems.Count(d => d.IsOnline);
        DeviceSummary = $"{online}台在线";
    }

    private void RaiseCanExecuteChanged()
    {
        RaisePropertyChanged(nameof(CanStart));
        RaisePropertyChanged(nameof(CanPause));
        RaisePropertyChanged(nameof(CanStop));
        RaisePropertyChanged(nameof(CanReset));
    }

    private static string ToLocalizedRunState(RunState state) => state switch
    {
        RunState.Running => Resources.Strings.Run_State_Running,
        RunState.Paused => Resources.Strings.Run_State_Paused,
        RunState.Stopped => Resources.Strings.Run_State_Stopped,
        RunState.Completed => Resources.Strings.Run_State_Completed,
        _ => Resources.Strings.Run_State_Idle
    };
}

/// <summary>
/// Lightweight bindable item for the device status bar.
/// </summary>
public class DeviceStatusItem
{
    public string DeviceName { get; }
    public bool IsOnline { get; }
    public string StatusText { get; }
    public string DeviceType { get; }

    public DeviceStatusItem(string deviceName, bool isOnline, string statusText, string deviceType)
    {
        DeviceName = deviceName;
        IsOnline = isOnline;
        StatusText = statusText;
        DeviceType = deviceType;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Commands;
using System.Collections.ObjectModel;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class RunExperimentViewModel : NagetiveCurdVeiwModel<ExperimentExecutionDto>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IRunExperimentAppService _svc;
    private readonly IExperimentAppService _experimentSvc;
    private readonly IAlarmAppService _alarmSvc;
    private RunState _state = RunState.Idle;
    private string _currentExperiment = Resources.Strings.Status_NotSelected;
    public ObservableCollection<ExperimentSummaryDto> Experiments { get; } = new();
    public ObservableCollection<ExperimentExecutionDto> Executions => Items;
    public ObservableCollection<ExperimentStepDto> Steps { get; } = new();
    public ObservableCollection<KeyMetricDto> Metrics { get; } = new();
    public ObservableCollection<AlarmDto> RecentAlarms { get; } = new();

    private ExperimentSummaryDto? _selectedExperiment;
    public ExperimentSummaryDto? SelectedExperiment
    {
        get => _selectedExperiment;
        set
        {
            if (SetProperty(ref _selectedExperiment, value))
            {
                CurrentExperiment = value?.Name ?? Resources.Strings.Status_NotSelected;
                RaisePropertyChanged(nameof(CanStart));
                RefreshSteps();
            }
        }
    }

    private ExperimentExecutionDto? _selectedExecution;
    public ExperimentExecutionDto? SelectedExecution
    {
        get => _selectedExecution;
        set
        {
            if (SetProperty(ref _selectedExecution, value) && value != null)
            {
                CurrentExperiment = value.ExperimentName;
                RefreshSteps();
            }
        }
    }

    private string _runStatus = Resources.Strings.Run_State_Idle;
    private string? _statusMessage;
    private int _progress = 0;
    private string _elapsedTime = "00:00:00";
    private string _currentStepProgress = "0/0";
    private int _selectedTabIndex = 0;

    public string CurrentExperiment { get => _currentExperiment; private set => SetProperty(ref _currentExperiment, value); }
    public string RunStatus { get => _runStatus; private set => SetProperty(ref _runStatus, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }
    public string ElapsedTime { get => _elapsedTime; private set => SetProperty(ref _elapsedTime, value); }
    public string CurrentStepProgress { get => _currentStepProgress; private set => SetProperty(ref _currentStepProgress, value); }
    public int SelectedTabIndex { get => _selectedTabIndex; set => SetProperty(ref _selectedTabIndex, value); }

    public bool CanStart => (_state is RunState.Idle or RunState.Stopped or RunState.Completed) && SelectedExperiment != null;
    public bool CanPause => _state == RunState.Running;
    public bool CanResume => _state == RunState.Paused;
    public bool CanStop => _state is RunState.Running or RunState.Paused;
    public bool CanReset => _state is RunState.Stopped or RunState.Completed;

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand NewExecutionCommand { get; }

    public RunExperimentViewModel(IRunExperimentAppService svc, IExperimentAppService experimentSvc, IAlarmAppService alarmSvc)
    {
        _svc = svc;
        _experimentSvc = experimentSvc;
        _alarmSvc = alarmSvc;
        StartCommand = new AsyncDelegateCommand(StartAsync);
        PauseCommand = new AsyncDelegateCommand(PauseAsync);
        ResumeCommand = new AsyncDelegateCommand(ResumeAsync);
        StopCommand = new AsyncDelegateCommand(StopAsync);
        ResetCommand = new DelegateCommand(ResetExperiment, () => CanReset);
        NewExecutionCommand = new DelegateCommand(NewExecution);
        _ = RefreshLoopAsync();
        _ = LoadExperimentsAsync();
        LoadMockExecutions();
        LoadMockMetrics();
        _logger.Info(Resources.Strings.Log_RunExperimentViewModel_Initialized);
    }

    private void LoadMockExecutions()
    {
        Executions.Clear();
        Executions.Add(new ExperimentExecutionDto("exec-001", Resources.Strings.Mock_Experiment_FilterValidation, RunState.Running, "2/5", TimeSpan.FromMinutes(2).Add(TimeSpan.FromSeconds(12)), DateTime.Today.AddHours(9).AddMinutes(15).AddSeconds(30)));
        Executions.Add(new ExperimentExecutionDto("exec-002", Resources.Strings.Mock_Experiment_FermentationMonitoring, RunState.Running, "1/5", TimeSpan.FromMinutes(39).Add(TimeSpan.FromSeconds(27)), DateTime.Today.AddHours(8).AddMinutes(42).AddSeconds(10)));
        Executions.Add(new ExperimentExecutionDto("exec-003", Resources.Strings.Mock_Experiment_CellCulture, RunState.Paused, "2/5", TimeSpan.FromMinutes(90), DateTime.Today.AddHours(7).AddMinutes(30)));
        Executions.Add(new ExperimentExecutionDto("exec-004", Resources.Strings.Mock_Experiment_FilterValidation, RunState.Completed, "5/5", TimeSpan.FromMinutes(3), DateTime.Today.AddHours(9)));
        Executions.Add(new ExperimentExecutionDto("exec-005", Resources.Strings.Mock_Experiment_PcrAmplification, RunState.Idle, "0/6", TimeSpan.Zero, null));
        SelectedExecution = Executions[0];
    }

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
        Steps.Add(new ExperimentStepDto(0, Resources.Strings.Step_SealValveClose, StepState.Completed));
        Steps.Add(new ExperimentStepDto(1, Resources.Strings.Step_StartPump, StepState.Completed));
        Steps.Add(new ExperimentStepDto(2, Resources.Strings.Step_Pump3Min, StepState.Running));
        Steps.Add(new ExperimentStepDto(3, Resources.Strings.Step_StopPump, StepState.Pending));
        Steps.Add(new ExperimentStepDto(4, Resources.Strings.Step_CleanAndFinish, StepState.Pending));
        CurrentStepProgress = string.Format(Resources.Strings.Run_StepProgressFormat, 3, Steps.Count);
    }

    private async Task StartAsync()
    {
        if (SelectedExperiment == null) return;
        _logger.Info(string.Format(Resources.Strings.Log_RunExperiment_Start, SelectedExperiment.Name));
        await _svc.StartAsync();
    }

    private async Task PauseAsync()
    {
        _logger.Info(Resources.Strings.Log_RunExperiment_Pause);
        await _svc.PauseAsync();
    }

    private async Task ResumeAsync()
    {
        _logger.Info(Resources.Strings.Log_RunExperiment_Resume);
        await _svc.ResumeAsync();
    }

    private async Task StopAsync()
    {
        _logger.Info(Resources.Strings.Log_RunExperiment_Stop);
        await _svc.StopAsync();
    }

    private void ResetExperiment()
    {
        Progress = 0;
        _state = RunState.Idle;
        RunStatus = Resources.Strings.Run_State_Idle;
        StatusMessage = null;
        ElapsedTime = "00:00:00";
        RaisePropertyChanged(nameof(CanStart));
        RaisePropertyChanged(nameof(CanPause));
        RaisePropertyChanged(nameof(CanResume));
        RaisePropertyChanged(nameof(CanStop));
        RaisePropertyChanged(nameof(CanReset));
    }

    private void NewExecution()
    {
        // 新建执行逻辑占位
    }

    private async Task RefreshLoopAsync()
    {
        while (true)
        {
            var s = await _svc.GetStatusAsync();
            _state = s.State;
            RunStatus = ToLocalizedRunState(s.State);
            Progress = s.Progress;
            StatusMessage = s.Message;
            // 更新运行时长
            if (SelectedExecution != null && s.State == RunState.Running)
            {
                var elapsed = SelectedExecution.Elapsed.Add(TimeSpan.FromMilliseconds(200));
                ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
            }
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanResume));
            RaisePropertyChanged(nameof(CanStop));
            RaisePropertyChanged(nameof(CanReset));
            // 刷新最近报警
            try
            {
                var alarms = await _alarmSvc.GetActiveAsync();
                RecentAlarms.Clear();
                foreach (var a in alarms.Take(5)) RecentAlarms.Add(a);
            }
            catch { /* ignore */ }
            await Task.Delay(1000);
        }
    }

    private async Task LoadExperimentsAsync()
    {
        _logger.Debug(Resources.Strings.Log_RunExperiment_LoadExperiments);
        Experiments.Clear();
        var list = await _experimentSvc.GetListAsync();
        foreach (var e in list) Experiments.Add(e);
        if (SelectedExperiment == null && Experiments.Count > 0)
        {
            SelectedExperiment = Experiments[0];
        }
    }

    private static string ToLocalizedRunState(RunState state) => state switch
    {
        RunState.Running => Resources.Strings.Run_State_Running,
        RunState.Paused => Resources.Strings.Run_State_Paused,
        RunState.Stopped => Resources.Strings.Run_State_Stopped,
        RunState.Completed => Resources.Strings.Run_State_Completed,
        _ => Resources.Strings.Run_State_Idle
    };

    protected override Task<IReadOnlyList<ExperimentExecutionDto>> LoadItemsAsync()
        => Task.FromResult<IReadOnlyList<ExperimentExecutionDto>>(Executions.ToList());
}

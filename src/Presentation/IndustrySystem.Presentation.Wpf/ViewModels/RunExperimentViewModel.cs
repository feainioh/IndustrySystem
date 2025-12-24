using System;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Mvvm;
using Prism.Commands;
using System.Collections.ObjectModel;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class RunExperimentViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IRunExperimentAppService _svc;
    private readonly IExperimentAppService _experimentSvc;
    private RunState _state = RunState.Idle;
    private string _currentExperiment = "未选择";
    public ObservableCollection<ExperimentSummaryDto> Experiments { get; } = new();
    private ExperimentSummaryDto? _selectedExperiment;
    public ExperimentSummaryDto? SelectedExperiment
    {
        get => _selectedExperiment;
        set
        {
            if (SetProperty(ref _selectedExperiment, value))
            {
                CurrentExperiment = value?.Name ?? "未选择";
                RaisePropertyChanged(nameof(CanStart));
            }
        }
    }
    private string _runStatus = "Idle";
    private string? _statusMessage;
    private int _progress = 0;

    public string CurrentExperiment { get => _currentExperiment; private set => SetProperty(ref _currentExperiment, value); }
    public string RunStatus { get => _runStatus; private set => SetProperty(ref _runStatus, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public int Progress { get => _progress; private set => SetProperty(ref _progress, value); }

    public bool CanStart => (_state is RunState.Idle or RunState.Stopped or RunState.Completed) && SelectedExperiment != null;
    public bool CanPause => _state == RunState.Running;
    public bool CanResume => _state == RunState.Paused;
    public bool CanStop => _state is RunState.Running or RunState.Paused;
    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResumeCommand { get; }
    public ICommand StopCommand { get; }

    public RunExperimentViewModel(IRunExperimentAppService svc, IExperimentAppService experimentSvc)
    {
        _svc = svc;
        _experimentSvc = experimentSvc;
        StartCommand = new AsyncDelegateCommand(StartAsync);
        PauseCommand = new AsyncDelegateCommand(PauseAsync);
        ResumeCommand = new AsyncDelegateCommand(ResumeAsync);
        StopCommand = new AsyncDelegateCommand(StopAsync);
        _ = RefreshLoopAsync();
        _ = LoadExperimentsAsync();
        _logger.Info(Resources.Strings.Log_RunExperimentViewModel_Initialized);
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

    private async Task RefreshLoopAsync()
    {
        while (true)
        {
            var s = await _svc.GetStatusAsync();
            _state = s.State;
            RunStatus = s.State.ToString();
            Progress = s.Progress;
            StatusMessage = s.Message;
            RaisePropertyChanged(nameof(CanStart));
            RaisePropertyChanged(nameof(CanPause));
            RaisePropertyChanged(nameof(CanResume));
            RaisePropertyChanged(nameof(CanStop));
            await Task.Delay(200);
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
}

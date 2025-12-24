using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentsViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IExperimentAppService _svc;
    public ObservableCollection<ExperimentItem> Experiments { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentsViewModel(IExperimentAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        DeleteCommand = new AsyncDelegateCommand<Guid>(DeleteAsync);
        _ = LoadAsync();
        _logger.Info(Resources.Strings.Log_ExperimentsViewModel_Initialized);
    }

    public async Task LoadAsync()
    {
        _logger.Debug(Resources.Strings.Log_Experiments_LoadStart);
        Experiments.Clear();
        var list = await _svc.GetListAsync();
        foreach (var e in list) Experiments.Add(new ExperimentItem(e.Id, e.Name, e.Status));
        _logger.Info(string.Format(Resources.Strings.Log_Experiments_LoadComplete, list.Count));
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.Info(string.Format(Resources.Strings.Log_Experiments_Delete, id));
        await _svc.DeleteAsync(id);
        var target = default(ExperimentItem);
        foreach (var e in Experiments) if (e.Id == id) { target = e; break; }
        if (target != null) Experiments.Remove(target);
    }

    public record ExperimentItem(Guid Id, string Name, string Status);
}

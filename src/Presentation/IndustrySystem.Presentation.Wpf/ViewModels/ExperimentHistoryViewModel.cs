using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentHistoryViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IExperimentHistoryAppService _svc;
    public ObservableCollection<HistoryItem> Items { get; } = new();
    public ICommand RefreshCommand { get; }

    public ExperimentHistoryViewModel(IExperimentHistoryAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        _ = LoadAsync();
        _logger.Info(Resources.Strings.Log_ExperimentHistoryViewModel_Initialized);
    }

    public async Task LoadAsync()
    {
        _logger.Debug(Resources.Strings.Log_ExperimentHistory_LoadStart);
        Items.Clear();
        var list = await _svc.GetRecentAsync();
        foreach (var h in list) Items.Add(new HistoryItem(h.Time, h.Name, h.Result));
        _logger.Info(string.Format(Resources.Strings.Log_ExperimentHistory_LoadComplete, Items.Count));
    }

    public record HistoryItem(DateTime Time, string Name, string Result);
}

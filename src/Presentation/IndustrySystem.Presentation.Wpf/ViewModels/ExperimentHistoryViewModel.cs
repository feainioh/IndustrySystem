using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands; // added

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentHistoryViewModel
{
    private readonly IExperimentHistoryAppService _svc;
    public ObservableCollection<HistoryItem> Items { get; } = new();
    public ICommand RefreshCommand { get; }

    public ExperimentHistoryViewModel(IExperimentHistoryAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _svc.GetRecentAsync();
        foreach (var h in list) Items.Add(new HistoryItem(h.Time, h.Name, h.Result));
    }

    public record HistoryItem(DateTime Time, string Name, string Result);
}

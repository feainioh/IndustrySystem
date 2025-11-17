using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class AlarmViewModel
{
    private readonly IAlarmAppService _svc;
    public ObservableCollection<AlarmItem> Alarms { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand AckCommand { get; }

    public AlarmViewModel(IAlarmAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        AckCommand = new AsyncDelegateCommand<Guid>(AcknowledgeAsync);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Alarms.Clear();
        var list = await _svc.GetActiveAsync();
        foreach (var a in list) Alarms.Add(new AlarmItem(a.Id, a.Message, a.Time, a.Acknowledged));
    }

    public async Task AcknowledgeAsync(Guid id)
    {
        await _svc.AcknowledgeAsync(id);
        for (int i = 0; i < Alarms.Count; i++)
        {
            if (Alarms[i].Id == id)
            {
                Alarms[i] = Alarms[i] with { Acknowledged = true };
                break;
            }
        }
    }

    public record AlarmItem(Guid Id, string Message, DateTime Time, bool Acknowledged);
}

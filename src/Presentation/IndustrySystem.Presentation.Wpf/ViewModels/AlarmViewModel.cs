using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class AlarmViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
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
        _logger.Info(Resources.Strings.Log_AlarmViewModel_Initialized);
    }

    public async Task LoadAsync()
    {
        _logger.Debug(Resources.Strings.Log_Alarm_LoadStart);
        Alarms.Clear();
        var list = await _svc.GetActiveAsync();
        foreach (var a in list) Alarms.Add(new AlarmItem(a.Id, a.Message, a.Time, a.Acknowledged));
        _logger.Info(string.Format(Resources.Strings.Log_Alarm_LoadComplete, list.Count));
    }

    public async Task AcknowledgeAsync(Guid id)
    {
        _logger.Info(string.Format(Resources.Strings.Log_Alarm_Acknowledge, id));
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

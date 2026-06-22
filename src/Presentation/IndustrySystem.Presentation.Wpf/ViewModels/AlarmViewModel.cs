using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class AlarmViewModel : CrudViewModel<AlarmViewModel.AlarmItem>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IAlarmAppService _svc;
    public ObservableCollection<AlarmItem> Alarms => Items;
    public int TotalAlarms => Alarms.Count;
    public int UnacknowledgedAlarms => Alarms.Count(a => !a.Acknowledged);
    public int AcknowledgedAlarms => Alarms.Count(a => a.Acknowledged);
    public ICommand AckCommand { get; }

    public AlarmViewModel(IAlarmAppService svc)
    {
        _svc = svc;
        // Prism will probe CanExecute with null before binding settles; use nullable parameter to avoid NRE.
        AckCommand = new AsyncDelegateCommand<Guid?>(AcknowledgeAsync, id => id.HasValue);
        Alarms.CollectionChanged += (_, _) => RaiseAlarmSummaryChanged();
        _ = RefreshAsync();
        _logger.Info(Resources.Strings.Log_AlarmViewModel_Initialized);
    }

    protected override async Task<IReadOnlyList<AlarmItem>> LoadItemsAsync()
    {
        _logger.Debug(Resources.Strings.Log_Alarm_LoadStart);
        var list = await _svc.GetActiveAsync();
        if (list == null)
        {
            _logger.Warn("Alarm service returned null list.");
            return Array.Empty<AlarmItem>();
        }

        var alarms = list
            .Select(a => new AlarmItem(a.Id, a.Message, a.Time, a.Acknowledged))
            .ToList();

        _logger.Info(string.Format(Resources.Strings.Log_Alarm_LoadComplete, list.Count));
        return alarms;
    }

    public async Task AcknowledgeAsync(Guid? id)
    {
        if (!id.HasValue)
        {
            return;
        }

        _logger.Info(string.Format(Resources.Strings.Log_Alarm_Acknowledge, id.Value));
        await _svc.AcknowledgeAsync(id.Value);
        await RefreshAsync();
    }

    private void RaiseAlarmSummaryChanged()
    {
        RaisePropertyChanged(nameof(TotalAlarms));
        RaisePropertyChanged(nameof(UnacknowledgedAlarms));
        RaisePropertyChanged(nameof(AcknowledgedAlarms));
    }

    public record AlarmItem(Guid Id, string Message, DateTime Time, bool Acknowledged);
}

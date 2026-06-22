using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using Microsoft.Win32;
using NLog;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentHistoryViewModel : CrudViewModel<ExperimentHistoryDto>
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly IExperimentHistoryAppService _svc;

    private static string T(string key) => LocalizationProvider.Instance[key];
    private static readonly string AllStatusOption = T("History_Status_All");

    private DateTime _startDate = DateTime.Today.AddDays(-30);
    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    private DateTime _endDate = DateTime.Today;
    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            if (SetProperty(ref _endDate, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    private string _selectedStatus = AllStatusOption;
    public string SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (SetProperty(ref _selectedStatus, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    private RunModeOption _selectedRunMode;
    public RunModeOption SelectedRunMode
    {
        get => _selectedRunMode;
        set
        {
            if (SetProperty(ref _selectedRunMode, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    public ObservableCollection<string> StatusOptions { get; } = new();
    public ObservableCollection<RunModeOption> RunModeOptions { get; } = new();

    private int _completedCount;
    public int CompletedCount
    {
        get => _completedCount;
        private set => SetProperty(ref _completedCount, value);
    }

    private int _failedCount;
    public int FailedCount
    {
        get => _failedCount;
        private set => SetProperty(ref _failedCount, value);
    }

    private double _successRate;
    public double SuccessRate
    {
        get => _successRate;
        private set
        {
            if (SetProperty(ref _successRate, value))
            {
                RaisePropertyChanged(nameof(SuccessRateText));
            }
        }
    }

    public string SuccessRateText => $"{SuccessRate:P1}";
    public bool HasNoRecords => Items.Count == 0;

    public ICommand ClearFilterCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentHistoryViewModel(IExperimentHistoryAppService svc)
    {
        _svc = svc;

        StatusOptions.Add(AllStatusOption);
        StatusOptions.Add(T("Run_State_Idle"));
        StatusOptions.Add(T("Run_State_Running"));
        StatusOptions.Add(T("Run_State_Paused"));
        StatusOptions.Add(T("Run_State_Stopped"));
        StatusOptions.Add(T("Run_State_Completed"));

        RunModeOptions.Add(new RunModeOption("History_RunMode_All", null));
        RunModeOptions.Add(new RunModeOption("History_RunMode_Online", true));
        RunModeOptions.Add(new RunModeOption("History_RunMode_Offline", false));
        _selectedRunMode = RunModeOptions[0];

        ClearFilterCommand = new DelegateCommand(ClearFilters);
        ExportCommand = new DelegateCommand(ExportCsv);
        ViewDetailsCommand = new DelegateCommand<ExperimentHistoryDto>(ShowDetails);
        DeleteCommand = new AsyncDelegateCommand<ExperimentHistoryDto>(DeleteAsync);

        _ = RefreshAsync();
        Logger.Info(Resources.Strings.Log_ExperimentHistoryViewModel_Initialized);
    }

    protected override async Task<IReadOnlyList<ExperimentHistoryDto>> LoadItemsAsync()
    {
        Logger.Debug(Resources.Strings.Log_ExperimentHistory_LoadStart);
        var list = await _svc.GetListAsync();
        Logger.Info(string.Format(Resources.Strings.Log_ExperimentHistory_LoadComplete, list.Count));
        return list;
    }

    protected override Task ApplyPagingAsync(bool resetToFirstPage = false)
    {
        if (PageSize <= 0)
        {
            PageSize = 1;
        }

        var filtered = BuildFilteredRecords();
        UpdateStatistics(filtered);

        TotalCount = filtered.Count;

        if (resetToFirstPage)
        {
            PageIndex = 0;
        }

        var maxPageIndex = Math.Max(0, TotalPages - 1);
        if (PageIndex > maxPageIndex)
        {
            PageIndex = maxPageIndex;
        }

        Items.Clear();
        foreach (var record in filtered.Skip(PageIndex * PageSize).Take(PageSize))
        {
            Items.Add(record);
        }

        RaisePagingCommandStates();
        RaisePropertyChanged(nameof(HasNoRecords));
        return Task.CompletedTask;
    }

    private List<ExperimentHistoryDto> BuildFilteredRecords()
    {
        NormalizeDateRange(out var startDate, out var endDateExclusive);
        IEnumerable<ExperimentHistoryDto> query = _all;

        query = query.Where(x => x.StartTime >= startDate && x.StartTime < endDateExclusive);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var keyword = SearchText.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.ExperimentId.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.Operator.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || x.Result.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(SelectedStatus, AllStatusOption, StringComparison.Ordinal))
        {
            query = query.Where(x => string.Equals(GetStatusText(x.Status), SelectedStatus, StringComparison.Ordinal));
        }

        if (SelectedRunMode.Value is bool runMode)
        {
            query = query.Where(x => x.IsOnline == runMode);
        }

        return query
            .OrderByDescending(x => x.StartTime)
            .ToList();
    }

    private void NormalizeDateRange(out DateTime startDate, out DateTime endDateExclusive)
    {
        startDate = StartDate.Date;
        var endDate = EndDate.Date;

        if (endDate < startDate)
        {
            (startDate, endDate) = (endDate, startDate);
        }

        endDateExclusive = endDate.AddDays(1);
    }

    private void UpdateStatistics(IReadOnlyCollection<ExperimentHistoryDto> records)
    {
        var completed = records.Count(x => x.Status == RunState.Completed && string.Equals(x.Result, T("History_Result_Success"), StringComparison.Ordinal));
        var failed = records.Count(x => string.Equals(x.Result, T("History_Result_Failed"), StringComparison.Ordinal));

        CompletedCount = completed;
        FailedCount = failed;
        SuccessRate = records.Count == 0 ? 0 : completed / (double)records.Count;
    }

    private void ClearFilters()
    {
        StartDate = DateTime.Today.AddDays(-30);
        EndDate = DateTime.Today;
        SearchText = string.Empty;
        SelectedStatus = AllStatusOption;
        SelectedRunMode = RunModeOptions[0];
        OnSearchTextChanged();
    }

    private void ShowDetails(ExperimentHistoryDto? record)
    {
        if (record == null)
        {
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine($"{T("Field_ExperimentName")}: {record.Name}");
        builder.AppendLine($"{T("Field_ExperimentId")}: {record.ExperimentId}");
        builder.AppendLine($"{T("Col_Status")}: {GetStatusText(record.Status)}");
        builder.AppendLine($"{T("Col_Result")}: {record.Result}");
        builder.AppendLine($"{T("Col_RunMode")}: {GetRunModeText(record.IsOnline)}");
        builder.AppendLine($"{T("Col_StartTime")}: {record.StartTime:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"{T("Col_EndTime")}: {(record.EndTime.HasValue ? record.EndTime.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture) : "-")}");
        builder.AppendLine($"{T("Col_Duration")}: {FormatDuration(record.Duration)}");
        builder.AppendLine($"{T("Lbl_Operator")}: {record.Operator}");
        if (!string.IsNullOrWhiteSpace(record.ErrorMessage))
        {
            builder.AppendLine($"{T("Msg_ErrorTitle")}: {record.ErrorMessage}");
        }

        MessageBox.Show(builder.ToString(), T("History_Details_Title"), MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task DeleteAsync(ExperimentHistoryDto? record)
    {
        if (record == null)
        {
            return;
        }

        var confirmText = string.Format(CultureInfo.CurrentCulture, T("Msg_ConfirmDeleteHistoryRecord"), record.Name);
        var confirm = MessageBox.Show(confirmText, T("Msg_WarningTitle"), MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _svc.DeleteAsync(record.Id);
            await RefreshAsync();
            MessageBox.Show(T("Msg_DeleteHistorySuccess"), T("Msg_SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Delete history record failed. Id={0}", record.Id);
            var text = string.Format(CultureInfo.CurrentCulture, T("Msg_HistoryDeleteFailedFormat"), ex.Message);
            MessageBox.Show(text, T("Msg_ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportCsv()
    {
        var records = BuildFilteredRecords();
        if (records.Count == 0)
        {
            MessageBox.Show(T("Msg_HistoryNoDataToExport"), T("Msg_WarningTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"{T("History_Export_FileNamePrefix")}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            var csv = BuildCsv(records);
            File.WriteAllText(dialog.FileName, csv, new UTF8Encoding(true));

            var successText = string.Format(CultureInfo.CurrentCulture, T("Msg_HistoryExportSuccessFormat"), dialog.FileName);
            MessageBox.Show(successText, T("Msg_SuccessTitle"), MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Export history csv failed.");
            var text = string.Format(CultureInfo.CurrentCulture, T("Msg_HistoryExportFailedFormat"), ex.Message);
            MessageBox.Show(text, T("Msg_ErrorTitle"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string BuildCsv(IEnumerable<ExperimentHistoryDto> records)
    {
        var lines = new List<string>
        {
            string.Join(",",
                EscapeCsv(T("Field_ExperimentName")),
                EscapeCsv(T("Field_ExperimentId")),
                EscapeCsv(T("Col_Status")),
                EscapeCsv(T("Col_Result")),
                EscapeCsv(T("Col_RunMode")),
                EscapeCsv(T("Col_StartTime")),
                EscapeCsv(T("Col_EndTime")),
                EscapeCsv(T("Col_Duration")),
                EscapeCsv(T("Lbl_Operator")),
                EscapeCsv(T("Msg_ErrorTitle")))
        };

        foreach (var record in records)
        {
            lines.Add(string.Join(",",
                EscapeCsv(record.Name),
                EscapeCsv(record.ExperimentId),
                EscapeCsv(GetStatusText(record.Status)),
                EscapeCsv(record.Result),
                EscapeCsv(GetRunModeText(record.IsOnline)),
                EscapeCsv(record.StartTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                EscapeCsv(record.EndTime?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty),
                EscapeCsv(FormatDuration(record.Duration)),
                EscapeCsv(record.Operator),
                EscapeCsv(record.ErrorMessage ?? string.Empty)));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string EscapeCsv(string value)
    {
        var safe = value.Replace("\"", "\"\"");
        return $"\"{safe}\"";
    }

    private static string GetRunModeText(bool isOnline)
    {
        return isOnline ? T("History_RunMode_Online") : T("History_RunMode_Offline");
    }

    private static string FormatDuration(TimeSpan? duration)
    {
        if (!duration.HasValue)
        {
            return "-";
        }

        var value = duration.Value;
        return value.TotalHours >= 1
            ? value.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)
            : value.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }

    private static string GetStatusText(RunState status)
    {
        return status switch
        {
            RunState.Running => T("Run_State_Running"),
            RunState.Paused => T("Run_State_Paused"),
            RunState.Stopped => T("Run_State_Stopped"),
            RunState.Completed => T("Run_State_Completed"),
            _ => T("Run_State_Idle")
        };
    }

    public sealed class RunModeOption : BaseViewModel
    {
        public RunModeOption(string labelKey, bool? value)
        {
            LabelKey = labelKey;
            Value = value;
        }

        public string LabelKey { get; }
        public bool? Value { get; }
        public string DisplayName => LocalizationProvider.Instance[LabelKey];
    }
}

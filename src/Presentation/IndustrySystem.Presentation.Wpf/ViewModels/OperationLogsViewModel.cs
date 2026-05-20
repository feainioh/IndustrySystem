using IndustrySystem.Application.Contracts.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
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

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class OperationLogsViewModel : NagetiveViewModel
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private static readonly string AllTypesOption = Resources.Strings.Hint_AllTypes;
    private static readonly string AllLevelsOption = Resources.Strings.Hint_AllLevels;

    private readonly IOperationLogService _operationLogService;
    private readonly List<OperationLog> _allLogs = new();
    private readonly List<OperationLog> _filteredLogs = new();

    private readonly DelegateCommand _firstPageCommand;
    private readonly DelegateCommand _previousPageCommand;
    private readonly DelegateCommand _nextPageCommand;
    private readonly DelegateCommand _lastPageCommand;

    public ObservableCollection<OperationLog> Logs { get; } = new();

    private DateTime? _startDate = DateTime.Today.AddDays(-7);
    public DateTime? StartDate
    {
        get => _startDate;
        set => SetProperty(ref _startDate, value);
    }

    private DateTime? _endDate = DateTime.Today;
    public DateTime? EndDate
    {
        get => _endDate;
        set => SetProperty(ref _endDate, value);
    }

    private string _operatorFilter = string.Empty;
    public string OperatorFilter
    {
        get => _operatorFilter;
        set => SetProperty(ref _operatorFilter, value);
    }

    private string? _selectedOperationType = AllTypesOption;
    public string? SelectedOperationType
    {
        get => _selectedOperationType;
        set => SetProperty(ref _selectedOperationType, value);
    }

    private string? _selectedLogLevel = AllLevelsOption;
    public string? SelectedLogLevel
    {
        get => _selectedLogLevel;
        set => SetProperty(ref _selectedLogLevel, value);
    }

    public ObservableCollection<string> OperationTypes { get; } = new();

    public ObservableCollection<string> LogLevels { get; } = new();

    public ObservableCollection<int> PageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _currentPage = 1;
    public int CurrentPage
    {
        get => _currentPage;
        set => SetProperty(ref _currentPage, value);
    }

    private int _totalPages = 1;
    public int TotalPages
    {
        get => _totalPages;
        set => SetProperty(ref _totalPages, value);
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        set => SetProperty(ref _totalCount, value);
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            var normalized = value <= 0 ? 20 : value;
            if (SetProperty(ref _pageSize, normalized))
            {
                CurrentPage = 1;
                UpdatePagedLogs();
            }
        }
    }

    public bool HasNoLogs => Logs.Count == 0;

    public ICommand SearchCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand FirstPageCommand => _firstPageCommand;
    public ICommand PreviousPageCommand => _previousPageCommand;
    public ICommand NextPageCommand => _nextPageCommand;
    public ICommand LastPageCommand => _lastPageCommand;

    public OperationLogsViewModel(IOperationLogService operationLogService)
    {
        _operationLogService = operationLogService;
        InitializeFilterOptions();

        SearchCommand = new DelegateCommand(OnSearch);
        ExportCommand = new DelegateCommand(Export);
        ViewDetailsCommand = new DelegateCommand<OperationLog?>(ViewDetails);

        _firstPageCommand = new DelegateCommand(() => ChangePage(1), () => CurrentPage > 1)
            .ObservesProperty(() => CurrentPage);
        _previousPageCommand = new DelegateCommand(() => ChangePage(CurrentPage - 1), () => CurrentPage > 1)
            .ObservesProperty(() => CurrentPage);
        _nextPageCommand = new DelegateCommand(() => ChangePage(CurrentPage + 1), () => CurrentPage < TotalPages)
            .ObservesProperty(() => CurrentPage)
            .ObservesProperty(() => TotalPages);
        _lastPageCommand = new DelegateCommand(() => ChangePage(TotalPages), () => CurrentPage < TotalPages)
            .ObservesProperty(() => CurrentPage)
            .ObservesProperty(() => TotalPages);

        RefreshCommand.Execute(null);
    }

    protected override async Task OnRefreshAsync()
    {
        await ReloadLogsAsync();
        CurrentPage = 1;
        UpdatePagedLogs();
    }

    private async Task ReloadLogsAsync()
    {
        try
        {
            var dtos = await _operationLogService.GetListAsync(StartDate, EndDate);
            _allLogs.Clear();
            _allLogs.AddRange(dtos.Select(d => new OperationLog
            {
                Id = d.Id,
                Timestamp = d.Timestamp,
                Level = d.Level,
                OperationType = d.OperationType,
                Operator = d.Operator,
                Description = d.Description,
                IPAddress = d.IPAddress,
                Logger = d.Logger
            }).OrderByDescending(x => x.Timestamp));

            UpdateFilterOptionsFromData();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load operation logs from database.");
        }
    }

    private void OnSearch()
    {
        CurrentPage = 1;
        UpdatePagedLogs();
    }

    private void ChangePage(int page)
    {
        if (TotalPages <= 0) return;

        var bounded = Math.Max(1, Math.Min(page, TotalPages));
        if (bounded == CurrentPage) return;

        CurrentPage = bounded;
        UpdatePagedLogs();
    }

    private void UpdatePagedLogs()
    {
        ApplyFilters();

        TotalCount = _filteredLogs.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        Logs.Clear();
        var skip = (CurrentPage - 1) * PageSize;
        foreach (var log in _filteredLogs.Skip(skip).Take(PageSize))
        {
            Logs.Add(log);
        }

        RaisePropertyChanged(nameof(HasNoLogs));
    }

    private void ApplyFilters()
    {
        IEnumerable<OperationLog> query = _allLogs;

        if (!string.IsNullOrWhiteSpace(SelectedOperationType) && !string.Equals(SelectedOperationType, AllTypesOption, StringComparison.Ordinal))
        {
            query = query.Where(x => string.Equals(x.OperationType, SelectedOperationType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(SelectedLogLevel) && !string.Equals(SelectedLogLevel, AllLevelsOption, StringComparison.Ordinal))
        {
            query = query.Where(x => string.Equals(x.Level, SelectedLogLevel, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(OperatorFilter))
        {
            var filter = OperatorFilter.Trim();
            query = query.Where(x =>
                x.Operator.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                x.Logger.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        _filteredLogs.Clear();
        _filteredLogs.AddRange(query.OrderByDescending(x => x.Timestamp));
    }

    private void Export()
    {
        if (_filteredLogs.Count == 0)
        {
            MessageBox.Show(Resources.Strings.Msg_NoLogsAvailable, Resources.Strings.Msg_WarningTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv|Text (*.txt)|*.txt",
            FileName = $"operation-logs-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var csv = BuildCsv(_filteredLogs);
            File.WriteAllText(dialog.FileName, csv, new UTF8Encoding(true));
            MessageBox.Show($"{Resources.Strings.Btn_Export}{Resources.Strings.Msg_SuccessTitle}{Environment.NewLine}{dialog.FileName}", Resources.Strings.Msg_SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to export operation logs.");
            MessageBox.Show($"{Resources.Strings.Btn_Export}{Resources.Strings.Msg_ErrorTitle}{Environment.NewLine}{ex.Message}", Resources.Strings.Msg_ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ViewDetails(OperationLog? log)
    {
        if (log == null) return;

        var detail = new StringBuilder();
        detail.AppendLine($"{Resources.Strings.Col_Timestamp}: {log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}");
        detail.AppendLine($"{Resources.Strings.Col_Level}: {log.Level}");
        detail.AppendLine($"{Resources.Strings.Col_OperationType}: {log.OperationType}");
        detail.AppendLine($"{Resources.Strings.Lbl_Operator}: {log.Operator}");
        detail.AppendLine($"{Resources.Strings.Col_IPAddress}: {log.IPAddress}");
        detail.AppendLine($"Logger: {log.Logger}");
        detail.AppendLine();
        detail.AppendLine($"{Resources.Strings.Col_OperationDesc}:");
        detail.AppendLine(log.Description);

        MessageBox.Show(detail.ToString(), Resources.Strings.Btn_Details, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static string BuildCsv(IEnumerable<OperationLog> logs)
    {
        var lines = new List<string>
        {
            string.Join(",",
                EscapeCsv(Resources.Strings.Col_Timestamp),
                EscapeCsv(Resources.Strings.Col_Level),
                EscapeCsv(Resources.Strings.Col_OperationType),
                EscapeCsv(Resources.Strings.Lbl_Operator),
                EscapeCsv(Resources.Strings.Col_IPAddress),
                EscapeCsv("Logger"),
                EscapeCsv(Resources.Strings.Col_OperationDesc))
        };

        foreach (var log in logs)
        {
            lines.Add(string.Join(",",
                EscapeCsv(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)),
                EscapeCsv(log.Level),
                EscapeCsv(log.OperationType),
                EscapeCsv(log.Operator),
                EscapeCsv(log.IPAddress),
                EscapeCsv(log.Logger),
                EscapeCsv(log.Description)));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string EscapeCsv(string value)
    {
        var safe = value.Replace("\"", "\"\"");
        return $"\"{safe}\"";
    }

    private void InitializeFilterOptions()
    {
        OperationTypes.Clear();
        OperationTypes.Add(AllTypesOption);
        SelectedOperationType = AllTypesOption;

        LogLevels.Clear();
        LogLevels.Add(AllLevelsOption);
        LogLevels.Add("Info");
        LogLevels.Add("Warning");
        LogLevels.Add("Error");
        LogLevels.Add("Debug");
        LogLevels.Add("Trace");
        LogLevels.Add("Fatal");
        SelectedLogLevel = AllLevelsOption;
    }

    private void UpdateFilterOptionsFromData()
    {
        var currentType = SelectedOperationType;
        var currentLevel = SelectedLogLevel;

        var opTypes = _allLogs
            .Select(x => x.OperationType)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        OperationTypes.Clear();
        OperationTypes.Add(AllTypesOption);
        foreach (var type in opTypes)
        {
            OperationTypes.Add(type);
        }

        var levels = _allLogs
            .Select(x => x.Level)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        LogLevels.Clear();
        LogLevels.Add(AllLevelsOption);
        foreach (var level in levels)
        {
            LogLevels.Add(level);
        }

        SelectedOperationType = !string.IsNullOrWhiteSpace(currentType) && OperationTypes.Contains(currentType)
            ? currentType
            : AllTypesOption;

        SelectedLogLevel = !string.IsNullOrWhiteSpace(currentLevel) && LogLevels.Contains(currentLevel)
            ? currentLevel
            : AllLevelsOption;
    }
}

public class OperationLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "Info";
    public string OperationType { get; set; } = "System";
    public string Operator { get; set; } = "system";
    public string Description { get; set; } = string.Empty;
    public string IPAddress { get; set; } = "-";
    public string Logger { get; set; } = string.Empty;
}

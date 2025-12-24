using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class OperationLogsViewModel : BindableBase
{
    #region Properties
    
    private ObservableCollection<OperationLog> _logs = new();
    public ObservableCollection<OperationLog> Logs
    {
        get => _logs;
        set => SetProperty(ref _logs, value);
    }

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

    private string? _selectedOperationType;
    public string? SelectedOperationType
    {
        get => _selectedOperationType;
        set => SetProperty(ref _selectedOperationType, value);
    }

    private string? _selectedLogLevel;
    public string? SelectedLogLevel
    {
        get => _selectedLogLevel;
        set => SetProperty(ref _selectedLogLevel, value);
    }

    public ObservableCollection<string> OperationTypes { get; } = new()
    {
        "全部", "登录", "登出", "创建", "修改", "删除", "查询", "导出", "导入"
    };

    public ObservableCollection<string> LogLevels { get; } = new()
    {
        "全部", "Info", "Warning", "Error", "Success"
    };

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
            if (SetProperty(ref _pageSize, value))
            {
                LoadLogs();
            }
        }
    }

    public bool HasNoLogs => Logs.Count == 0;

    #endregion

    #region Commands

    public ICommand RefreshCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }

    #endregion

    public OperationLogsViewModel()
    {
        RefreshCommand = new DelegateCommand(LoadLogs);
        SearchCommand = new DelegateCommand(Search);
        ExportCommand = new DelegateCommand(Export);
        ViewDetailsCommand = new DelegateCommand<OperationLog>(ViewDetails);
        FirstPageCommand = new DelegateCommand(GoToFirstPage, CanGoToFirstPage).ObservesProperty(() => CurrentPage);
        PreviousPageCommand = new DelegateCommand(GoToPreviousPage, CanGoToPreviousPage).ObservesProperty(() => CurrentPage);
        NextPageCommand = new DelegateCommand(GoToNextPage, CanGoToNextPage).ObservesProperty(() => CurrentPage).ObservesProperty(() => TotalPages);
        LastPageCommand = new DelegateCommand(GoToLastPage, CanGoToLastPage).ObservesProperty(() => CurrentPage).ObservesProperty(() => TotalPages);

        // 初始化示例数据
        LoadLogs();
    }

    private void LoadLogs()
    {
        // TODO: 从数据库或服务加载真实数据
        // 这里使用示例数据
        var sampleLogs = GenerateSampleLogs();
        
        TotalCount = sampleLogs.Count;
        TotalPages = (int)Math.Ceiling((double)TotalCount / PageSize);
        
        var pagedLogs = sampleLogs
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
        
        Logs = new ObservableCollection<OperationLog>(pagedLogs);
        RaisePropertyChanged(nameof(HasNoLogs));
    }

    private void Search()
    {
        CurrentPage = 1;
        LoadLogs();
    }

    private void Export()
    {
        // TODO: 实现导出功能
        System.Windows.MessageBox.Show("导出功能开发中...", "提示");
    }

    private void ViewDetails(OperationLog? log)
    {
        if (log == null) return;
        // TODO: 显示详情对话框
        System.Windows.MessageBox.Show($"操作详情:\n\n{log.Description}\n\n时间: {log.Timestamp}\n操作人: {log.Operator}", "操作日志详情");
    }

    #region Pagination

    private void GoToFirstPage()
    {
        CurrentPage = 1;
        LoadLogs();
    }

    private bool CanGoToFirstPage() => CurrentPage > 1;

    private void GoToPreviousPage()
    {
        CurrentPage--;
        LoadLogs();
    }

    private bool CanGoToPreviousPage() => CurrentPage > 1;

    private void GoToNextPage()
    {
        CurrentPage++;
        LoadLogs();
    }

    private bool CanGoToNextPage() => CurrentPage < TotalPages;

    private void GoToLastPage()
    {
        CurrentPage = TotalPages;
        LoadLogs();
    }

    private bool CanGoToLastPage() => CurrentPage < TotalPages;

    #endregion

    #region Sample Data

    private System.Collections.Generic.List<OperationLog> GenerateSampleLogs()
    {
        var logs = new System.Collections.Generic.List<OperationLog>();
        var random = new Random();
        var operators = new[] { "admin", "user001", "user002", "operator1", "operator2" };
        var operations = new[] { "登录", "登出", "创建用户", "修改角色", "删除权限", "查询数据", "导出报表", "运行实验", "修改配置" };
        var levels = new[] { "Info", "Warning", "Error", "Success" };
        var ips = new[] { "192.168.1.100", "192.168.1.101", "192.168.1.102", "10.0.0.50" };

        for (int i = 0; i < 86; i++)
        {
            logs.Add(new OperationLog
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now.AddHours(-random.Next(0, 168)),
                Level = levels[random.Next(levels.Length)],
                OperationType = operations[random.Next(operations.Length)],
                Operator = operators[random.Next(operators.Length)],
                Description = $"执行了 {operations[random.Next(operations.Length)]} 操作，涉及模块: {new[] { "用户管理", "实验管理", "设备管理", "数据管理" }[random.Next(4)]}",
                IPAddress = ips[random.Next(ips.Length)]
            });
        }

        return logs.OrderByDescending(x => x.Timestamp).ToList();
    }

    #endregion
}

/// <summary>
/// 操作日志模型
/// </summary>
public class OperationLog
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "Info";
    public string OperationType { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IPAddress { get; set; } = string.Empty;
}

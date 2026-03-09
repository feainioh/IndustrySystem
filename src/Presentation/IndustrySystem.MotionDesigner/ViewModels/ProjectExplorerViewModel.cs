using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.MotionDesigner.Models;
using IndustrySystem.MotionDesigner.Services;
using Microsoft.Win32;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// 项目管理 ViewModel
/// 管理项目-子项目-子程序的层级结构
/// </summary>
public class ProjectExplorerViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IProjectService _projectService;
    private readonly IEventAggregator _eventAggregator;

    private MotionProject? _currentProject;
    private object? _selectedItem;
    private SubProgram? _activeSubProgram;
    private string _statusMessage = "就绪";

    #region Properties

    /// <summary>
    /// 当前项目
    /// </summary>
    public MotionProject? CurrentProject
    {
        get => _currentProject;
        set
        {
            if (SetProperty(ref _currentProject, value))
            {
                RaisePropertyChanged(nameof(HasProject));
                RaisePropertyChanged(nameof(ProjectTitle));
            }
        }
    }

    /// <summary>
    /// 是否有打开的项目
    /// </summary>
    public bool HasProject => CurrentProject != null;

    /// <summary>
    /// 项目标题
    /// </summary>
    public string ProjectTitle => CurrentProject != null
        ? $"{CurrentProject.Name}{(CurrentProject.IsModified ? " *" : "")}"
        : "无项目";

    /// <summary>
    /// 选中的项（项目/子项目/子程序）
    /// </summary>
    public object? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                RaisePropertyChanged(nameof(CanAddSubProject));
                RaisePropertyChanged(nameof(CanAddSubProgram));
                RaisePropertyChanged(nameof(CanDeleteItem));
                RaisePropertyChanged(nameof(CanRenameItem));

                // 如果选中了子程序，发布事件
                if (value is SubProgram subProgram)
                {
                    _eventAggregator.GetEvent<SubProgramSelectedEvent>().Publish(subProgram);
                }
            }
        }
    }

    /// <summary>
    /// 当前活动的子程序
    /// </summary>
    public SubProgram? ActiveSubProgram
    {
        get => _activeSubProgram;
        set => SetProperty(ref _activeSubProgram, value);
    }

    /// <summary>
    /// 打开的子程序标签页
    /// </summary>
    public ObservableCollection<SubProgram> OpenSubPrograms { get; } = [];

    /// <summary>
    /// 状态消息
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// 可以添加子项目
    /// </summary>
    public bool CanAddSubProject => CurrentProject != null;

    /// <summary>
    /// 可以添加子程序
    /// </summary>
    public bool CanAddSubProgram => SelectedItem is SubProject;

    /// <summary>
    /// 可以删除
    /// </summary>
    public bool CanDeleteItem => SelectedItem is SubProject or SubProgram;

    /// <summary>
    /// 可以重命名
    /// </summary>
    public bool CanRenameItem => SelectedItem != null;

    #endregion

    #region Commands

    public ICommand NewProjectCommand { get; }
    public ICommand OpenProjectCommand { get; }
    public ICommand SaveProjectCommand { get; }
    public ICommand SaveProjectAsCommand { get; }
    public ICommand CloseProjectCommand { get; }

    public ICommand AddSubProjectCommand { get; }
    public ICommand AddSubProgramCommand { get; }
    public ICommand DeleteItemCommand { get; }
    public ICommand RenameItemCommand { get; }
    public ICommand DuplicateSubProgramCommand { get; }

    public ICommand OpenSubProgramCommand { get; }
    public ICommand CloseSubProgramCommand { get; }
    public ICommand ExportSubProgramCommand { get; }
    public ICommand ImportSubProgramCommand { get; }

    #endregion

    #region Constructor

    public ProjectExplorerViewModel(IProjectService projectService, IEventAggregator eventAggregator)
    {
        _projectService = projectService;
        _eventAggregator = eventAggregator;

        // 项目命令
        NewProjectCommand = new DelegateCommand(NewProject);
        OpenProjectCommand = new DelegateCommand(async () => await OpenProjectAsync());
        SaveProjectCommand = new DelegateCommand(async () => await SaveProjectAsync(), () => HasProject)
            .ObservesProperty(() => HasProject);
        SaveProjectAsCommand = new DelegateCommand(async () => await SaveProjectAsAsync(), () => HasProject)
            .ObservesProperty(() => HasProject);
        CloseProjectCommand = new DelegateCommand(CloseProject, () => HasProject)
            .ObservesProperty(() => HasProject);

        // 结构操作命令
        AddSubProjectCommand = new DelegateCommand(AddSubProject, () => CanAddSubProject)
            .ObservesProperty(() => CanAddSubProject);
        AddSubProgramCommand = new DelegateCommand(AddSubProgram, () => CanAddSubProgram)
            .ObservesProperty(() => CanAddSubProgram);
        DeleteItemCommand = new DelegateCommand(DeleteItem, () => CanDeleteItem)
            .ObservesProperty(() => CanDeleteItem);
        RenameItemCommand = new DelegateCommand(RenameItem, () => CanRenameItem)
            .ObservesProperty(() => CanRenameItem);
        DuplicateSubProgramCommand = new DelegateCommand(DuplicateSubProgram, () => SelectedItem is SubProgram)
            .ObservesProperty(() => SelectedItem);

        // 子程序操作命令
        OpenSubProgramCommand = new DelegateCommand<SubProgram>(OpenSubProgram);
        CloseSubProgramCommand = new DelegateCommand<SubProgram>(CloseSubProgram);
        ExportSubProgramCommand = new DelegateCommand(async () => await ExportSubProgramAsync(), () => SelectedItem is SubProgram)
            .ObservesProperty(() => SelectedItem);
        ImportSubProgramCommand = new DelegateCommand(async () => await ImportSubProgramAsync(), () => CanAddSubProgram)
            .ObservesProperty(() => CanAddSubProgram);

        // 订阅事件
        _eventAggregator.GetEvent<SubProgramModifiedEvent>().Subscribe(OnSubProgramModified);
    }

    #endregion

    #region Project Operations

    private void NewProject()
    {
        if (CurrentProject?.IsModified == true)
        {
            var result = System.Windows.MessageBox.Show(
                "当前项目有未保存的更改，是否保存？",
                "保存项目",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Cancel) return;
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _ = SaveProjectAsync();
            }
        }

        CurrentProject = _projectService.CreateProject("新项目");
        OpenSubPrograms.Clear();
        StatusMessage = "已创建新项目";
        _logger.Info("Created new project");
    }

    private async Task OpenProjectAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "运动程序项目 (*.mproj)|*.mproj|所有文件 (*.*)|*.*",
            Title = "打开项目"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            CurrentProject = await _projectService.OpenProjectAsync(dialog.FileName);
            OpenSubPrograms.Clear();

            // 自动打开主程序
            var mainProgram = CurrentProject.SubProjects
                .SelectMany(sp => sp.SubPrograms)
                .FirstOrDefault(p => p.ProgramType == SubProgramType.Main);

            if (mainProgram != null)
                OpenSubProgram(mainProgram);

            StatusMessage = $"已打开项目: {CurrentProject.Name}";
            _logger.Info($"Opened project: {CurrentProject.Name}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to open project");
            System.Windows.MessageBox.Show($"打开项目失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task SaveProjectAsync()
    {
        if (CurrentProject == null) return;

        if (string.IsNullOrEmpty(CurrentProject.FilePath))
        {
            await SaveProjectAsAsync();
            return;
        }

        try
        {
            await _projectService.SaveProjectAsync(CurrentProject);
            RaisePropertyChanged(nameof(ProjectTitle));
            StatusMessage = $"项目已保存: {CurrentProject.Name}";
            _logger.Info($"Saved project: {CurrentProject.Name}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save project");
            System.Windows.MessageBox.Show($"保存项目失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task SaveProjectAsAsync()
    {
        if (CurrentProject == null) return;

        var dialog = new SaveFileDialog
        {
            Filter = "运动程序项目 (*.mproj)|*.mproj",
            Title = "保存项目",
            FileName = CurrentProject.Name
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await _projectService.SaveProjectAsync(CurrentProject, dialog.FileName);
            RaisePropertyChanged(nameof(ProjectTitle));
            StatusMessage = $"项目已保存: {CurrentProject.Name}";
            _logger.Info($"Saved project as: {dialog.FileName}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save project");
            System.Windows.MessageBox.Show($"保存项目失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private void CloseProject()
    {
        if (CurrentProject?.IsModified == true)
        {
            var result = System.Windows.MessageBox.Show(
                "当前项目有未保存的更改，是否保存？",
                "保存项目",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Cancel) return;
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _ = SaveProjectAsync();
            }
        }

        _projectService.CloseProject();
        CurrentProject = null;
        OpenSubPrograms.Clear();
        ActiveSubProgram = null;
        StatusMessage = "项目已关闭";
        _logger.Info("Project closed");
    }

    #endregion

    #region Structure Operations

    private void AddSubProject()
    {
        if (CurrentProject == null) return;

        var name = $"子项目{CurrentProject.SubProjects.Count + 1}";
        var subProject = _projectService.AddSubProject(CurrentProject, name);
        SelectedItem = subProject;
        StatusMessage = $"已添加子项目: {name}";
    }

    private void AddSubProgram()
    {
        if (SelectedItem is not SubProject subProject) return;

        var name = $"子程序{subProject.SubPrograms.Count + 1}";
        var subProgram = _projectService.AddSubProgram(subProject, name);
        SelectedItem = subProgram;
        OpenSubProgram(subProgram);
        StatusMessage = $"已添加子程序: {name}";
    }

    private void DeleteItem()
    {
        if (CurrentProject == null) return;

        switch (SelectedItem)
        {
            case SubProject subProject:
                var confirmSp = System.Windows.MessageBox.Show(
                    $"确定要删除子项目 '{subProject.Name}' 及其所有子程序吗？",
                    "确认删除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (confirmSp == System.Windows.MessageBoxResult.Yes)
                {
                    // 关闭所有打开的子程序
                    foreach (var sp in subProject.SubPrograms.ToList())
                        CloseSubProgram(sp);

                    _projectService.RemoveSubProject(CurrentProject, subProject);
                    SelectedItem = null;
                    StatusMessage = $"已删除子项目: {subProject.Name}";
                }
                break;

            case SubProgram subProgram:
                var parent = CurrentProject.SubProjects
                    .FirstOrDefault(sp => sp.SubPrograms.Contains(subProgram));

                if (parent != null)
                {
                    var confirmProg = System.Windows.MessageBox.Show(
                        $"确定要删除子程序 '{subProgram.Name}' 吗？",
                        "确认删除",
                        System.Windows.MessageBoxButton.YesNo,
                        System.Windows.MessageBoxImage.Warning);

                    if (confirmProg == System.Windows.MessageBoxResult.Yes)
                    {
                        CloseSubProgram(subProgram);
                        _projectService.RemoveSubProgram(parent, subProgram);
                        SelectedItem = null;
                        StatusMessage = $"已删除子程序: {subProgram.Name}";
                    }
                }
                break;
        }
    }

    private void RenameItem()
    {
        // 通过输入对话框实现重命名
        // TODO: 实现重命名对话框
        StatusMessage = "重命名功能待实现";
    }

    private void DuplicateSubProgram()
    {
        if (CurrentProject == null || SelectedItem is not SubProgram source) return;

        var parent = CurrentProject.SubProjects
            .FirstOrDefault(sp => sp.SubPrograms.Contains(source));

        if (parent != null)
        {
            var copy = _projectService.DuplicateSubProgram(parent, source);
            SelectedItem = copy;
            OpenSubProgram(copy);
            StatusMessage = $"已复制子程序: {copy.Name}";
        }
    }

    #endregion

    #region SubProgram Operations

    private void OpenSubProgram(SubProgram? subProgram)
    {
        if (subProgram == null) return;

        if (!OpenSubPrograms.Contains(subProgram))
        {
            OpenSubPrograms.Add(subProgram);
            subProgram.IsOpen = true;
        }

        ActiveSubProgram = subProgram;
        _eventAggregator.GetEvent<SubProgramActivatedEvent>().Publish(subProgram);
        StatusMessage = $"已打开子程序: {subProgram.Name}";
    }

    private void CloseSubProgram(SubProgram? subProgram)
    {
        if (subProgram == null) return;

        if (subProgram.IsModified)
        {
            var result = System.Windows.MessageBox.Show(
                $"子程序 '{subProgram.Name}' 有未保存的更改，是否保存？",
                "保存更改",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Cancel) return;
            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 保存更改（已经保存在项目中）
                subProgram.IsModified = false;
            }
        }

        OpenSubPrograms.Remove(subProgram);
        subProgram.IsOpen = false;

        if (ActiveSubProgram == subProgram)
        {
            ActiveSubProgram = OpenSubPrograms.LastOrDefault();
            if (ActiveSubProgram != null)
                _eventAggregator.GetEvent<SubProgramActivatedEvent>().Publish(ActiveSubProgram);
        }

        StatusMessage = $"已关闭子程序: {subProgram.Name}";
    }

    private async Task ExportSubProgramAsync()
    {
        if (SelectedItem is not SubProgram subProgram) return;

        var dialog = new SaveFileDialog
        {
            Filter = "子程序文件 (*.subprog)|*.subprog",
            Title = "导出子程序",
            FileName = subProgram.Name
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            await _projectService.ExportSubProgramAsync(subProgram, dialog.FileName);
            StatusMessage = $"子程序已导出: {subProgram.Name}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to export sub-program");
            System.Windows.MessageBox.Show($"导出失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task ImportSubProgramAsync()
    {
        if (SelectedItem is not SubProject subProject) return;

        var dialog = new OpenFileDialog
        {
            Filter = "子程序文件 (*.subprog)|*.subprog",
            Title = "导入子程序"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var imported = await _projectService.ImportSubProgramAsync(dialog.FileName);
            subProject.SubPrograms.Add(imported);
            SelectedItem = imported;
            OpenSubProgram(imported);
            StatusMessage = $"子程序已导入: {imported.Name}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to import sub-program");
            System.Windows.MessageBox.Show($"导入失败: {ex.Message}", "错误",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    #endregion

    #region Event Handlers

    private void OnSubProgramModified(SubProgram subProgram)
    {
        subProgram.IsModified = true;
        if (CurrentProject != null)
        {
            CurrentProject.IsModified = true;
            RaisePropertyChanged(nameof(ProjectTitle));
        }
    }

    #endregion
}

#region Events

/// <summary>
/// 子程序选中事件
/// </summary>
public class SubProgramSelectedEvent : PubSubEvent<SubProgram> { }

/// <summary>
/// 子程序激活事件（在设计器中打开）
/// </summary>
public class SubProgramActivatedEvent : PubSubEvent<SubProgram> { }

/// <summary>
/// 子程序修改事件
/// </summary>
public class SubProgramModifiedEvent : PubSubEvent<SubProgram> { }

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class PermissionsViewModel : NagetiveCurdVeiwModel<PermissionDto>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IPermissionAppService _svc;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// 新增权限名称输入。
    /// </summary>
    private string _newName = string.Empty;
    public string NewName { get => _newName; set => SetProperty(ref _newName, value); }

    /// <summary>
    /// 新增权限显示名输入。
    /// </summary>
    private string _newDisplayName = string.Empty;
    public string NewDisplayName { get => _newDisplayName; set => SetProperty(ref _newDisplayName, value); }

    /// <summary>
    /// 新增权限分组输入。
    /// </summary>
    private string _newGroupName = string.Empty;
    public string NewGroupName { get => _newGroupName; set => SetProperty(ref _newGroupName, value); }

    /// <summary>
    /// 列表搜索关键字。
    /// </summary>
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                PermissionsView.Refresh();
            }
        }
    }

    /// <summary>
    /// 原始权限集合。
    /// </summary>
    public ObservableCollection<PermissionDto> Permissions { get; } = new();

    /// <summary>
    /// 过滤后的权限视图（用于搜索）。
    /// </summary>
    public ICollectionView PermissionsView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public PermissionsViewModel(IPermissionAppService svc, IDialogService dialogService)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _dialogService = dialogService;
        PermissionsView = CollectionViewSource.GetDefaultView(Permissions);
        PermissionsView.Filter = FilterPermissions;

        // 页面命令：刷新、新增、编辑、删除
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(async () => await AddCurrentAsync());
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenPermissionDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

        _logger.Info("PermissionsViewModel initialized");

        // 视图初始化后延迟加载，避免首屏阻塞。
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(100);
            await LoadAsync();
        });
    }

    /// <summary>
    /// 列表过滤逻辑：按名称、显示名、分组模糊匹配。
    /// </summary>
    private bool FilterPermissions(object item)
    {
        if (item is not PermissionDto permission) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var key = SearchText.Trim();
        return (permission.Name?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
               || (permission.DisplayName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
               || (permission.GroupName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// 执行新增并清空输入框。
    /// </summary>
    private async Task AddCurrentAsync()
    {
        await AddAsync(NewName, NewDisplayName, NewGroupName);
        NewName = string.Empty;
        NewDisplayName = string.Empty;
        NewGroupName = string.Empty;
    }

    /// <summary>
    /// 打开权限编辑弹窗，保存成功后刷新列表。
    /// </summary>
    private void OpenPermissionDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", (Guid?)id } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.PermissionEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                await LoadAsync();
            }
        });
    }

    /// <summary>
    /// 加载权限列表并更新UI集合。
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            _logger.Debug("Loading permissions...");

            var permissions = await _svc.GetListAsync();
            _logger.Info($"Loaded {permissions?.Count ?? 0} permissions from database");

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Permissions.Clear();
                if (permissions != null)
                {
                    foreach (var p in permissions)
                    {
                        Permissions.Add(p);
                    }
                }
            });

            _logger.Info($"UI updated with {Permissions.Count} permissions");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load permissions");
            MessageBox.Show($"{Strings.Msg_LoadFailed}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 新增权限。
    /// </summary>
    public async Task AddAsync(string name, string displayName, string groupName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(Strings.Msg_ValidationPermissionName, Strings.Msg_ValidationTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _logger.Info($"Adding permission: {name}");
            var dto = await _svc.CreateAsync(new PermissionDto(Guid.Empty, name, displayName ?? name, groupName ?? string.Empty));

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Permissions.Add(dto);
            });

            _logger.Info($"Permission '{name}' added successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to add permission: {name}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 删除权限（带确认）。
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var permission = Permissions.FirstOrDefault(p => p.Id == id);
        var name = permission?.Name ?? id.ToString();

        var result = MessageBox.Show($"{Strings.Msg_ConfirmDeletePermission}", Strings.Msg_ConfirmDelete,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            _logger.Info($"Deleting permission: {id}");
            await _svc.DeleteAsync(id);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (permission != null) Permissions.Remove(permission);
            });

            _logger.Info($"Permission '{name}' deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to delete permission: {id}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override async Task<IReadOnlyList<PermissionDto>> LoadItemsAsync()
        => await _svc.GetListAsync();
}


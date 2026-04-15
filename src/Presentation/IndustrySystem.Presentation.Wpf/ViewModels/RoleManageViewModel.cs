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

public class RoleManageViewModel : NagetiveCurdVeiwModel<RoleDto>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IRoleAppService _svc;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// 新增角色名称输入。
    /// </summary>
    private string _newRoleName = string.Empty;
    public string NewRoleName { get => _newRoleName; set => SetProperty(ref _newRoleName, value); }

    /// <summary>
    /// 新增角色描述输入。
    /// </summary>
    private string _newRoleDescription = string.Empty;
    public string NewRoleDescription { get => _newRoleDescription; set => SetProperty(ref _newRoleDescription, value); }

    /// <summary>
    /// 角色列表搜索关键字。
    /// </summary>
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RolesView.Refresh();
            }
        }
    }

    /// <summary>
    /// 原始角色集合。
    /// </summary>
    public ObservableCollection<RoleDto> Roles { get; } = new();

    /// <summary>
    /// 过滤后的角色视图（用于搜索）。
    /// </summary>
    public ICollectionView RolesView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ManagePermissionsCommand { get; }

    public RoleManageViewModel(IRoleAppService svc, IDialogService dialogService)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _dialogService = dialogService;
        RolesView = CollectionViewSource.GetDefaultView(Roles);
        RolesView.Filter = FilterRoles;

        // 页面命令：刷新、新增、编辑、删除、权限管理
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(async () => await AddCurrentAsync());
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenRoleDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });
        ManagePermissionsCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await ManagePermissionsAsync(id.Value);
        });

        _logger.Info("RoleManageViewModel initialized");

        // 延迟加载，避免首屏阻塞。
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(100);
            await LoadAsync();
        });
    }

    /// <summary>
    /// 角色过滤逻辑：按名称、描述模糊匹配。
    /// </summary>
    private bool FilterRoles(object item)
    {
        if (item is not RoleDto role) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var key = SearchText.Trim();
        return (role.Name?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
               || (role.Description?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// 执行新增并清空输入框。
    /// </summary>
    private async Task AddCurrentAsync()
    {
        await AddAsync(NewRoleName, NewRoleDescription);
        NewRoleName = string.Empty;
        NewRoleDescription = string.Empty;
    }

    /// <summary>
    /// 打开角色编辑弹窗，保存后刷新列表。
    /// </summary>
    private void OpenRoleDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", (Guid?)id } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.RoleEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                await LoadAsync();
            }
        });
    }

    /// <summary>
    /// 加载角色列表并刷新UI。
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            _logger.Debug("Loading roles...");

            var roles = await _svc.GetListAsync();
            _logger.Info($"Loaded {roles?.Count ?? 0} roles from database");

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Roles.Clear();
                if (roles != null)
                {
                    foreach (var r in roles)
                    {
                        Roles.Add(r);
                    }
                }
            });

            _logger.Info($"UI updated with {Roles.Count} roles");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load roles");
            MessageBox.Show($"{Strings.Msg_LoadFailed}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 新增角色。
    /// </summary>
    public async Task AddAsync(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(Strings.Msg_ValidationRoleName, Strings.Msg_ValidationTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _logger.Info($"Adding role: {name}");
            var dto = await _svc.CreateAsync(new RoleDto(Guid.Empty, name, description, false));

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Roles.Add(dto);
            });

            _logger.Info($"Role '{name}' added successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to add role: {name}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 管理角色权限：打开角色编辑弹窗进行权限配置。
    /// </summary>
    public Task ManagePermissionsAsync(Guid id)
    {
        _logger.Info($"Managing permissions for role {id}");
        OpenRoleDialogAsync(id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除角色（带默认角色保护与确认）。
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var role = Roles.FirstOrDefault(r => r.Id == id);
        if (role?.IsDefault == true)
        {
            MessageBox.Show(Strings.Msg_CannotDeleteDefault, Strings.Msg_WarningTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var name = role?.Name ?? id.ToString();
        var result = MessageBox.Show($"{Strings.Msg_ConfirmDeleteRole}", Strings.Msg_ConfirmDelete,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            _logger.Info($"Deleting role: {id}");
            await _svc.DeleteAsync(id);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (role != null) Roles.Remove(role);
            });

            _logger.Info($"Role '{name}' deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to delete role: {id}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override async Task<IReadOnlyList<RoleDto>> LoadItemsAsync()
        => await _svc.GetListAsync();
}


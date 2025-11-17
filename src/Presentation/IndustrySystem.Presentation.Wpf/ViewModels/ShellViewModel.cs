using Prism.Mvvm;
using Prism.Commands;
using Prism.Ioc;
using System.Windows.Input;
using ModernWpf.Controls;
using IndustrySystem.Presentation.Wpf.Views;
using IndustrySystem.Presentation.Wpf.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ShellViewModel : BindableBase
{
    private readonly IContainerProvider _container;
    private readonly IAuthState _authState;
    private object? _currentContent;
    private string _currentUserName = "未登录";
    private string _currentUserRole = string.Empty;

    public object? CurrentContent
    {
        get => _currentContent;
        private set => SetProperty(ref _currentContent, value);
    }

    /// <summary>当前登录用户名</summary>
    public string CurrentUserName
    {
        get => _currentUserName;
        private set => SetProperty(ref _currentUserName, value);
    }

    /// <summary>当前登录用户角色（可选）</summary>
    public string CurrentUserRole
    {
        get => _currentUserRole;
        private set => SetProperty(ref _currentUserRole, value);
    }

    public ICommand OnLoadedCommand { get; }
    public ICommand NavSelectionChangedCommand { get; }

    public ShellViewModel(IContainerProvider container)
    {
        _container = container;
        _authState = container.Resolve<IAuthState>();
        CurrentUserName = _authState.UserName ?? "未登录";
        _authState.AuthChanged += (s, e) => { CurrentUserName = _authState.UserName ?? "未登录"; };

        OnLoadedCommand = new DelegateCommand<object?>(OnLoaded);
        NavSelectionChangedCommand = new DelegateCommand<object?>(OnSelectionChanged);
    }

    private void OnLoaded(object? _)
    {
        // Default -> Users page
        Navigate("Users");
    }

    private void OnSelectionChanged(object? args)
    {
        if (args is NavigationViewSelectionChangedEventArgs ev && ev.SelectedItem is NavigationViewItem item && item.Tag is string tag)
        {
            Navigate(tag);
        }
    }

    /// <summary>
    /// Public navigation API for code-behind to navigate after login.
    /// </summary>
    public void NavigateTo(string tag) => Navigate(tag);

    private void Navigate(string tag)
    {
        object? next = tag switch
        {
            "Users" => _container.Resolve<UsersView>(),
            "Roles" => _container.Resolve<RoleManageView>(),
            "Permissions" => _container.Resolve<PermissionsView>(),
            "Templates" => _container.Resolve<ExperimentTemplateView>(),
            "ExperimentConfig" => _container.Resolve<ExperimentConfigView>(),
            "ExperimentGroupConfig" => _container.Resolve<ExperimentGroupsView>(),
            "ExperimentGroupTemplates" => _container.Resolve<ExperimentGroupsView>(),
            "RunExperiment" => _container.Resolve<RunExperimentView>(),
            "RealtimeData" => _container.Resolve<RealtimeDataView>(),
            "ExperimentHistory" => _container.Resolve<ExperimentHistoryView>(),
            "InventoryRecords" => _container.Resolve<InventoryView>(),
            "OperationLogs" => _container.Resolve<OperationLogsView>(),
            "MaterialInfo" => _container.Resolve<MaterialInfoView>(),
            "ShelfInfo" => _container.Resolve<ShelfInfoView>(),
            "Inventory" => _container.Resolve<InventoryView>(),
            "ManualDebug" => _container.Resolve<HardwareDebugView>(),
            "DeviceParams" => _container.Resolve<DeviceParamsView>(),
            "PeripheralDebug" => _container.Resolve<PeripheralDebugView>(),
            _ => null
        };
        if (next != null)
        {
            CurrentContent = next;
        }
    }
}

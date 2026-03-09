using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class PermissionsViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IPermissionAppService _svc;

    public ObservableCollection<PermissionDto> Permissions { get; } = new();

    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public PermissionsViewModel(IPermissionAppService svc)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

        _logger.Info("PermissionsViewModel initialized");

        // ÑÓ³Ù¼ÓÔØÊý¾Ý
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(100);
            await LoadAsync();
        });
    }

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

    public void EditPermission(Guid id)
    {
        _logger.Info($"Edit permission: {id}");
        MessageBox.Show($"Edit Permission: {id}\n\nThis feature is not yet implemented.",
            "Edit Permission", MessageBoxButton.OK, MessageBoxImage.Information);
    }

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
}


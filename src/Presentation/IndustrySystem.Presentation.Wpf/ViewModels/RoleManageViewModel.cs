using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class RoleManageViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IRoleAppService _svc;
    
    public ObservableCollection<RoleDto> Roles { get; } = new();
    
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public RoleManageViewModel(IRoleAppService svc)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        DeleteCommand = new DelegateCommand<Guid?>(async id => 
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });
        
        _logger.Info("RoleManageViewModel initialized");
        
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
            MessageBox.Show($"Failed to load roles: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task AddAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Please enter role name", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        try
        {
            _logger.Info($"Adding role: {name}");
            var dto = await _svc.CreateAsync(new RoleDto(Guid.Empty, name, null, false));
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Roles.Add(dto);
            });
            
            _logger.Info($"Role '{name}' added successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to add role: {name}");
            MessageBox.Show($"Failed to add role: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task ManagePermissionsAsync(Guid id)
    {
        _logger.Info($"Managing permissions for role {id}");
        MessageBox.Show($"Manage Permissions for Role: {id}\n\nThis feature is not yet implemented.", 
            "Manage Permissions", MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = Roles.FirstOrDefault(r => r.Id == id);
        if (role?.IsDefault == true)
        {
            MessageBox.Show("Cannot delete default role", "Warning", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        var name = role?.Name ?? id.ToString();
        var result = MessageBox.Show($"Delete role '{name}'?", "Confirm Delete",
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
            MessageBox.Show($"Failed to delete role: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

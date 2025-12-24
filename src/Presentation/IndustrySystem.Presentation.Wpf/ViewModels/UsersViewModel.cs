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

public class UsersViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IUserAppService _svc;
    
    public ObservableCollection<UserDto> Users { get; } = new();
    
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public UsersViewModel(IUserAppService svc)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        DeleteCommand = new DelegateCommand<Guid?>(async id => 
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });
        
        _logger.Info("UsersViewModel initialized");
        
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
            _logger.Debug("Loading users...");
            
            var users = await _svc.GetListAsync();
            _logger.Info($"Loaded {users?.Count ?? 0} users from database");
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Users.Clear();
                if (users != null)
                {
                    foreach (var u in users)
                    {
                        Users.Add(u);
                    }
                }
            });
            
            _logger.Info($"UI updated with {Users.Count} users");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load users");
            MessageBox.Show($"Failed to load users: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task AddAsync(string userName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            MessageBox.Show("Please enter user name", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        try
        {
            _logger.Info($"Adding user: {userName}");
            var dto = await _svc.CreateAsync(new UserDto(Guid.Empty, userName, displayName ?? userName, true));
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Users.Add(dto);
            });
            
            _logger.Info($"User '{userName}' added successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to add user: {userName}");
            MessageBox.Show($"Failed to add user: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task ResetPasswordAsync(Guid id)
    {
        _logger.Info($"Resetting password for user {id}");
        MessageBox.Show($"Reset Password for User: {id}\n\nThis feature is not yet implemented.", 
            "Reset Password", MessageBoxButton.OK, MessageBoxImage.Information);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        var name = user?.UserName ?? id.ToString();
        
        var result = MessageBox.Show($"Delete user '{name}'?", "Confirm Delete",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        
        try
        {
            _logger.Info($"Deleting user: {id}");
            await _svc.DeleteAsync(id);
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (user != null) Users.Remove(user);
            });
            
            _logger.Info($"User '{name}' deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to delete user: {id}");
            MessageBox.Show($"Failed to delete user: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

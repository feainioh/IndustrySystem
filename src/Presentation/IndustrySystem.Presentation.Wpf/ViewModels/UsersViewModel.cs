using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
using IndustrySystem.Presentation.Wpf.Views.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class UsersViewModel : CrudViewModel<UserDto>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IUserAppService _svc;
    private readonly IDialogService _dialogService;

    private string _newUserName = string.Empty;
    public string NewUserName { get => _newUserName; set => SetProperty(ref _newUserName, value); }

    private string _newDisplayName = string.Empty;
    public string NewDisplayName { get => _newDisplayName; set => SetProperty(ref _newDisplayName, value); }

    public ObservableCollection<UserDto> Users { get; } = new();
    public ObservableCollection<UserDto> PagedUsers { get; } = new();
    public ICollectionView UsersView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ResetPasswordCommand { get; }

    public UsersViewModel(IUserAppService svc, IDialogService dialogService)
    {
        _svc = svc ?? throw new ArgumentNullException(nameof(svc));
        _dialogService = dialogService;
        UsersView = CollectionViewSource.GetDefaultView(Users);
        UsersView.Filter = FilterUsers;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(async () => await AddCurrentAsync());
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenUserDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });
        ResetPasswordCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await ResetPasswordAsync(id.Value);
        });

        _logger.Info("UsersViewModel initialized");

        // Delay initial load to ensure the view is fully initialized.
        System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            await Task.Delay(100);
            await LoadAsync();
        });
    }

    private bool FilterUsers(object item)
    {
        if (item is not UserDto user) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var key = SearchText.Trim();
        return (user.UserName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
               || (user.DisplayName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private async Task AddCurrentAsync()
    {
        await AddAsync(NewUserName, NewDisplayName);
        NewUserName = string.Empty;
        NewDisplayName = string.Empty;
    }

    private void OpenUserDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", (Guid?)id } };
        _dialogService.ShowDialog(nameof(UserEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                await LoadAsync();
            }
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

                ApplyUserPaging(resetToFirstPage: true);
            });

            _logger.Info($"UI updated with {Users.Count} users");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load users");
            MessageBox.Show($"{Strings.Msg_LoadFailed}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task AddAsync(string userName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            MessageBox.Show(Strings.Msg_ValidationUserName, Strings.Msg_ValidationTitle,
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            _logger.Info($"Adding user: {userName}");
            var dto = await _svc.CreateAsync(new UserDto { Id = Guid.Empty, UserName = userName, DisplayName = displayName ?? userName, IsActive = true });

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Users.Add(dto);
                ApplyUserPaging();
            });

            _logger.Info($"User '{userName}' added successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to add user: {userName}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public async Task ResetPasswordAsync(Guid id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        var userName = user?.UserName ?? id.ToString();

        var input = PromptChangePassword(userName);
        if (input == null || string.IsNullOrWhiteSpace(input.Value.OldPassword) || string.IsNullOrWhiteSpace(input.Value.NewPassword))
        {
            return;
        }

        try
        {
            await _svc.ChangePasswordAsync(id, input.Value.OldPassword, input.Value.NewPassword);
            MessageBox.Show("密码修改成功", Strings.Msg_SuccessTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to change password for user: {id}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static (string OldPassword, string NewPassword)? PromptChangePassword(string userName)
    {
        var window = new Window
        {
            Title = $"修改密码 - {userName}",
            Width = 380,
            Height = 230,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.ToolWindow
        };

        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = "原密码：", Margin = new Thickness(0, 0, 0, 6) });
        var oldPasswordBox = new PasswordBox { Height = 32, Margin = new Thickness(0, 0, 0, 10) };
        panel.Children.Add(oldPasswordBox);

        panel.Children.Add(new TextBlock { Text = "新密码：", Margin = new Thickness(0, 0, 0, 6) });
        var newPasswordBox = new PasswordBox { Height = 32, Margin = new Thickness(0, 0, 0, 12) };
        panel.Children.Add(newPasswordBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        (string OldPassword, string NewPassword)? result = null;
        var ok = new Button { Content = "确定", Width = 80, Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "取消", Width = 80 };

        ok.Click += (_, _) =>
        {
            result = (oldPasswordBox.Password, newPasswordBox.Password);
            window.DialogResult = true;
            window.Close();
        };
        cancel.Click += (_, _) =>
        {
            window.DialogResult = false;
            window.Close();
        };

        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(buttons);

        window.Content = panel;
        return window.ShowDialog() == true ? result : null;
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = Users.FirstOrDefault(u => u.Id == id);
        var name = user?.UserName ?? id.ToString();

        var result = MessageBox.Show($"{Strings.Msg_ConfirmDeleteUser}", Strings.Msg_ConfirmDelete,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            _logger.Info($"Deleting user: {id}");
            await _svc.DeleteAsync(id);

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (user != null) Users.Remove(user);
                ApplyUserPaging();
            });

            _logger.Info($"User '{name}' deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to delete user: {id}");
            MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override async Task<IReadOnlyList<UserDto>> LoadItemsAsync()
        => await _svc.GetListAsync();

    protected override void OnSearchTextChanged()
    {
        ApplyUserPaging(resetToFirstPage: true);
    }

    protected override void OnPagingParametersChanged(bool resetToFirstPage)
    {
        ApplyUserPaging(resetToFirstPage);
    }

    private IEnumerable<UserDto> BuildFilteredUsers()
    {
        var query = Users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var key = SearchText.Trim();
            query = query.Where(user =>
                (user.UserName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (user.DisplayName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return query;
    }

    private void ApplyUserPaging(bool resetToFirstPage = false)
    {
        var filtered = BuildFilteredUsers().ToList();
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

        PagedUsers.Clear();
        foreach (var user in filtered.Skip(PageIndex * PageSize).Take(PageSize))
        {
            PagedUsers.Add(user);
        }

        RaisePagingCommandStates();
    }
}


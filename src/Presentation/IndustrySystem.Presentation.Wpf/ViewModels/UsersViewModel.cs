using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class UsersViewModel
{
    private readonly IUserAppService _svc;
    public ObservableCollection<UserDto> Users { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public UsersViewModel(IUserAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        DeleteCommand = new AsyncDelegateCommand<Guid>(DeleteAsync);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Users.Clear();
        foreach (var u in await _svc.GetListAsync()) Users.Add(u);
    }

    public async Task AddAsync(string userName, string displayName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return;
        var dto = await _svc.CreateAsync(new UserDto(Guid.Empty, userName, displayName, true));
        Users.Add(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _svc.DeleteAsync(id);
        var target = Users.FirstOrDefault(x => x.Id == id);
        if (target != null) Users.Remove(target);
    }
}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class RoleManageViewModel
{
    private readonly IRoleAppService _svc;
    public ObservableCollection<RoleDto> Roles { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public RoleManageViewModel(IRoleAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        DeleteCommand = new AsyncDelegateCommand<Guid>(DeleteAsync);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Roles.Clear();
        foreach (var r in await _svc.GetListAsync()) Roles.Add(r);
    }

    public async Task AddAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var dto = await _svc.CreateAsync(new RoleDto(Guid.Empty, name, null, false));
        Roles.Add(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _svc.DeleteAsync(id);
        var target = Roles.FirstOrDefault(x => x.Id == id);
        if (target != null) Roles.Remove(target);
    }
}

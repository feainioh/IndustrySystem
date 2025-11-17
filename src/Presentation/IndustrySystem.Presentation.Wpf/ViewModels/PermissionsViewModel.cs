using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class PermissionsViewModel
{
    private readonly IPermissionAppService _svc;
    public ObservableCollection<PermissionDto> Permissions { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand DeleteCommand { get; }

    public PermissionsViewModel(IPermissionAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        DeleteCommand = new AsyncDelegateCommand<Guid>(DeleteAsync);
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        Permissions.Clear();
        foreach (var p in await _svc.GetListAsync()) Permissions.Add(p);
    }

    public async Task AddAsync(string name, string displayName)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        var dto = await _svc.CreateAsync(new PermissionDto(Guid.Empty, name, displayName));
        Permissions.Add(dto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _svc.DeleteAsync(id);
        var target = Permissions.FirstOrDefault(x => x.Id == id);
        if (target != null) Permissions.Remove(target);
    }
}

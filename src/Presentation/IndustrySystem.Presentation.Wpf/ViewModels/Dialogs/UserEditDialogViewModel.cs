using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class UserEditDialogViewModel : DialogViewModel
{
    private readonly IUserAppService _svc; private readonly IRoleAppService _roleSvc;
    public UserEditDialogViewModel(IUserAppService svc, IRoleAppService roleSvc) { _svc = svc; _roleSvc = roleSvc; Title = "±à¼­ÓÃ»§"; }

    private Guid _id; private string _userName = string.Empty; private string _displayName = string.Empty; private bool _isActive = true;
    public Guid Id { get => _id; set => SetProperty(ref _id, value); }
    public string UserName { get => _userName; set => SetProperty(ref _userName, value); }
    public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
    public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }

    public ObservableCollection<RoleItem> AllRoles { get; } = new();

    public async Task LoadAsync(Guid? id)
    {
        AllRoles.Clear(); var roles = await _roleSvc.GetListAsync(); foreach (var r in roles) AllRoles.Add(new RoleItem(r.Id, r.Name));
        if (id is { } v)
        {
            var dto = await _svc.GetAsync(v); if (dto != null)
            {
                Id = dto.Id; UserName = dto.UserName; DisplayName = dto.DisplayName; IsActive = dto.IsActive;
                var checkedIds = await _svc.GetRoleIdsAsync(Id); foreach (var item in AllRoles) item.IsChecked = checkedIds.Contains(item.Id);
            }
        }
        else { Id = Guid.Empty; UserName = string.Empty; DisplayName = string.Empty; IsActive = true; }
    }

    protected override bool CanSave() => !string.IsNullOrWhiteSpace(UserName);
    protected override async Task OnSaveAsync()
    {
        var input = new UserDto(Id, UserName, DisplayName, IsActive);
        UserDto saved = Id == Guid.Empty ? await _svc.CreateAsync(input) : await _svc.UpdateAsync(input);
        var roleIds = AllRoles.Where(x => x.IsChecked).Select(x => x.Id).ToArray(); await _svc.SetRolesAsync(saved.Id, roleIds);
        DialogResult = true;
    }
    protected override void OnCancel() => DialogResult = false;
}

public class RoleItem : Prism.Mvvm.BindableBase
{
    public Guid Id { get; }
    public string Name { get; }
    private bool _isChecked; public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
    public RoleItem(Guid id, string name) { Id = id; Name = name; }
}

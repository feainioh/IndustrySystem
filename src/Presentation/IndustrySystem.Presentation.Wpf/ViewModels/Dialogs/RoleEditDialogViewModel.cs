using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

/// <summary>
///角色编辑弹窗 VM，支持必填校验与权限分配。
/// </summary>
public class RoleEditDialogViewModel : DialogViewModel, INotifyDataErrorInfo
{
 private readonly IRoleAppService _svc;
 private readonly IPermissionAppService _permSvc;
 public RoleEditDialogViewModel(IRoleAppService svc, IPermissionAppService permSvc)
 {
 _svc = svc; _permSvc = permSvc;
 Title = "编辑角色";
 }

 private Guid _id;
 private string _name = string.Empty;
 private string? _description;
 private bool _isDefault;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public string Name { get => _name; set { if (SetProperty(ref _name, value)) { ValidateRequired(nameof(Name), _name, "名称不能为空"); } } }
 public string? Description { get => _description; set => SetProperty(ref _description, value); }
 public bool IsDefault { get => _isDefault; set => SetProperty(ref _isDefault, value); }

 public ObservableCollection<CheckItem> AllPermissions { get; } = new();

 public async Task LoadAsync(Guid? id)
 {
 ClearErrors();
 AllPermissions.Clear();
 var perms = await _permSvc.GetListAsync();
 foreach (var p in perms) AllPermissions.Add(new CheckItem(p.Id, p.DisplayName));

 if (id is { } v)
 {
 var dto = await _svc.GetAsync(v);
 if (dto != null)
 {
 Id = dto.Id;
 Name = dto.Name;
 Description = dto.Description;
 IsDefault = dto.IsDefault;
 var checkedIds = await _svc.GetPermissionIdsAsync(Id);
 foreach (var item in AllPermissions) item.IsChecked = checkedIds.Contains(item.Id);
 }
 }
 else
 {
 Id = Guid.Empty;
 Name = string.Empty;
 Description = null;
 IsDefault = false;
 }
 }

 protected override bool CanSave() => !HasErrors;

 protected override async Task OnSaveAsync()
 {
 try
 {
 var input = new IndustrySystem.Application.Contracts.Dtos.RoleDto(Id, Name, Description, IsDefault);
 IndustrySystem.Application.Contracts.Dtos.RoleDto saved;
 if (Id == Guid.Empty)
 {
 saved = await _svc.CreateAsync(input);
 Id = saved.Id;
 }
 else
 {
 saved = await _svc.UpdateAsync(input);
 }
 var permIds = AllPermissions.Where(x => x.IsChecked).Select(x => x.Id).ToArray();
 await _svc.SetPermissionsAsync(Id, permIds);
 DialogResult = true;
 }
 catch (Exception ex)
 {
 AddError(string.Empty, ex.Message);
 }
 }

 #region Validation
 private readonly Dictionary<string, List<string>> _errors = new();
 public bool HasErrors => _errors.Count >0;
 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
 public IEnumerable GetErrors(string? propertyName)
 {
 if (string.IsNullOrEmpty(propertyName))
 { foreach (var kv in _errors) foreach (var e in kv.Value) yield return e; yield break; }
 if (_errors.TryGetValue(propertyName, out var list)) { foreach (var e in list) yield return e; }
 }
 private void ValidateRequired(string propertyName, string? value, string error)
 { if (string.IsNullOrWhiteSpace(value)) AddError(propertyName, error); else ClearError(propertyName); }
 private void AddError(string propertyName, string error)
 {
 if (!_errors.TryGetValue(propertyName, out var list)) { list = new List<string>(); _errors[propertyName] = list; }
 if (!list.Contains(error)) { list.Add(error); ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); }
 }
 private void ClearError(string propertyName)
 { if (_errors.Remove(propertyName)) { ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); } }
 private void ClearErrors()
 { var keys = _errors.Keys.ToList(); _errors.Clear(); foreach (var k in keys) ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(k)); }
 #endregion
}

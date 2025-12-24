using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class PermissionEditDialogViewModel : DialogViewModel, INotifyDataErrorInfo
{
 private readonly IPermissionAppService _svc;
 private Guid _id;
 private string _name = string.Empty;
 private string _displayName = string.Empty;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public string Name { get => _name; set { if (SetProperty(ref _name, value)) { ValidateRequired(nameof(Name), _name, "名称不能为空"); ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged(); } } }
 public string DisplayName { get => _displayName; set { if (SetProperty(ref _displayName, value)) { ValidateRequired(nameof(DisplayName), _displayName, "显示名不能为空"); ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged(); } } }

 public PermissionEditDialogViewModel(IPermissionAppService svc)
 { _svc = svc; Title = "编辑权限"; }

 public async Task LoadAsync(Guid? id)
 {
 ClearErrors();
 if (id is { } v)
 {
 var dto = await _svc.GetAsync(v);
 if (dto != null)
 {
 Id = dto.Id; Name = dto.Name; DisplayName = dto.DisplayName;
 }
 }
 else
 {
 Id = Guid.Empty; Name = string.Empty; DisplayName = string.Empty;
 }
 }

 protected override bool CanSave() => !HasErrors;
 protected override async Task OnSaveAsync()
 {
 try
 {
 var input = new PermissionDto(Id, Name, DisplayName);
 _ = Id == Guid.Empty ? await _svc.CreateAsync(input) : await _svc.UpdateAsync(input);
 DialogResult = true;
 }
 catch (Exception ex)
 {
 AddError(string.Empty, ex.Message);
 }
 }
 protected override void OnCancel() => DialogResult = false;

 #region Validation
 private readonly Dictionary<string, List<string>> _errors = new();
 public bool HasErrors => _errors.Count > 0;
 public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
 public IEnumerable GetErrors(string? propertyName)
 { if (string.IsNullOrEmpty(propertyName)) { foreach (var kv in _errors) foreach (var e in kv.Value) yield return e; yield break; } if (_errors.TryGetValue(propertyName, out var list)) { foreach (var e in list) yield return e; } }
 private void ValidateRequired(string propertyName, string? value, string error) { if (string.IsNullOrWhiteSpace(value)) AddError(propertyName, error); else ClearError(propertyName); }
 private void AddError(string propertyName, string error) { if (!_errors.TryGetValue(propertyName, out var list)) { list = new List<string>(); _errors[propertyName] = list; } if (!list.Contains(error)) { list.Add(error); ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); } }
 private void ClearError(string propertyName) { if (_errors.Remove(propertyName)) { ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName)); } }
 private void ClearErrors() { var keys = _errors.Keys.ToList(); _errors.Clear(); foreach (var k in keys) ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(k)); }
 #endregion
}

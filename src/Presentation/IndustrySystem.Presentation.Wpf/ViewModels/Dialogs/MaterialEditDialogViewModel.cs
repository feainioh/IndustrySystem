using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class MaterialEditDialogViewModel : DialogViewModel
{
 private Guid _id;
 private string _name = string.Empty;
 private int _qty;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public string Name { get => _name; set => SetProperty(ref _name, value); }
 public int Qty { get => _qty; set => SetProperty(ref _qty, value); }

 public ICommand SaveCommand { get; }
 public ICommand CancelCommand { get; }

 public MaterialEditDialogViewModel()
 {
 Title = "编辑物料";
 SaveCommand = new AsyncDelegateCommand(SaveAsync, CanSave);
 CancelCommand = new DelegateCommand(Cancel);
 }

 public Task LoadAsync(Guid? id)
 {
 if (id is null)
 {
 Id = Guid.Empty;
 Name = string.Empty;
 Qty =0;
 }
 else
 {
 Id = id.Value;
 }
 return Task.CompletedTask;
 }

 private bool CanSave() => !string.IsNullOrWhiteSpace(Name);
 private Task SaveAsync()
 {
 //需要物料服务接口以完成创建/更新
 DialogResult = true;
 return Task.CompletedTask;
 }
 private void Cancel() => DialogResult = false;
}

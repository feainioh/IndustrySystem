using System;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentGroupEditDialogViewModel : DialogViewModel
{
 private Guid _id;
 private string _name = string.Empty;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public string Name { get => _name; set => SetProperty(ref _name, value); }

 public ICommand SaveCommand { get; }
 public ICommand CancelCommand { get; }

 public ExperimentGroupEditDialogViewModel()
 {
 Title = "编辑实验组";
 SaveCommand = new AsyncDelegateCommand(SaveAsync, CanSave);
 CancelCommand = new DelegateCommand(Cancel);
 }

 public Task LoadAsync(Guid? id)
 {
 if (id is null)
 {
 Id = Guid.Empty;
 Name = string.Empty;
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
 //需要后续服务接口支持
 DialogResult = true;
 return Task.CompletedTask;
 }
 private void Cancel() => DialogResult = false;
}

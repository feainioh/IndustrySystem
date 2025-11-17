using System;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentEditDialogViewModel : DialogViewModel
{
 // NOTE: IExperimentAppService currently has no create/update methods in Contracts.
 // This VM is scaffolded and awaits service expansion.
 private Guid _id;
 private Guid _templateId;
 private Guid _groupId;
 private string _name = string.Empty;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public Guid TemplateId { get => _templateId; set => SetProperty(ref _templateId, value); }
 public Guid GroupId { get => _groupId; set => SetProperty(ref _groupId, value); }
 public string Name { get => _name; set => SetProperty(ref _name, value); }

 public ICommand SaveCommand { get; }
 public ICommand CancelCommand { get; }

 public ExperimentEditDialogViewModel()
 {
 Title = "±à¼­ÊµÑé";
 SaveCommand = new AsyncDelegateCommand(SaveAsync, CanSave);
 CancelCommand = new DelegateCommand(Cancel);
 }

 public Task LoadAsync(Guid? id)
 {
 // Placeholder load, depends on future service methods
 if (id is null)
 {
 Id = Guid.Empty;
 Name = string.Empty;
 TemplateId = Guid.Empty;
 GroupId = Guid.Empty;
 }
 else
 {
 Id = id.Value; // would fetch real data once services are available
 }
 return Task.CompletedTask;
 }

 private bool CanSave() => !string.IsNullOrWhiteSpace(Name);
 private Task SaveAsync()
 {
 // awaiting service expansion: create/update experiment
 DialogResult = true;
 return Task.CompletedTask;
 }
 private void Cancel() => DialogResult = false;
}

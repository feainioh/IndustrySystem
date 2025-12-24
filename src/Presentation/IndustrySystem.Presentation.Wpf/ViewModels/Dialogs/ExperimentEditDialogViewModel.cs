using System;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentEditDialogViewModel : DialogViewModel
{
 private Guid _id;
 private Guid _templateId;
 private Guid _groupId;
 private string _name = string.Empty;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public Guid TemplateId { get => _templateId; set => SetProperty(ref _templateId, value); }
 public Guid GroupId { get => _groupId; set => SetProperty(ref _groupId, value); }
 public string Name { get => _name; set => SetProperty(ref _name, value); }

 public ExperimentEditDialogViewModel() { Title = "±à¼­ÊµÑé"; }

 public Task LoadAsync(Guid? id)
 {
 if (id is null)
 {
 Id = Guid.Empty; Name = string.Empty; TemplateId = Guid.Empty; GroupId = Guid.Empty;
 }
 else { Id = id.Value; }
 return Task.CompletedTask;
 }

 protected override bool CanSave() => !string.IsNullOrWhiteSpace(Name);
 protected override Task OnSaveAsync()
 {
 DialogResult = true;
 return Task.CompletedTask;
 }
 protected override void OnCancel() => DialogResult = false;
}

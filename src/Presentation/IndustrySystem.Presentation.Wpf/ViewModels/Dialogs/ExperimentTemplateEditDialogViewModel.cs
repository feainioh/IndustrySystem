using System;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentTemplateEditDialogViewModel : DialogViewModel
{
 private readonly IExperimentTemplateAppService _svc;
 private Guid _id;
 private string _name = string.Empty;
 private string? _description;

 public Guid Id { get => _id; set => SetProperty(ref _id, value); }
 public string Name { get => _name; set => SetProperty(ref _name, value); }
 public string? Description { get => _description; set => SetProperty(ref _description, value); }

 public ICommand SaveCommand { get; }
 public ICommand CancelCommand { get; }

 public ExperimentTemplateEditDialogViewModel(IExperimentTemplateAppService svc)
 {
 _svc = svc;
 Title = "±à¼­ÊµÑéÄ£°å";
 SaveCommand = new AsyncDelegateCommand(SaveAsync, CanSave);
 CancelCommand = new DelegateCommand(Cancel);
 }

 public async Task LoadAsync(Guid? id)
 {
 if (id is { } v)
 {
 var dto = await _svc.GetAsync(v);
 if (dto != null)
 {
 Id = dto.Id;
 Name = dto.Name;
 Description = dto.Description;
 }
 }
 else
 {
 Id = Guid.Empty;
 Name = string.Empty;
 Description = null;
 }
 }

 private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

 private async Task SaveAsync()
 {
 var input = new ExperimentTemplateDto(Id, Name, Description);
 var result = Id == Guid.Empty
 ? await _svc.CreateAsync(input)
 : await _svc.UpdateAsync(input);
 DialogResult = true;
 }

 private void Cancel() => DialogResult = false;
}

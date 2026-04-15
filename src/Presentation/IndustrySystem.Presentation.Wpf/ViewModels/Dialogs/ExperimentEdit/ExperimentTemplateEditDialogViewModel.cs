using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentTemplateEditDialogViewModel : DialogViewModel
{
    private readonly IExperimentTemplateAppService _svc;
    private Guid _id;
    private string _name = string.Empty;
    private string? _description;
    private ExperimentType _selectedType = ExperimentType.Reaction;

    public Guid Id { get => _id; set => SetProperty(ref _id, value); }

    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                RaiseSaveCanExecuteChanged();
        }
    }

    public string? Description { get => _description; set => SetProperty(ref _description, value); }

    public ExperimentType SelectedType
    {
        get => _selectedType;
        set => SetProperty(ref _selectedType, value);
    }

    public ObservableCollection<ExperimentType> ExperimentTypes { get; } =
        Enum.GetValues(typeof(ExperimentType)).Cast<ExperimentType>().ToObservableCollection();

    public ExperimentTemplateEditDialogViewModel(IExperimentTemplateAppService svc)
    {
        _svc = svc;
        Title = "新建实验模板";
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        var id = parameters.GetValue<Guid?>("id");
        _ = LoadAsync(id);
    }

    public async Task LoadAsync(Guid? id)
    {
        if (id is { } v)
        {
            Title = "编辑实验模板";
            var dto = await _svc.GetAsync(v);
            if (dto != null)
            {
                Id = dto.Id;
                Name = dto.Name;
                SelectedType = dto.Type;
                Description = dto.Description;
            }
        }
        else
        {
            Title = "新建实验模板";
            Id = Guid.Empty;
            Name = string.Empty;
            SelectedType = ExperimentType.Reaction;
            Description = null;
        }
    }

    protected override bool CanSave() => !string.IsNullOrWhiteSpace(Name);

    protected override async Task OnSaveAsync()
    {
        var now = DateTime.Now;
        var input = new ExperimentTemplateDto(
            Id,
            Name.Trim(),
            SelectedType,
            null,
            true,
            Id == Guid.Empty ? now : now,
            now,
            Description);

        _ = Id == Guid.Empty
            ? await _svc.CreateAsync(input)
            : await _svc.UpdateAsync(input);
        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }

    protected override void OnCancel() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
}

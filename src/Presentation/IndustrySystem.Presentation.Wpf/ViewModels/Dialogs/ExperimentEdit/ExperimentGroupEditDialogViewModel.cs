using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Services;
using Prism.Dialogs;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentGroupEditDialogViewModel : DialogViewModel
{
    private readonly IExperimentGroupAppService _svc;
    private readonly IAuthState _authState;

    private Guid _id;
    public Guid Id { get => _id; set => SetProperty(ref _id, value); }

    private string _groupCode = string.Empty;
    public string GroupCode
    {
        get => _groupCode;
        set
        {
            if (SetProperty(ref _groupCode, value))
                RaiseSaveCanExecuteChanged();
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
                RaiseSaveCanExecuteChanged();
        }
    }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    private bool _isEnabled = true;
    public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }

    private string _createdBy = string.Empty;
    public string CreatedBy { get => _createdBy; set => SetProperty(ref _createdBy, value); }

    private DateTime _createdAt = DateTime.Now;
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }

    private DateTime? _updatedAt;
    public DateTime? UpdatedAt { get => _updatedAt; set => SetProperty(ref _updatedAt, value); }

    public ObservableCollection<ExperimentSelectionItem> ExperimentOptions { get; } = new();

    public string StepSummary
    {
        get
        {
            var selected = ExperimentOptions.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0) return "未选择步骤";
            if (selected.Count <= 3) return string.Join(" → ", selected.Select(x => x.Name));
            return $"已选择 {selected.Count} 个实验步骤";
        }
    }

    public ExperimentGroupEditDialogViewModel(IExperimentGroupAppService svc, IAuthState authState)
    {
        _svc = svc;
        _authState = authState;
        Title = "实验组编辑";
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        var id = parameters.GetValue<Guid?>("id");
        _ = LoadAsync(id);
    }

    public async Task LoadAsync(Guid? id)
    {
        var options = await _svc.GetExperimentOptionsAsync();
        ExperimentOptions.Clear();
        foreach (var option in options)
        {
            var item = new ExperimentSelectionItem { Id = option.Id, Name = option.Name };
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ExperimentSelectionItem.IsSelected))
                {
                    RaisePropertyChanged(nameof(StepSummary));
                    RaiseSaveCanExecuteChanged();
                }
            };
            ExperimentOptions.Add(item);
        }

        if (id is null)
        {
            Id = Guid.Empty;
            GroupCode = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            IsEnabled = true;
            CreatedBy = _authState.UserName ?? "系统";
            CreatedAt = DateTime.Now;
            UpdatedAt = null;
            RaisePropertyChanged(nameof(StepSummary));
            return;
        }

        var dto = await _svc.GetAsync(id.Value);
        if (dto is null) return;

        Id = dto.Id;
        GroupCode = dto.GroupCode;
        Name = dto.Name;
        Description = dto.Description;
        IsEnabled = dto.IsEnabled;
        CreatedBy = dto.CreatedBy;
        CreatedAt = dto.CreatedAt;
        UpdatedAt = dto.UpdatedAt;

        var selectedSet = dto.StepExperimentIds.ToHashSet();
        foreach (var item in ExperimentOptions)
            item.IsSelected = selectedSet.Contains(item.Id);

        RaisePropertyChanged(nameof(StepSummary));
    }

    protected override bool CanSave()
        => !string.IsNullOrWhiteSpace(Name)
           && ExperimentOptions.Any(x => x.IsSelected);

    protected override async Task OnSaveAsync()
    {
        var dto = new ExperimentGroupDto(
            Id,
            string.IsNullOrWhiteSpace(GroupCode) ? string.Empty : GroupCode.Trim(),
            Name.Trim(),
            Description?.Trim() ?? string.Empty,
            ExperimentOptions.Where(x => x.IsSelected).Select(x => x.Id).ToList(),
            IsEnabled,
            string.IsNullOrWhiteSpace(CreatedBy) ? (_authState.UserName ?? "系统") : CreatedBy.Trim(),
            CreatedAt == default ? DateTime.Now : CreatedAt,
            DateTime.Now);

        if (Id == Guid.Empty)
            await _svc.CreateAsync(dto);
        else
            await _svc.UpdateAsync(dto);

        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }

    protected override void OnCancel() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
}

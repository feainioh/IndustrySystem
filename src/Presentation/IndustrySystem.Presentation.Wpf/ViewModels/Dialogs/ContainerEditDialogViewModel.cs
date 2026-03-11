using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ContainerEditDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;

    public Guid Id { get; set; }

    private string _name = string.Empty;
    public string Name { get => _name; set { if (SetProperty(ref _name, value)) RaiseSaveCanExecuteChanged(); } }

    private ContainerType _containerType = ContainerType.Other;
    public ContainerType ContainerType { get => _containerType; set => SetProperty(ref _containerType, value); }

    private int _rows = 1;
    public int Rows { get => _rows; set => SetProperty(ref _rows, value); }

    private int _columns = 1;
    public int Columns { get => _columns; set => SetProperty(ref _columns, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    public Array ContainerTypeOptions => Enum.GetValues(typeof(ContainerType));

    public ContainerEditDialogViewModel(IShelfAppService svc)
    {
        _svc = svc;
        Title = "容器信息";
    }

    public async Task LoadAsync(Guid? id)
    {
        if (id is null)
        {
            Id = Guid.Empty;
            Name = string.Empty;
            ContainerType = ContainerType.Other;
            Rows = 1;
            Columns = 1;
            Description = string.Empty;
            return;
        }

        var item = await _svc.GetContainerAsync(id.Value);
        if (item is null) return;
        Id = item.Id;
        Name = item.Name;
        ContainerType = item.ContainerType;
        Rows = item.Rows;
        Columns = item.Columns;
        Description = item.Description;
    }

    protected override bool CanSave() => !string.IsNullOrWhiteSpace(Name);

    protected override async Task OnSaveAsync()
    {
        var dto = new ContainerInfoDto(Id, Name.Trim(), ContainerType, Rows, Columns, Description.Trim());
        if (Id == Guid.Empty)
            await _svc.CreateContainerAsync(dto);
        else
            await _svc.UpdateContainerAsync(dto);
        DialogResult = true;
    }

    protected override void OnCancel() => DialogResult = false;
}

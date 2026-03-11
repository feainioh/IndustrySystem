using System;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ShelfEditDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;

    public Guid Id { get; set; }

    private string _shelfCode = string.Empty;
    public string ShelfCode { get => _shelfCode; set { if (SetProperty(ref _shelfCode, value)) RaiseSaveCanExecuteChanged(); } }

    private string _name = string.Empty;
    public string Name { get => _name; set { if (SetProperty(ref _name, value)) RaiseSaveCanExecuteChanged(); } }

    private int _rows = 4;
    public int Rows { get => _rows; set => SetProperty(ref _rows, value); }

    private int _columns = 6;
    public int Columns { get => _columns; set => SetProperty(ref _columns, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    public ShelfEditDialogViewModel(IShelfAppService svc)
    {
        _svc = svc;
        Title = "货架配置";
    }

    public async Task LoadAsync(Guid? id)
    {
        if (id is null)
        {
            Id = Guid.Empty;
            ShelfCode = string.Empty;
            Name = string.Empty;
            Rows = 4;
            Columns = 6;
            Description = string.Empty;
            return;
        }

        var item = await _svc.GetShelfAsync(id.Value);
        if (item is null) return;
        Id = item.Id;
        ShelfCode = item.ShelfCode;
        Name = item.Name;
        Rows = item.Rows;
        Columns = item.Columns;
        Description = item.Description;
    }

    protected override bool CanSave()
        => !string.IsNullOrWhiteSpace(ShelfCode) && !string.IsNullOrWhiteSpace(Name) && Rows > 0 && Columns > 0;

    protected override async Task OnSaveAsync()
    {
        var dto = new ShelfConfigDto(Id, ShelfCode.Trim(), Name.Trim(), Rows, Columns, Description.Trim());
        if (Id == Guid.Empty)
            await _svc.CreateShelfAsync(dto);
        else
            await _svc.UpdateShelfAsync(dto);
        DialogResult = true;
    }

    protected override void OnCancel() => DialogResult = false;
}

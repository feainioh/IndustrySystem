using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class InventoryEditDialogViewModel : DialogViewModel
{
    private readonly IInventoryAppService _svc;
    private readonly IMaterialAppService _materialSvc;

    public Guid Id { get; set; }

    private Guid _materialId;
    public Guid MaterialId { get => _materialId; set => SetProperty(ref _materialId, value); }

    private string _materialCode = string.Empty;
    public string MaterialCode { get => _materialCode; set { if (SetProperty(ref _materialCode, value)) RaiseSaveCanExecuteChanged(); } }

    private string _materialName = string.Empty;
    public string MaterialName { get => _materialName; set { if (SetProperty(ref _materialName, value)) RaiseSaveCanExecuteChanged(); } }

    private string _batchNo = string.Empty;
    public string BatchNo { get => _batchNo; set { if (SetProperty(ref _batchNo, value)) RaiseSaveCanExecuteChanged(); } }

    private decimal _quantity;
    public decimal Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    private decimal _safetyStock;
    public decimal SafetyStock { get => _safetyStock; set => SetProperty(ref _safetyStock, value); }

    private string _unit = string.Empty;
    public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

    private DateTime? _inboundDate = DateTime.Today;
    public DateTime? InboundDate { get => _inboundDate; set => SetProperty(ref _inboundDate, value); }

    private DateTime? _expiryDate;
    public DateTime? ExpiryDate { get => _expiryDate; set => SetProperty(ref _expiryDate, value); }

    private string _location = string.Empty;
    public string Location { get => _location; set => SetProperty(ref _location, value); }

    private int _wellRow;
    public int WellRow { get => _wellRow; set => SetProperty(ref _wellRow, value); }

    private int _wellColumn;
    public int WellColumn { get => _wellColumn; set => SetProperty(ref _wellColumn, value); }

    private Guid? _shelfSlotId;
    public Guid? ShelfSlotId { get => _shelfSlotId; set => SetProperty(ref _shelfSlotId, value); }

    private string _remark = string.Empty;
    public string Remark { get => _remark; set => SetProperty(ref _remark, value); }

    private MaterialDto? _selectedMaterial;
    public MaterialDto? SelectedMaterial
    {
        get => _selectedMaterial;
        set
        {
            if (SetProperty(ref _selectedMaterial, value) && value is not null)
            {
                MaterialId = value.Id;
                MaterialCode = value.MaterialCode;
                MaterialName = value.Name;
                Unit = value.Unit;
            }
        }
    }

    public ObservableCollection<MaterialDto> MaterialOptions { get; } = new();

    public InventoryEditDialogViewModel(IInventoryAppService svc, IMaterialAppService materialSvc)
    {
        _svc = svc;
        _materialSvc = materialSvc;
        Title = "库存记录";
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        var id = parameters.GetValue<Guid?>("id");
        _ = LoadAsync(id);
    }

    public async Task LoadAsync(Guid? id)
    {
        var materials = await _materialSvc.GetListAsync();
        MaterialOptions.Clear();
        foreach (var m in materials) MaterialOptions.Add(m);

        if (id is null)
        {
            Id = Guid.Empty;
            MaterialId = Guid.Empty;
            MaterialCode = string.Empty;
            MaterialName = string.Empty;
            BatchNo = string.Empty;
            Quantity = 0;
            SafetyStock = 0;
            Unit = string.Empty;
            InboundDate = DateTime.Today;
            ExpiryDate = null;
            Location = string.Empty;
            WellRow = 0;
            WellColumn = 0;
            ShelfSlotId = null;
            Remark = string.Empty;
            SelectedMaterial = null;
            return;
        }

        var item = await _svc.GetAsync(id.Value);
        if (item is null) { Id = id.Value; return; }

        Id = item.Id;
        MaterialId = item.MaterialId;
        MaterialCode = item.MaterialCode;
        MaterialName = item.MaterialName;
        BatchNo = item.BatchNo;
        Quantity = item.Quantity;
        SafetyStock = item.SafetyStock;
        Unit = item.Unit;
        InboundDate = item.InboundDate;
        ExpiryDate = item.ExpiryDate;
        Location = item.Location;
        WellRow = item.WellRow;
        WellColumn = item.WellColumn;
        ShelfSlotId = item.ShelfSlotId;
        Remark = item.Remark;
        SelectedMaterial = MaterialOptions.FirstOrDefault(m => m.Id == item.MaterialId);
    }

    protected override bool CanSave()
        => !string.IsNullOrWhiteSpace(MaterialName) && !string.IsNullOrWhiteSpace(BatchNo);

    protected override async Task OnSaveAsync()
    {
        var dto = new InventoryRecordDto(
            Id, MaterialId, MaterialCode.Trim(), MaterialName.Trim(),
            BatchNo.Trim(), Quantity, SafetyStock, Unit.Trim(),
            InboundDate, ExpiryDate, Location.Trim(),
            WellRow, WellColumn, ShelfSlotId, Remark.Trim());

        if (Id == Guid.Empty)
            _ = await _svc.CreateAsync(dto);
        else
            _ = await _svc.UpdateAsync(dto);

        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }

    protected override void OnCancel() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
}

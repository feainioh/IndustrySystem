using System;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class InventoryOutboundDialogViewModel : DialogViewModel
{
    private readonly IInventoryAppService _svc;
    private Guid _recordId;

    private string _materialName = string.Empty;
    public string MaterialName { get => _materialName; set => SetProperty(ref _materialName, value); }

    private string _batchNo = string.Empty;
    public string BatchNo { get => _batchNo; set => SetProperty(ref _batchNo, value); }

    private string _unit = string.Empty;
    public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

    private decimal _currentStock;
    public decimal CurrentStock { get => _currentStock; set => SetProperty(ref _currentStock, value); }

    private decimal _outboundQty = 1;
    public decimal OutboundQty
    {
        get => _outboundQty;
        set
        {
            if (SetProperty(ref _outboundQty, value))
                RaiseSaveCanExecuteChanged();
        }
    }

    public InventoryOutboundDialogViewModel(IInventoryAppService svc)
    {
        _svc = svc;
        Title = "出库";
    }

    public async Task LoadAsync(Guid id)
    {
        var item = await _svc.GetAsync(id);
        if (item is null) return;

        _recordId = item.Id;
        MaterialName = item.MaterialName;
        BatchNo = item.BatchNo;
        Unit = item.Unit;
        CurrentStock = item.Quantity;
        OutboundQty = 1;
    }

    protected override bool CanSave()
        => OutboundQty > 0 && OutboundQty <= CurrentStock;

    protected override async Task OnSaveAsync()
    {
        await _svc.OutboundAsync(_recordId, OutboundQty);
        DialogResult = true;
    }

    protected override void OnCancel() => DialogResult = false;
}

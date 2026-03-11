using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class SlotConfigDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _shelfSvc;
    private readonly IInventoryAppService _invSvc;

    public Guid SlotId { get; set; }
    public string PositionLabel { get; set; } = string.Empty;

    private bool _isDisabled;
    public bool IsDisabled { get => _isDisabled; set => SetProperty(ref _isDisabled, value); }

    private string _remark = string.Empty;
    public string Remark { get => _remark; set => SetProperty(ref _remark, value); }

    private ContainerInfoDto? _selectedContainer;
    public ContainerInfoDto? SelectedContainer { get => _selectedContainer; set => SetProperty(ref _selectedContainer, value); }

    private InventoryRecordDto? _selectedInventory;
    public InventoryRecordDto? SelectedInventory { get => _selectedInventory; set => SetProperty(ref _selectedInventory, value); }

    public ObservableCollection<ContainerInfoDto> ContainerOptions { get; } = new();
    public ObservableCollection<InventoryRecordDto> InventoryOptions { get; } = new();

    public SlotConfigDialogViewModel(IShelfAppService shelfSvc, IInventoryAppService invSvc)
    {
        _shelfSvc = shelfSvc;
        _invSvc = invSvc;
        Title = "槽位配置";
    }

    public async Task LoadAsync(SlotDisplayItem slot)
    {
        SlotId = slot.Id;
        PositionLabel = slot.PositionLabel;
        IsDisabled = slot.IsDisabled;
        Remark = slot.Remark;

        var containers = await _shelfSvc.GetContainerListAsync();
        ContainerOptions.Clear();
        foreach (var c in containers) ContainerOptions.Add(c);
        SelectedContainer = slot.ContainerId.HasValue
            ? ContainerOptions.FirstOrDefault(c => c.Id == slot.ContainerId.Value)
            : null;

        var invList = await _invSvc.GetListAsync();
        InventoryOptions.Clear();
        foreach (var i in invList) InventoryOptions.Add(i);
        SelectedInventory = slot.InventoryRecordId.HasValue
            ? InventoryOptions.FirstOrDefault(i => i.Id == slot.InventoryRecordId.Value)
            : null;

        RaisePropertyChanged(nameof(PositionLabel));
    }

    protected override async Task OnSaveAsync()
    {
        var dto = new ShelfSlotDto(
            SlotId, Guid.Empty, 0, 0,
            SelectedContainer?.Id, SelectedInventory?.Id,
            IsDisabled, Remark,
            null, null, null, null,
            null, null, null);
        await _shelfSvc.SaveSlotsAsync(Guid.Empty, [dto]);
        DialogResult = true;
    }

    protected override void OnCancel() => DialogResult = false;
}

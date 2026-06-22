using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>单个槽位的显示模型</summary>
public class SlotDisplayItem : BindableBase
{
    private static string T(string key) => LocalizationProvider.Instance[key];

    public Guid Id { get; set; }
    public Guid ShelfId { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    private Guid? _containerId;
    public Guid? ContainerId
    {
        get => _containerId;
        set
        {
            if (SetProperty(ref _containerId, value))
            {
                RaisePropertyChanged(nameof(HasContainer));
                RaisePropertyChanged(nameof(IsEmpty));
            }
        }
    }

    private IReadOnlyList<ContainerType> _allowedContainerTypes = [];
    public IReadOnlyList<ContainerType> AllowedContainerTypes { get => _allowedContainerTypes; set => SetProperty(ref _allowedContainerTypes, value); }

    private Guid? _inventoryRecordId;
    public Guid? InventoryRecordId { get => _inventoryRecordId; set => SetProperty(ref _inventoryRecordId, value); }

    private bool _isDisabled;
    public bool IsDisabled
    {
        get => _isDisabled;
        set
        {
            if (SetProperty(ref _isDisabled, value))
            {
                RaisePropertyChanged(nameof(IsEmpty));
                RaisePropertyChanged(nameof(StatusLine1));
            }
        }
    }

    private string _remark = string.Empty;
    public string Remark { get => _remark; set => SetProperty(ref _remark, value); }

    private string? _containerName;
    public string? ContainerName
    {
        get => _containerName;
        set
        {
            if (SetProperty(ref _containerName, value))
                RaisePropertyChanged(nameof(StatusLine1));
        }
    }

    private ContainerType? _containerType;
    public ContainerType? ContainerTypeValue { get => _containerType; set => SetProperty(ref _containerType, value); }

    private int? _containerRows;
    public int? ContainerRows { get => _containerRows; set => SetProperty(ref _containerRows, value); }

    private int? _containerColumns;
    public int? ContainerColumns { get => _containerColumns; set => SetProperty(ref _containerColumns, value); }

    private string? _materialName;
    public string? MaterialName
    {
        get => _materialName;
        set
        {
            if (SetProperty(ref _materialName, value))
                RaisePropertyChanged(nameof(StatusLine2));
        }
    }

    private decimal? _quantity;
    public decimal? Quantity
    {
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
                RaisePropertyChanged(nameof(QuantityText));
        }
    }

    private string? _unit;
    public string? Unit
    {
        get => _unit;
        set
        {
            if (SetProperty(ref _unit, value))
                RaisePropertyChanged(nameof(QuantityText));
        }
    }

    private int _inventoryRecordCount;
    public int InventoryRecordCount
    {
        get => _inventoryRecordCount;
        set
        {
            if (SetProperty(ref _inventoryRecordCount, value))
            {
                RaisePropertyChanged(nameof(HasInventory));
                RaisePropertyChanged(nameof(IsEmpty));
                RaisePropertyChanged(nameof(QuantityText));
            }
        }
    }

    public string PositionLabel => $"R{Row}C{Column}";

    public bool HasContainer => ContainerId.HasValue;
    public bool HasInventory => InventoryRecordCount > 0;
    public bool IsEmpty => !IsDisabled && !HasContainer && !HasInventory;

    public string ContainerSizeLabel => ContainerRows.HasValue && ContainerColumns.HasValue
        ? $"{ContainerRows}×{ContainerColumns}" : string.Empty;

    public string StatusLine1
    {
        get
        {
            if (IsDisabled) return T("Legend_Disabled");
            if (ContainerName is not null) return ContainerName;
            return T("Legend_EmptySlot");
        }
    }

    public string StatusLine2
    {
        get
        {
            if (MaterialName is not null) return $"{MaterialName}";
            return string.Empty;
        }
    }

    public string QuantityText
    {
        get
        {
            if (!Quantity.HasValue) return string.Empty;
            var text = $"{Quantity:0.##} {Unit}";
            if (InventoryRecordCount > 1) text += string.Format(T("ShelfSlot_RecordCountSuffixFormat"), InventoryRecordCount);
            return text;
        }
    }

    /// <summary>容器孔位网格模型（用于在槽位中渲染mini网格）</summary>
    public ObservableCollection<WellDisplayItem> WellItems { get; } = new();

    /// <summary>构建容器的孔位网格</summary>
    public void BuildWellGrid(IReadOnlyList<WellOccupancyDto> occupiedWells)
    {
        WellItems.Clear();
        if (!ContainerRows.HasValue || !ContainerColumns.HasValue
            || ContainerRows.Value <= 0 || ContainerColumns.Value <= 0)
            return;

        for (int r = 1; r <= ContainerRows.Value; r++)
        {
            for (int c = 1; c <= ContainerColumns.Value; c++)
            {
                var occ = occupiedWells.FirstOrDefault(w => w.WellRow == r && w.WellColumn == c);
                WellItems.Add(new WellDisplayItem
                {
                    WellRow = r,
                    WellColumn = c,
                    IsOccupied = occ is not null,
                    MaterialName = occ?.MaterialName,
                    Quantity = occ?.Quantity,
                    Unit = occ?.Unit
                });
            }
        }
    }
}

/// <summary>容器内单个孔位的显示模型</summary>
public class WellDisplayItem : BindableBase
{
    public int WellRow { get; set; }
    public int WellColumn { get; set; }

    private bool _isOccupied;
    public bool IsOccupied { get => _isOccupied; set => SetProperty(ref _isOccupied, value); }

    private string? _materialName;
    public string? MaterialName { get => _materialName; set => SetProperty(ref _materialName, value); }

    private decimal? _quantity;
    public decimal? Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    private string? _unit;
    public string? Unit { get => _unit; set => SetProperty(ref _unit, value); }

    public string Label => $"{(char)('A' + WellRow - 1)}{WellColumn}";

    public string TooltipText => IsOccupied
        ? $"{Label}: {MaterialName} ({Quantity:0.##}{Unit})"
        : string.Format(LocalizationProvider.Instance["ShelfSlot_EmptyValueFormat"], Label);
}

public class ShelfInfoViewModel : NavigationViewModel
{
    private readonly IShelfAppService _svc;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _autoRefreshTimer;
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(10);

    // ── 容器弹窗（通过 IDialogService 打开）──
    public ICommand OpenContainerListCommand { get; }

    // ── 货架弹窗（通过 IDialogService 打开）──
    public ICommand OpenShelfListCommand { get; }

    // ── 货架显示 ──
    private ShelfConfigDto? _selectedShelf;
    public ShelfConfigDto? SelectedShelf
    {
        get => _selectedShelf;
        set
        {
            if (SetProperty(ref _selectedShelf, value))
                _ = LoadSlotsAsync();
        }
    }

    private int _shelfRows;
    public int ShelfRows { get => _shelfRows; set => SetProperty(ref _shelfRows, value); }

    private int _shelfColumns;
    public int ShelfColumns { get => _shelfColumns; set => SetProperty(ref _shelfColumns, value); }

    public ObservableCollection<SlotDisplayItem> Slots { get; } = new();

    public ICommand ConfigureSlotCommand { get; }

    public ShelfInfoViewModel(IShelfAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;

        OpenContainerListCommand = new DelegateCommand(() => OpenContainerListDialogAsync());
        OpenShelfListCommand = new DelegateCommand(() => OpenShelfListDialogAsync());

        ConfigureSlotCommand = new DelegateCommand<SlotDisplayItem>(slot => { if (slot is not null) OpenSlotConfigAsync(slot); });

        // Auto-refresh timer to poll inventory changes
        _autoRefreshTimer = new DispatcherTimer { Interval = AutoRefreshInterval };
        _autoRefreshTimer.Tick += async (_, _) => await RefreshSlotsQuietAsync();
        _autoRefreshTimer.Start();

        _ = OnRefreshAsync();
    }

    protected override async Task OnRefreshAsync()
    {
        await LoadSlotsAsync();
    }

    private async Task LoadSlotsAsync()
    {
        Slots.Clear();
        if (SelectedShelf is null) { ShelfRows = 0; ShelfColumns = 0; return; }

        ShelfRows = SelectedShelf.Rows;
        ShelfColumns = SelectedShelf.Columns;

        var list = await _svc.GetSlotsByShelfAsync(SelectedShelf.Id);
        foreach (var dto in list)
        {
            var item = new SlotDisplayItem
            {
                Id = dto.Id,
                ShelfId = dto.ShelfId,
                Row = dto.Row,
                Column = dto.Column,
                AllowedContainerTypes = dto.AllowedContainerTypes,
                ContainerId = dto.ContainerId,
                InventoryRecordId = dto.InventoryRecordId,
                IsDisabled = dto.IsDisabled,
                Remark = dto.Remark,
                ContainerName = dto.ContainerName,
                ContainerTypeValue = dto.ContainerTypeValue,
                ContainerRows = dto.ContainerRows,
                ContainerColumns = dto.ContainerColumns,
                MaterialName = dto.MaterialName,
                Quantity = dto.Quantity,
                Unit = dto.Unit,
                InventoryRecordCount = dto.InventoryRecordCount
            };
            item.BuildWellGrid(dto.OccupiedWells);
            Slots.Add(item);
        }
    }

    /// <summary>
    /// Quiet refresh: re-query slots from service and update well occupancy
    /// in-place without rebuilding the full Slots collection (avoids UI flicker).
    /// </summary>
    private async Task RefreshSlotsQuietAsync()
    {
        if (SelectedShelf is null) return;

        try
        {
            var list = await _svc.GetSlotsByShelfAsync(SelectedShelf.Id);
            var dtoMap = list.ToDictionary(d => d.Id);

            foreach (var slot in Slots)
            {
                if (!dtoMap.TryGetValue(slot.Id, out var dto)) continue;

                slot.ContainerId = dto.ContainerId;
                slot.AllowedContainerTypes = dto.AllowedContainerTypes;
                slot.ContainerName = dto.ContainerName;
                slot.ContainerTypeValue = dto.ContainerTypeValue;
                slot.ContainerRows = dto.ContainerRows;
                slot.ContainerColumns = dto.ContainerColumns;
                slot.IsDisabled = dto.IsDisabled;
                slot.Remark = dto.Remark;
                slot.InventoryRecordId = dto.InventoryRecordId;
                slot.MaterialName = dto.MaterialName;
                slot.Quantity = dto.Quantity;
                slot.Unit = dto.Unit;
                slot.InventoryRecordCount = dto.InventoryRecordCount;

                slot.BuildWellGrid(dto.OccupiedWells);
            }
        }
        catch
        {
            // Swallow errors during quiet refresh to avoid disrupting the UI
        }
    }

    // ── 容器列表弹窗（通过 IDialogService）──

    private void OpenContainerListDialogAsync()
    {
        _dialogService.ShowDialog(nameof(Views.Dialogs.ContainerListDialog), _ => { });
    }

    // ── 货架列表弹窗（通过 IDialogService）──

    private void OpenShelfListDialogAsync()
    {
        _dialogService.ShowDialog(nameof(Views.Dialogs.ShelfListDialog), _ => { });
    }

    // ── 槽位配置 ──

    private void OpenSlotConfigAsync(SlotDisplayItem slot)
    {
        var parameters = new DialogParameters { { "slot", slot } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.SlotConfigDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadSlotsAsync();
        });
    }

}

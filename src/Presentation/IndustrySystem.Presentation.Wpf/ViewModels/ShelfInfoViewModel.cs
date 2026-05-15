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
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
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

public class ShelfInfoViewModel : NagetiveViewModel
{
    private readonly IShelfAppService _svc;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _autoRefreshTimer;
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(10);

    // Track whether a list dialog is currently open so nested actions
    // (add/edit/delete) can close it first and re-open afterwards.
    private bool _containerListOpen;
    private bool _shelfListOpen;

    // ── 容器管理 ──
    public ObservableCollection<ContainerInfoDto> Containers { get; } = new();
    public ObservableCollection<ContainerInfoDto> PagedContainers { get; } = new();

    public ObservableCollection<int> ContainerPageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _containerCurrentPage = 1;
    public int ContainerCurrentPage
    {
        get => _containerCurrentPage;
        set
        {
            if (SetProperty(ref _containerCurrentPage, value))
            {
                ApplyContainerPaging();
            }
        }
    }

    private int _containerPageSize = 20;
    public int ContainerPageSize
    {
        get => _containerPageSize;
        set
        {
            if (SetProperty(ref _containerPageSize, value))
            {
                ApplyContainerPaging(resetToFirstPage: true);
            }
        }
    }

    private int _containerTotalCount;
    public int ContainerTotalCount
    {
        get => _containerTotalCount;
        private set
        {
            if (SetProperty(ref _containerTotalCount, value))
            {
                RaisePropertyChanged(nameof(ContainerTotalPages));
            }
        }
    }

    public int ContainerTotalPages => Math.Max(1, (int)Math.Ceiling(ContainerTotalCount / (double)ContainerPageSize));

    public ICommand AddContainerCommand { get; }
    public ICommand EditContainerCommand { get; }
    public ICommand DeleteContainerCommand { get; }
    public ICommand OpenContainerListCommand { get; }
    public ICommand ContainerFirstPageCommand { get; }
    public ICommand ContainerPreviousPageCommand { get; }
    public ICommand ContainerNextPageCommand { get; }
    public ICommand ContainerLastPageCommand { get; }

    // ── 货架配置 ──
    public ObservableCollection<ShelfConfigDto> Shelves { get; } = new();
    public ObservableCollection<ShelfConfigDto> PagedShelves { get; } = new();

    public ObservableCollection<int> ShelfPageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _shelfCurrentPage = 1;
    public int ShelfCurrentPage
    {
        get => _shelfCurrentPage;
        set
        {
            if (SetProperty(ref _shelfCurrentPage, value))
            {
                ApplyShelfPaging();
            }
        }
    }

    private int _shelfPageSize = 20;
    public int ShelfPageSize
    {
        get => _shelfPageSize;
        set
        {
            if (SetProperty(ref _shelfPageSize, value))
            {
                ApplyShelfPaging(resetToFirstPage: true);
            }
        }
    }

    private int _shelfTotalCount;
    public int ShelfTotalCount
    {
        get => _shelfTotalCount;
        private set
        {
            if (SetProperty(ref _shelfTotalCount, value))
            {
                RaisePropertyChanged(nameof(ShelfTotalPages));
            }
        }
    }

    public int ShelfTotalPages => Math.Max(1, (int)Math.Ceiling(ShelfTotalCount / (double)ShelfPageSize));

    public ICommand AddShelfCommand { get; }
    public ICommand EditShelfCommand { get; }
    public ICommand DeleteShelfCommand { get; }
    public ICommand OpenShelfListCommand { get; }
    public ICommand ShelfFirstPageCommand { get; }
    public ICommand ShelfPreviousPageCommand { get; }
    public ICommand ShelfNextPageCommand { get; }
    public ICommand ShelfLastPageCommand { get; }

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

        AddContainerCommand = new DelegateCommand(async () => await OpenContainerDialogAsync(null));
        EditContainerCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await OpenContainerDialogAsync(id.Value); });
        DeleteContainerCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await DeleteContainerAsync(id.Value); });
        OpenContainerListCommand = new DelegateCommand(async () => await OpenContainerListDialogAsync());

        ContainerFirstPageCommand = new DelegateCommand(() => SetContainerPage(1), () => ContainerCurrentPage > 1);
        ContainerPreviousPageCommand = new DelegateCommand(() => SetContainerPage(ContainerCurrentPage - 1), () => ContainerCurrentPage > 1);
        ContainerNextPageCommand = new DelegateCommand(() => SetContainerPage(ContainerCurrentPage + 1), () => ContainerCurrentPage < ContainerTotalPages);
        ContainerLastPageCommand = new DelegateCommand(() => SetContainerPage(ContainerTotalPages), () => ContainerCurrentPage < ContainerTotalPages);

        AddShelfCommand = new DelegateCommand(async () => await OpenShelfDialogAsync(null));
        EditShelfCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await OpenShelfDialogAsync(id.Value); });
        DeleteShelfCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await DeleteShelfAsync(id.Value); });
        OpenShelfListCommand = new DelegateCommand(async () => await OpenShelfListDialogAsync());

        ShelfFirstPageCommand = new DelegateCommand(() => SetShelfPage(1), () => ShelfCurrentPage > 1);
        ShelfPreviousPageCommand = new DelegateCommand(() => SetShelfPage(ShelfCurrentPage - 1), () => ShelfCurrentPage > 1);
        ShelfNextPageCommand = new DelegateCommand(() => SetShelfPage(ShelfCurrentPage + 1), () => ShelfCurrentPage < ShelfTotalPages);
        ShelfLastPageCommand = new DelegateCommand(() => SetShelfPage(ShelfTotalPages), () => ShelfCurrentPage < ShelfTotalPages);

        ConfigureSlotCommand = new DelegateCommand<SlotDisplayItem>(slot => { if (slot is not null) OpenSlotConfigAsync(slot); });

        // Auto-refresh timer to poll inventory changes
        _autoRefreshTimer = new DispatcherTimer { Interval = AutoRefreshInterval };
        _autoRefreshTimer.Tick += async (_, _) => await RefreshSlotsQuietAsync();
        _autoRefreshTimer.Start();

        _ = OnRefreshAsync();
    }

    protected override async Task OnRefreshAsync()
    {
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        await LoadContainersAsync();
        await LoadShelvesAsync();
    }

    private async Task LoadContainersAsync()
    {
        var list = await _svc.GetContainerListAsync();
        Containers.Clear();
        foreach (var c in list) Containers.Add(c);

        ApplyContainerPaging();
    }

    private async Task LoadShelvesAsync()
    {
        var prevSelectedId = SelectedShelf?.Id;
        var list = await _svc.GetShelfListAsync();
        Shelves.Clear();
        foreach (var s in list) Shelves.Add(s);

        ApplyShelfPaging();

        // Try to re-select previously selected, or first
        var toSelect = prevSelectedId.HasValue ? Shelves.FirstOrDefault(s => s.Id == prevSelectedId.Value) : null;
        SelectedShelf = toSelect ?? (Shelves.Count > 0 ? Shelves[0] : null);
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

    // ── Helper: close the current list dialog if one is open ──

    private void CloseListDialogIfOpen()
    {
        if (_containerListOpen || _shelfListOpen)
        {
            try { DialogHost.Close("RootDialogHost"); } catch { }
        }
    }

    /// <summary>
    /// Quiet refresh: re-query slots from service and update well occupancy
    /// in-place without rebuilding the full Slots collection (avoids UI flicker).
    /// Skips if a dialog is open to prevent conflicts.
    /// </summary>
    private async Task RefreshSlotsQuietAsync()
    {
        if (_containerListOpen || _shelfListOpen) return;
        if (SelectedShelf is null) return;

        try
        {
            var list = await _svc.GetSlotsByShelfAsync(SelectedShelf.Id);
            var dtoMap = list.ToDictionary(d => d.Id);

            foreach (var slot in Slots)
            {
                if (!dtoMap.TryGetValue(slot.Id, out var dto)) continue;

                // Update container info
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

                // Rebuild well grid
                slot.BuildWellGrid(dto.OccupiedWells);
            }
        }
        catch
        {
            // Swallow errors during quiet refresh to avoid disrupting the UI
        }
    }

    // ── 容器弹窗 ──

    private async Task OpenContainerDialogAsync(Guid? id)
    {
        // If invoked from inside the list dialog, close it first
        var wasListOpen = _containerListOpen;
        if (wasListOpen) CloseListDialogIfOpen();

        // Small delay to allow the dialog host to finish closing
        if (wasListOpen) await Task.Delay(50);

        var parameters = new DialogParameters();
        if (id.HasValue)
            parameters.Add("id", id.Value);
        _dialogService.ShowDialog(nameof(Views.Dialogs.ContainerEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadContainersAsync();

            // Re-open the list dialog if it was previously open
            if (wasListOpen)
                await OpenContainerListDialogAsync();
        });
    }

    private async Task DeleteContainerAsync(Guid id)
    {
        var r = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;
        await _svc.DeleteContainerAsync(id);
        await LoadContainersAsync();
    }

    private async Task OpenContainerListDialogAsync()
    {
        _containerListOpen = true;
        try
        {
            var dialog = new Views.Dialogs.ContainerListDialog { DataContext = this };
            await DialogHost.Show(dialog, "RootDialogHost");
        }
        finally { _containerListOpen = false; }
        await LoadContainersAsync();
    }

    // ── 货架弹窗 ──

    private async Task OpenShelfDialogAsync(Guid? id)
    {
        // If invoked from inside the list dialog, close it first
        var wasListOpen = _shelfListOpen;
        if (wasListOpen) CloseListDialogIfOpen();

        if (wasListOpen) await Task.Delay(50);

        var parameters = new DialogParameters();
        if (id.HasValue)
            parameters.Add("id", id.Value);
        _dialogService.ShowDialog(nameof(Views.Dialogs.ShelfEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadShelvesAsync();

            // Re-open the list dialog if it was previously open
            if (wasListOpen)
                await OpenShelfListDialogAsync();
        });
    }

    private async Task DeleteShelfAsync(Guid id)
    {
        var r = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;
        await _svc.DeleteShelfAsync(id);
        if (SelectedShelf?.Id == id) SelectedShelf = null;
        await LoadShelvesAsync();
    }

    private async Task OpenShelfListDialogAsync()
    {
        _shelfListOpen = true;
        try
        {
            var dialog = new Views.Dialogs.ShelfListDialog { DataContext = this };
            await DialogHost.Show(dialog, "RootDialogHost");
        }
        finally { _shelfListOpen = false; }
        await LoadShelvesAsync();
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

    private void ApplyContainerPaging(bool resetToFirstPage = false)
    {
        if (ContainerPageSize <= 0)
        {
            ContainerPageSize = 1;
        }

        ContainerTotalCount = Containers.Count;

        if (resetToFirstPage)
        {
            _containerCurrentPage = 1;
            RaisePropertyChanged(nameof(ContainerCurrentPage));
        }

        var totalPages = ContainerTotalPages;
        if (_containerCurrentPage > totalPages)
        {
            _containerCurrentPage = totalPages;
            RaisePropertyChanged(nameof(ContainerCurrentPage));
        }
        if (_containerCurrentPage < 1)
        {
            _containerCurrentPage = 1;
            RaisePropertyChanged(nameof(ContainerCurrentPage));
        }

        PagedContainers.Clear();
        foreach (var container in Containers.Skip((_containerCurrentPage - 1) * ContainerPageSize).Take(ContainerPageSize))
        {
            PagedContainers.Add(container);
        }

        RaiseContainerPagingCommandStates();
    }

    private void RaiseContainerPagingCommandStates()
    {
        (ContainerFirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerPreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerNextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerLastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    private void SetContainerPage(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (page > ContainerTotalPages)
        {
            page = ContainerTotalPages;
        }

        ContainerCurrentPage = page;
    }

    private void ApplyShelfPaging(bool resetToFirstPage = false)
    {
        if (ShelfPageSize <= 0)
        {
            ShelfPageSize = 1;
        }

        ShelfTotalCount = Shelves.Count;

        if (resetToFirstPage)
        {
            _shelfCurrentPage = 1;
            RaisePropertyChanged(nameof(ShelfCurrentPage));
        }

        var totalPages = ShelfTotalPages;
        if (_shelfCurrentPage > totalPages)
        {
            _shelfCurrentPage = totalPages;
            RaisePropertyChanged(nameof(ShelfCurrentPage));
        }
        if (_shelfCurrentPage < 1)
        {
            _shelfCurrentPage = 1;
            RaisePropertyChanged(nameof(ShelfCurrentPage));
        }

        PagedShelves.Clear();
        foreach (var shelf in Shelves.Skip((_shelfCurrentPage - 1) * ShelfPageSize).Take(ShelfPageSize))
        {
            PagedShelves.Add(shelf);
        }

        RaiseShelfPagingCommandStates();
    }

    private void RaiseShelfPagingCommandStates()
    {
        (ShelfFirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfPreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfNextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfLastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    private void SetShelfPage(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (page > ShelfTotalPages)
        {
            page = ShelfTotalPages;
        }

        ShelfCurrentPage = page;
    }
}

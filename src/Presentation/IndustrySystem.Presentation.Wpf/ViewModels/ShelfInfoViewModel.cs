using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

/// <summary>单个槽位的显示模型</summary>
public class SlotDisplayItem : BindableBase
{
    public Guid Id { get; set; }
    public Guid ShelfId { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    private Guid? _containerId;
    public Guid? ContainerId { get => _containerId; set => SetProperty(ref _containerId, value); }

    private Guid? _inventoryRecordId;
    public Guid? InventoryRecordId { get => _inventoryRecordId; set => SetProperty(ref _inventoryRecordId, value); }

    private bool _isDisabled;
    public bool IsDisabled { get => _isDisabled; set => SetProperty(ref _isDisabled, value); }

    private string _remark = string.Empty;
    public string Remark { get => _remark; set => SetProperty(ref _remark, value); }

    private string? _containerName;
    public string? ContainerName { get => _containerName; set => SetProperty(ref _containerName, value); }

    private ContainerType? _containerType;
    public ContainerType? ContainerTypeValue { get => _containerType; set => SetProperty(ref _containerType, value); }

    private int? _containerRows;
    public int? ContainerRows { get => _containerRows; set => SetProperty(ref _containerRows, value); }

    private int? _containerColumns;
    public int? ContainerColumns { get => _containerColumns; set => SetProperty(ref _containerColumns, value); }

    private string? _materialName;
    public string? MaterialName { get => _materialName; set => SetProperty(ref _materialName, value); }

    private decimal? _quantity;
    public decimal? Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }

    private string? _unit;
    public string? Unit { get => _unit; set => SetProperty(ref _unit, value); }

    public string PositionLabel => $"R{Row}C{Column}";

    public bool HasContainer => ContainerId.HasValue;
    public bool HasInventory => InventoryRecordId.HasValue;
    public bool IsEmpty => !IsDisabled && !HasContainer && !HasInventory;

    public string ContainerSizeLabel => ContainerRows.HasValue && ContainerColumns.HasValue
        ? $"{ContainerRows}×{ContainerColumns}" : string.Empty;

    public string StatusLine1
    {
        get
        {
            if (IsDisabled) return "已禁用";
            if (ContainerName is not null) return ContainerName;
            return "空位";
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
            if (Quantity.HasValue) return $"{Quantity:0.##} {Unit}";
            return string.Empty;
        }
    }
}

public class ShelfInfoViewModel : BindableBase
{
    private readonly IShelfAppService _svc;

    // Track whether a list dialog is currently open so nested actions
    // (add/edit/delete) can close it first and re-open afterwards.
    private bool _containerListOpen;
    private bool _shelfListOpen;

    // ── 容器管理 ──
    public ObservableCollection<ContainerInfoDto> Containers { get; } = new();
    public ICommand AddContainerCommand { get; }
    public ICommand EditContainerCommand { get; }
    public ICommand DeleteContainerCommand { get; }
    public ICommand OpenContainerListCommand { get; }

    // ── 货架配置 ──
    public ObservableCollection<ShelfConfigDto> Shelves { get; } = new();
    public ICommand AddShelfCommand { get; }
    public ICommand EditShelfCommand { get; }
    public ICommand DeleteShelfCommand { get; }
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

    public ICommand RefreshCommand { get; }
    public ICommand ConfigureSlotCommand { get; }

    public ShelfInfoViewModel(IShelfAppService svc)
    {
        _svc = svc;

        RefreshCommand = new DelegateCommand(async () => await LoadAllAsync());

        AddContainerCommand = new DelegateCommand(async () => await OpenContainerDialogAsync(null));
        EditContainerCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await OpenContainerDialogAsync(id.Value); });
        DeleteContainerCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await DeleteContainerAsync(id.Value); });
        OpenContainerListCommand = new DelegateCommand(async () => await OpenContainerListDialogAsync());

        AddShelfCommand = new DelegateCommand(async () => await OpenShelfDialogAsync(null));
        EditShelfCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await OpenShelfDialogAsync(id.Value); });
        DeleteShelfCommand = new DelegateCommand<Guid?>(async id => { if (id.HasValue) await DeleteShelfAsync(id.Value); });
        OpenShelfListCommand = new DelegateCommand(async () => await OpenShelfListDialogAsync());

        ConfigureSlotCommand = new DelegateCommand<SlotDisplayItem>(async slot => { if (slot is not null) await OpenSlotConfigAsync(slot); });

        _ = LoadAllAsync();
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
    }

    private async Task LoadShelvesAsync()
    {
        var prevSelectedId = SelectedShelf?.Id;
        var list = await _svc.GetShelfListAsync();
        Shelves.Clear();
        foreach (var s in list) Shelves.Add(s);

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
            Slots.Add(new SlotDisplayItem
            {
                Id = dto.Id,
                ShelfId = dto.ShelfId,
                Row = dto.Row,
                Column = dto.Column,
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
                Unit = dto.Unit
            });
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

    // ── 容器弹窗 ──

    private async Task OpenContainerDialogAsync(Guid? id)
    {
        // If invoked from inside the list dialog, close it first
        var wasListOpen = _containerListOpen;
        if (wasListOpen) CloseListDialogIfOpen();

        // Small delay to allow the dialog host to finish closing
        if (wasListOpen) await Task.Delay(50);

        var vm = ContainerLocator.Current.Resolve<ContainerEditDialogViewModel>();
        await vm.LoadAsync(id);
        var dialog = new Views.Dialogs.ContainerEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };
        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is true) await LoadContainersAsync();
        }
        finally { vm.PropertyChanged -= handler; }

        // Re-open the list dialog if it was previously open
        if (wasListOpen)
            await OpenContainerListDialogAsync();
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

        var vm = ContainerLocator.Current.Resolve<ShelfEditDialogViewModel>();
        await vm.LoadAsync(id);
        var dialog = new Views.Dialogs.ShelfEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };
        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is true) await LoadShelvesAsync();
        }
        finally { vm.PropertyChanged -= handler; }

        // Re-open the list dialog if it was previously open
        if (wasListOpen)
            await OpenShelfListDialogAsync();
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

    private async Task OpenSlotConfigAsync(SlotDisplayItem slot)
    {
        var vm = ContainerLocator.Current.Resolve<SlotConfigDialogViewModel>();
        await vm.LoadAsync(slot);
        var dialog = new Views.Dialogs.SlotConfigDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };
        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is true) await LoadSlotsAsync();
        }
        finally { vm.PropertyChanged -= handler; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

/// <summary>货架编辑对话框中单个槽位的编辑模型</summary>
public class SlotEditItem : BindableBase
{
    public Guid SlotId { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }

    private bool _isDisabled;
    public bool IsDisabled { get => _isDisabled; set { SetProperty(ref _isDisabled, value); RaisePropertyChanged(nameof(IsEnabled)); } }

    public bool IsEnabled { get => !_isDisabled; set => IsDisabled = !value; }

    /// <summary>允许的容器类型（多选）</summary>
    public ObservableCollection<ContainerTypeOption> AllowedContainerTypeOptions { get; } = new();

    /// <summary>已选中的容器类型列表</summary>
    public List<ContainerType> SelectedContainerTypes
        => AllowedContainerTypeOptions.Where(o => o.IsSelected).Select(o => o.ContainerType).ToList();

    /// <summary>显示标签：不限制 或 逗号分隔的类型名</summary>
    private string _allowedTypesLabel = "不限制";
    public string AllowedTypesLabel { get => _allowedTypesLabel; set => SetProperty(ref _allowedTypesLabel, value); }

    public string PositionLabel => $"R{Row}C{Column}";

    public void InitOptions(IEnumerable<ContainerType> selectedTypes)
    {
        AllowedContainerTypeOptions.Clear();
        var selectedSet = new HashSet<ContainerType>(selectedTypes);
        foreach (ContainerType ct in Enum.GetValues(typeof(ContainerType)))
        {
            var opt = new ContainerTypeOption(ct) { IsSelected = selectedSet.Contains(ct) };
            opt.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ContainerTypeOption.IsSelected))
                    UpdateLabel();
            };
            AllowedContainerTypeOptions.Add(opt);
        }
        UpdateLabel();
    }

    public void ToggleEnabled() => IsDisabled = !IsDisabled;

    public void ToggleContainerType(ContainerType ct)
    {
        var opt = AllowedContainerTypeOptions.FirstOrDefault(o => o.ContainerType == ct);
        if (opt is not null) opt.IsSelected = !opt.IsSelected;
    }

    public void ClearAllContainerTypes()
    {
        foreach (var opt in AllowedContainerTypeOptions) opt.IsSelected = false;
    }

    private void UpdateLabel()
    {
        var selected = SelectedContainerTypes;
        AllowedTypesLabel = selected.Count == 0 ? "不限制" : string.Join(",", selected.Select(ct => ct.ToString()[..2]));
    }
}

public class ShelfEditDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;

    public Guid Id { get; set; }

    /// <summary>左键点击槽位 —— 切换启用/禁用</summary>
    public ICommand ToggleSlotEnabledCommand { get; }

    /// <summary>右键菜单 —— 切换某个容器类型</summary>
    public ICommand ToggleContainerTypeCommand { get; }

    /// <summary>右键菜单 —— 清除所有容器类型限制</summary>
    public ICommand ClearContainerTypesCommand { get; }

    private string _shelfCode = string.Empty;
    public string ShelfCode { get => _shelfCode; set { if (SetProperty(ref _shelfCode, value)) RaiseSaveCanExecuteChanged(); } }

    private string _name = string.Empty;
    public string Name { get => _name; set { if (SetProperty(ref _name, value)) RaiseSaveCanExecuteChanged(); } }

    private int _rows = 4;
    public int Rows
    {
        get => _rows;
        set
        {
            if (SetProperty(ref _rows, value))
            {
                RaiseSaveCanExecuteChanged();
                RebuildSlotGrid();
            }
        }
    }

    private int _columns = 6;
    public int Columns
    {
        get => _columns;
        set
        {
            if (SetProperty(ref _columns, value))
            {
                RaiseSaveCanExecuteChanged();
                RebuildSlotGrid();
            }
        }
    }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    public ObservableCollection<SlotEditItem> SlotEditItems { get; } = new();

    // Track existing slot data loaded from server (edit mode)
    private IReadOnlyList<ShelfSlotDto>? _existingSlots;

    public ShelfEditDialogViewModel(IShelfAppService svc)
    {
        _svc = svc;
        Title = "货架配置";

        ToggleSlotEnabledCommand = new DelegateCommand<SlotEditItem>(slot => slot?.ToggleEnabled());
        ToggleContainerTypeCommand = new DelegateCommand<object>(param =>
        {
            if (param is object[] args && args.Length == 2 && args[0] is SlotEditItem slot && args[1] is ContainerType ct)
                slot.ToggleContainerType(ct);
        });
        ClearContainerTypesCommand = new DelegateCommand<SlotEditItem>(slot => slot?.ClearAllContainerTypes());
    }

    public async Task LoadAsync(Guid? id)
    {
        _existingSlots = null;

        if (id is null)
        {
            Id = Guid.Empty;
            ShelfCode = string.Empty;
            Name = string.Empty;
            _rows = 4;
            _columns = 6;
            RaisePropertyChanged(nameof(Rows));
            RaisePropertyChanged(nameof(Columns));
            Description = string.Empty;
            RebuildSlotGrid();
            return;
        }

        var item = await _svc.GetShelfAsync(id.Value);
        if (item is null) return;
        Id = item.Id;
        ShelfCode = item.ShelfCode;
        Name = item.Name;
        _rows = item.Rows;
        _columns = item.Columns;
        RaisePropertyChanged(nameof(Rows));
        RaisePropertyChanged(nameof(Columns));
        Description = item.Description;

        // Load existing slot data
        _existingSlots = await _svc.GetSlotsByShelfAsync(Id);
        RebuildSlotGrid();
    }

    private void RebuildSlotGrid()
    {
        SlotEditItems.Clear();
        var rows = Math.Max(0, Math.Min(_rows, 20));
        var cols = Math.Max(0, Math.Min(_columns, 20));

        for (int r = 1; r <= rows; r++)
        {
            for (int c = 1; c <= cols; c++)
            {
                var item = new SlotEditItem { Row = r, Column = c };

                // Restore existing slot state if editing
                var existing = _existingSlots?.FirstOrDefault(s => s.Row == r && s.Column == c);
                if (existing is not null)
                {
                    item.SlotId = existing.Id;
                    item.IsDisabled = existing.IsDisabled;
                    item.InitOptions(existing.AllowedContainerTypes);
                }
                else
                {
                    item.InitOptions([]);
                }

                SlotEditItems.Add(item);
            }
        }
    }

    protected override bool CanSave()
        => !string.IsNullOrWhiteSpace(ShelfCode) && !string.IsNullOrWhiteSpace(Name) && Rows > 0 && Columns > 0;

    protected override async Task OnSaveAsync()
    {
        var dto = new ShelfConfigDto(Id, ShelfCode.Trim(), Name.Trim(), Rows, Columns, Description.Trim());

        Guid shelfId;
        if (Id == Guid.Empty)
        {
            var created = await _svc.CreateShelfAsync(dto);
            shelfId = created.Id;
        }
        else
        {
            await _svc.UpdateShelfAsync(dto);
            shelfId = Id;
        }

        // Save slot configurations
        await SaveSlotConfigsAsync(shelfId);

        DialogResult = true;
    }

    private async Task SaveSlotConfigsAsync(Guid shelfId)
    {
        // Load actual slots from DB (they were auto-generated for new shelves)
        var dbSlots = await _svc.GetSlotsByShelfAsync(shelfId);

        var slotDtos = SlotEditItems.Select(item =>
        {
            var dbSlot = dbSlots.FirstOrDefault(s => s.Row == item.Row && s.Column == item.Column);
            if (dbSlot is null) return null;

            return new ShelfSlotDto(
                dbSlot.Id, shelfId, item.Row, item.Column,
                item.SelectedContainerTypes,
                dbSlot.ContainerId, dbSlot.InventoryRecordId,
                item.IsDisabled, dbSlot.Remark,
                null, null, null, null,
                null, null, null, []);
        }).Where(d => d is not null).ToList();

        if (slotDtos.Count > 0)
            await _svc.SaveSlotsAsync(shelfId, slotDtos!);
    }

    protected override void OnCancel() => DialogResult = false;
}

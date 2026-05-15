using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Ioc;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class InventoryViewModel : NagetiveCurdVeiwModel<InventoryRecordDto>
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IInventoryAppService _svc;
    private readonly IDialogService _dialogService;

    // ── Detail records ──
    public new ObservableCollection<InventoryRecordDto> Items { get; } = new();
    public ObservableCollection<InventoryRecordDto> PagedItems { get; } = new();
    public ICollectionView InventoryView { get; }

    // ── Summary (grouped by MaterialId + BatchNo) ──
    public ObservableCollection<InventorySummaryDto> SummaryItems { get; } = new();
    public ObservableCollection<InventorySummaryDto> PagedSummaryItems { get; } = new();
    public ICollectionView SummaryView { get; }

    private bool _showSummary;
    public bool ShowSummary
    {
        get => _showSummary;
        set
        {
            if (SetProperty(ref _showSummary, value))
            {
                ApplyInventoryPaging(resetToFirstPage: true);
            }
        }
    }

    public new ICommand RefreshCommand { get; }
    public ICommand InboundCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand OutboundCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleSummaryCommand { get; }

    public InventoryViewModel(IInventoryAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;
        InventoryView = CollectionViewSource.GetDefaultView(Items);
        InventoryView.Filter = FilterItems;

        SummaryView = CollectionViewSource.GetDefaultView(SummaryItems);
        SummaryView.Filter = FilterSummary;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        InboundCommand = new DelegateCommand(() => OpenInboundDialogAsync());
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenEditDialogAsync(id.Value);
        });
        OutboundCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenOutboundDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });
        ToggleSummaryCommand = new DelegateCommand(() => ShowSummary = !ShowSummary);

        _ = LoadAsync();
    }

    private bool FilterItems(object item)
    {
        if (item is not InventoryRecordDto record) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var key = SearchText.Trim();
         return (record.MaterialCode?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
             || (record.MaterialName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
             || (record.BatchNo?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
             || (record.Location?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private bool FilterSummary(object item)
    {
        if (item is not InventorySummaryDto summary) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var key = SearchText.Trim();
         return (summary.MaterialCode?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
             || (summary.MaterialName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
             || (summary.BatchNo?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync();
        Items.Clear();
        foreach (var item in list)
            Items.Add(item);

        var summaries = await _svc.GetSummaryListAsync();
        SummaryItems.Clear();
        foreach (var s in summaries)
            SummaryItems.Add(s);

        ApplyInventoryPaging(resetToFirstPage: true);
    }

    /// <summary>入库：弹出完整入库弹窗（新建库存记录）</summary>
    private void OpenInboundDialogAsync()
    {
        var parameters = new DialogParameters();
        _dialogService.ShowDialog(nameof(Views.Dialogs.InventoryEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadAsync();
        });
    }

    /// <summary>编辑：弹出编辑弹窗（修改已有库存记录）</summary>
    private void OpenEditDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", id } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.InventoryEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadAsync();
        });
    }

    /// <summary>出库：弹出出库弹窗，自主选择出库数量</summary>
    private void OpenOutboundDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", id } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.InventoryOutboundDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadAsync();
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        await _svc.DeleteAsync(id);
        await LoadAsync();
    }

    protected override async Task<IReadOnlyList<InventoryRecordDto>> LoadItemsAsync()
        => await _svc.GetListAsync();

    protected override void OnSearchTextChanged()
    {
        InventoryView.Refresh();
        SummaryView.Refresh();
        ApplyInventoryPaging(resetToFirstPage: true);
    }

    protected override void OnPagingParametersChanged(bool resetToFirstPage)
    {
        ApplyInventoryPaging(resetToFirstPage);
    }

    private IEnumerable<InventoryRecordDto> BuildFilteredItems()
    {
        var query = Items.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var key = SearchText.Trim();
            query = query.Where(record =>
                (record.MaterialCode?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (record.MaterialName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (record.BatchNo?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (record.Location?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return query;
    }

    private IEnumerable<InventorySummaryDto> BuildFilteredSummaryItems()
    {
        var query = SummaryItems.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var key = SearchText.Trim();
            query = query.Where(summary =>
                (summary.MaterialCode?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (summary.MaterialName?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false)
                || (summary.BatchNo?.Contains(key, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return query;
    }

    private void ApplyInventoryPaging(bool resetToFirstPage = false)
    {
        var filteredItems = BuildFilteredItems().ToList();
        var filteredSummaryItems = BuildFilteredSummaryItems().ToList();
        TotalCount = ShowSummary ? filteredSummaryItems.Count : filteredItems.Count;

        if (resetToFirstPage)
        {
            PageIndex = 0;
        }

        var maxPageIndex = Math.Max(0, TotalPages - 1);
        if (PageIndex > maxPageIndex)
        {
            PageIndex = maxPageIndex;
        }

        PagedItems.Clear();
        foreach (var item in filteredItems.Skip(PageIndex * PageSize).Take(PageSize))
        {
            PagedItems.Add(item);
        }

        PagedSummaryItems.Clear();
        foreach (var summary in filteredSummaryItems.Skip(PageIndex * PageSize).Take(PageSize))
        {
            PagedSummaryItems.Add(summary);
        }

        RaisePagingCommandStates();
    }
}

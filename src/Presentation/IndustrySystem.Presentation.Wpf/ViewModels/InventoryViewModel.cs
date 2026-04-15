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

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                InventoryView.Refresh();
                SummaryView.Refresh();
            }
        }
    }

    // ── Detail records ──
    public ObservableCollection<InventoryRecordDto> Items { get; } = new();
    public ICollectionView InventoryView { get; }

    // ── Summary (grouped by MaterialId + BatchNo) ──
    public ObservableCollection<InventorySummaryDto> SummaryItems { get; } = new();
    public ICollectionView SummaryView { get; }

    private bool _showSummary;
    public bool ShowSummary
    {
        get => _showSummary;
        set => SetProperty(ref _showSummary, value);
    }

    public ICommand RefreshCommand { get; }
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
        return record.MaterialCode.Contains(key, StringComparison.OrdinalIgnoreCase)
               || record.MaterialName.Contains(key, StringComparison.OrdinalIgnoreCase)
               || record.BatchNo.Contains(key, StringComparison.OrdinalIgnoreCase)
               || record.Location.Contains(key, StringComparison.OrdinalIgnoreCase);
    }

    private bool FilterSummary(object item)
    {
        if (item is not InventorySummaryDto summary) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        var key = SearchText.Trim();
        return summary.MaterialCode.Contains(key, StringComparison.OrdinalIgnoreCase)
               || summary.MaterialName.Contains(key, StringComparison.OrdinalIgnoreCase)
               || summary.BatchNo.Contains(key, StringComparison.OrdinalIgnoreCase);
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
    }

    /// <summary>入库：弹出完整入库弹窗（新建库存记录）</summary>
    private void OpenInboundDialogAsync()
    {
        var parameters = new DialogParameters { { "id", (Guid?)null } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.InventoryEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadAsync();
        });
    }

    /// <summary>编辑：弹出编辑弹窗（修改已有库存记录）</summary>
    private void OpenEditDialogAsync(Guid id)
    {
        var parameters = new DialogParameters { { "id", (Guid?)id } };
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
}

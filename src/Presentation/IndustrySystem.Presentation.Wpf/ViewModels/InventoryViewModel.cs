using System;
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
using Prism.Ioc;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class InventoryViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IInventoryAppService _svc;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                InventoryView.Refresh();
        }
    }

    public ObservableCollection<InventoryRecordDto> Items { get; } = new();
    public ICollectionView InventoryView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand InboundCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand OutboundCommand { get; }
    public ICommand DeleteCommand { get; }

    public InventoryViewModel(IInventoryAppService svc)
    {
        _svc = svc;
        InventoryView = CollectionViewSource.GetDefaultView(Items);
        InventoryView.Filter = FilterItems;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        InboundCommand = new DelegateCommand(async () => await OpenInboundDialogAsync());
        EditCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await OpenEditDialogAsync(id.Value);
        });
        OutboundCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await OpenOutboundDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

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

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync();
        Items.Clear();
        foreach (var item in list)
            Items.Add(item);
    }

    /// <summary>入库：弹出完整入库弹窗（新建库存记录）</summary>
    private async Task OpenInboundDialogAsync()
    {
        var vm = ContainerLocator.Current.Resolve<InventoryEditDialogViewModel>();
        vm.Title = "物料入库";
        await vm.LoadAsync(null);
        var dialog = new Views.Dialogs.InventoryEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };

        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is bool saved && saved)
                await LoadAsync();
        }
        finally
        {
            vm.PropertyChanged -= handler;
        }
    }

    /// <summary>编辑：弹出编辑弹窗（修改已有库存记录）</summary>
    private async Task OpenEditDialogAsync(Guid id)
    {
        var vm = ContainerLocator.Current.Resolve<InventoryEditDialogViewModel>();
        vm.Title = "编辑库存记录";
        await vm.LoadAsync(id);
        var dialog = new Views.Dialogs.InventoryEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };

        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is bool saved && saved)
                await LoadAsync();
        }
        finally
        {
            vm.PropertyChanged -= handler;
        }
    }

    /// <summary>出库：弹出出库弹窗，自主选择出库数量</summary>
    private async Task OpenOutboundDialogAsync(Guid id)
    {
        var vm = ContainerLocator.Current.Resolve<InventoryOutboundDialogViewModel>();
        await vm.LoadAsync(id);
        var dialog = new Views.Dialogs.InventoryOutboundDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                DialogHost.Close("RootDialogHost", vm.DialogResult);
        };

        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is bool saved && saved)
                await LoadAsync();
        }
        finally
        {
            vm.PropertyChanged -= handler;
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        await _svc.DeleteAsync(id);
        await LoadAsync();
    }
}

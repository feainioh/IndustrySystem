using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Commands;
using Prism.Mvvm;
using NLog;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class InventoryViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IInventoryAppService _svc;
    public ObservableCollection<Item> Items { get; } = new();
    public ICommand RefreshCommand { get; }
    public ICommand InCommand { get; }
    public ICommand OutCommand { get; }

    public InventoryViewModel(IInventoryAppService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncDelegateCommand(LoadAsync);
        InCommand = new AsyncDelegateCommand<Guid>(InAsync);
        OutCommand = new AsyncDelegateCommand<Guid>(OutAsync);
        _ = LoadAsync();
        _logger.Info(Resources.Strings.Log_InventoryViewModel_Initialized);
    }

    private async Task LoadAsync()
    {
        _logger.Debug(Resources.Strings.Log_Inventory_LoadStart);
        Items.Clear();
        var list = await _svc.GetListAsync();
        foreach (var it in list) Items.Add(new Item(it.Id, it.Name, it.Qty));
        _logger.Info(string.Format(Resources.Strings.Log_Inventory_LoadComplete, Items.Count));
    }

    private async Task InAsync(Guid id)
    {
        _logger.Info(string.Format(Resources.Strings.Log_Inventory_In, id));
        await _svc.InAsync(id);
        await LoadAsync();
    }

    private async Task OutAsync(Guid id)
    {
        _logger.Info(string.Format(Resources.Strings.Log_Inventory_Out, id));
        await _svc.OutAsync(id);
        await LoadAsync();
    }

    public record Item(Guid Id, string Name, int Qty);
}

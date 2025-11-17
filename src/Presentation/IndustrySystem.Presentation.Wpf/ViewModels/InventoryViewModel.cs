using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class InventoryViewModel
{
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
    }

    private async Task LoadAsync()
    {
        Items.Clear();
        var list = await _svc.GetListAsync();
        foreach (var it in list) Items.Add(new Item(it.Id, it.Name, it.Qty));
    }

    private async Task InAsync(Guid id)
    {
        await _svc.InAsync(id);
        await LoadAsync();
    }
    private async Task OutAsync(Guid id)
    {
        await _svc.OutAsync(id);
        await LoadAsync();
    }

    public record Item(Guid Id, string Name, int Qty);
}

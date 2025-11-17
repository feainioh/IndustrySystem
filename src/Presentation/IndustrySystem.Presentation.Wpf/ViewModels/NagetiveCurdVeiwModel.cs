using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class NagetiveCurdVeiwModel<TItem> : NagetiveViewModel
{
    public ObservableCollection<TItem> Items { get; } = new();

    public async Task RefreshAsync() => await OnRefreshAsync();

    protected override async Task OnRefreshAsync()
    {
        Items.Clear();
        foreach (var item in await LoadItemsAsync()) Items.Add(item);
    }

    protected abstract Task<IReadOnlyList<TItem>> LoadItemsAsync();
}

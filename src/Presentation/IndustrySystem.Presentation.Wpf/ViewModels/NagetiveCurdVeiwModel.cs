using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Commands;
using System.Windows.Input;
using System.Linq;
using System;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class NagetiveCurdVeiwModel<TItem> : NagetiveViewModel
{
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                PageIndex = 0;
            }
        }
    }

    private int _pageIndex;
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            if (value < 0) value = 0;
            if (SetProperty(ref _pageIndex, value))
            {
                _ = ApplyPagingAsync();
                RaisePageCommands();
                RaisePropertyChanged(nameof(PageIndex));
            }
        }
    }

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value <= 0) value = 1;
            if (SetProperty(ref _pageSize, value))
            {
                PageIndex = 0;
                _ = ApplyPagingAsync();
                RaisePageCommands();
                RaisePropertyChanged(nameof(PageSize));
            }
        }
    }

    private int _totalCount;
    public int TotalCount { get => _totalCount; private set { if (SetProperty(ref _totalCount, value)) RaisePropertyChanged(nameof(TotalCount)); } }

    public ObservableCollection<TItem> Items { get; } = new(); // current page
    protected readonly ObservableCollection<TItem> _all = new(); // full filtered set

    public ICommand SearchCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand PrevPageCommand { get; }

    protected NagetiveCurdVeiwModel()
    {
        SearchCommand = new AsyncDelegateCommand(async () =>
        {
            PageIndex = 0;
            await RefreshAsync();
        });
        NextPageCommand = new DelegateCommand(() => PageIndex++, () => (PageIndex + 1) * PageSize < TotalCount);
        PrevPageCommand = new DelegateCommand(() => PageIndex--, () => PageIndex > 0);
    }

    private void RaisePageCommands()
    {
        (NextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (PrevPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    public async Task RefreshAsync() => await OnRefreshAsync();

    protected override async Task OnRefreshAsync()
    {
        IsBusy = true;
        try
        {
            _all.Clear();
            Items.Clear();
            foreach (var item in await LoadItemsAsync()) _all.Add(item);
            TotalCount = _all.Count;
            await ApplyPagingAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private Task ApplyPagingAsync()
    {
        Items.Clear();
        var skip = PageIndex * PageSize;
        foreach (var item in _all.Skip(skip).Take(PageSize)) Items.Add(item);
        RaisePageCommands();
        return Task.CompletedTask;
    }

    protected abstract Task<IReadOnlyList<TItem>> LoadItemsAsync();

    protected virtual bool FilterItem(TItem item) => true; // override for custom filter
}

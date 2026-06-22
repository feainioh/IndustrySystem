using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Prism.Commands;
using System.Windows.Input;
using System.Linq;
using System;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class CrudViewModel<TItem> : NavigationViewModel
{
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                OnSearchTextChanged();
            }
        }
    }

    private int _pageIndex;
    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            var normalized = Math.Max(0, value);
            if (SetProperty(ref _pageIndex, normalized))
            {
                RaisePropertyChanged(nameof(CurrentPage));
                RaisePagingCommandStates();
            }
        }
    }

    public int CurrentPage
    {
        get => PageIndex + 1;
        set
        {
            var normalized = value < 1 ? 1 : value;
            PageIndex = normalized - 1;
        }
    }

    public ObservableCollection<int> PageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _pageSize = 20;
    public int PageSize
    {
        get => _pageSize;
        set
        {
            var normalized = value <= 0 ? 1 : value;
            if (SetProperty(ref _pageSize, normalized))
            {
                RaisePropertyChanged(nameof(TotalPages));
                if (PageIndex != 0)
                {
                    PageIndex = 0;
                }

                OnPagingParametersChanged(resetToFirstPage: true);
            }
        }
    }

    private int _totalCount;
    public int TotalCount
    {
        get => _totalCount;
        protected set
        {
            if (SetProperty(ref _totalCount, value))
            {
                RaisePropertyChanged(nameof(TotalPages));
                RaisePagingCommandStates();
            }
        }
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));

    public ObservableCollection<TItem> Items { get; } = new();
    protected readonly ObservableCollection<TItem> _all = new();

    public ICommand SearchCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }

    public ICommand PrevPageCommand => PreviousPageCommand;

    protected CrudViewModel()
    {
        SearchCommand = new DelegateCommand(() => OnSearchTextChanged());
        FirstPageCommand = new DelegateCommand(() => ChangePage(1), () => CurrentPage > 1);
        PreviousPageCommand = new DelegateCommand(() => ChangePage(CurrentPage - 1), () => CurrentPage > 1);
        NextPageCommand = new DelegateCommand(() => ChangePage(CurrentPage + 1), () => CurrentPage < TotalPages);
        LastPageCommand = new DelegateCommand(() => ChangePage(TotalPages), () => CurrentPage < TotalPages);
    }

    protected void RaisePagingCommandStates()
    {
        (FirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (PreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (NextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (LastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    private void ChangePage(int page)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (page > TotalPages)
        {
            page = TotalPages;
        }

        if (page == CurrentPage)
        {
            return;
        }

        CurrentPage = page;
        OnPagingParametersChanged(resetToFirstPage: false);
    }

    protected virtual void OnSearchTextChanged()
    {
        OnPagingParametersChanged(resetToFirstPage: true);
    }

    protected virtual void OnPagingParametersChanged(bool resetToFirstPage)
    {
        _ = ApplyPagingAsync(resetToFirstPage);
    }

    public async Task RefreshAsync() => await OnRefreshAsync();

    protected override async Task OnRefreshAsync()
    {
        IsBusy = true;
        try
        {
            _all.Clear();
            Items.Clear();
            foreach (var item in await LoadItemsAsync())
            {
                _all.Add(item);
            }

            await ApplyPagingAsync(resetToFirstPage: true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual Task ApplyPagingAsync(bool resetToFirstPage = false)
    {
        if (PageSize <= 0)
        {
            PageSize = 1;
        }

        var filtered = _all.Where(FilterItem).ToList();
        TotalCount = filtered.Count;

        if (resetToFirstPage)
        {
            PageIndex = 0;
        }

        var maxPageIndex = Math.Max(0, TotalPages - 1);
        if (PageIndex > maxPageIndex)
        {
            PageIndex = maxPageIndex;
        }

        Items.Clear();
        var skip = PageIndex * PageSize;
        foreach (var item in filtered.Skip(skip).Take(PageSize))
        {
            Items.Add(item);
        }

        RaisePagingCommandStates();
        return Task.CompletedTask;
    }

    protected abstract Task<IReadOnlyList<TItem>> LoadItemsAsync();

    protected virtual bool FilterItem(TItem item) => true;
}

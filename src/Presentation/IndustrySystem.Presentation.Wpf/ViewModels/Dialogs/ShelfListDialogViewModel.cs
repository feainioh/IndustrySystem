using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ShelfListDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;
    private readonly IDialogService _dialogService;
    private List<ShelfConfigDto> _all = new();

    public ObservableCollection<ShelfConfigDto> PagedShelves { get; } = new();
    public ObservableCollection<int> ShelfPageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _shelfCurrentPage = 1;
    public int ShelfCurrentPage
    {
        get => _shelfCurrentPage;
        set
        {
            if (SetProperty(ref _shelfCurrentPage, value))
                ApplyShelfPaging();
        }
    }

    private int _shelfPageSize = 20;
    public int ShelfPageSize
    {
        get => _shelfPageSize;
        set
        {
            if (SetProperty(ref _shelfPageSize, value))
                ApplyShelfPaging(resetToFirstPage: true);
        }
    }

    private int _shelfTotalCount;
    public int ShelfTotalCount
    {
        get => _shelfTotalCount;
        private set
        {
            if (SetProperty(ref _shelfTotalCount, value))
                RaisePropertyChanged(nameof(ShelfTotalPages));
        }
    }

    public int ShelfTotalPages => Math.Max(1, (int)Math.Ceiling(ShelfTotalCount / (double)ShelfPageSize));

    public ICommand AddShelfCommand { get; }
    public ICommand EditShelfCommand { get; }
    public ICommand DeleteShelfCommand { get; }
    public ICommand ShelfFirstPageCommand { get; }
    public ICommand ShelfPreviousPageCommand { get; }
    public ICommand ShelfNextPageCommand { get; }
    public ICommand ShelfLastPageCommand { get; }

    public ShelfListDialogViewModel(IShelfAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;
        Title = Strings.Dialog_ShelfEdit_Title;

        AddShelfCommand = new DelegateCommand(() => OpenEditDialog(null));
        EditShelfCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenEditDialog(id.Value);
        });
        DeleteShelfCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteShelfAsync(id.Value);
        });

        ShelfFirstPageCommand = new DelegateCommand(() => SetShelfPage(1), () => ShelfCurrentPage > 1);
        ShelfPreviousPageCommand = new DelegateCommand(() => SetShelfPage(ShelfCurrentPage - 1), () => ShelfCurrentPage > 1);
        ShelfNextPageCommand = new DelegateCommand(() => SetShelfPage(ShelfCurrentPage + 1), () => ShelfCurrentPage < ShelfTotalPages);
        ShelfLastPageCommand = new DelegateCommand(() => SetShelfPage(ShelfTotalPages), () => ShelfCurrentPage < ShelfTotalPages);
    }

    public override async void OnDialogOpened(IDialogParameters parameters)
    {
        await LoadShelvesAsync();
    }

    private async Task LoadShelvesAsync()
    {
        var list = await _svc.GetShelfListAsync();
        _all = list.ToList();
        ApplyShelfPaging(resetToFirstPage: true);
    }

    private void OpenEditDialog(Guid? id)
    {
        var parameters = new DialogParameters();
        if (id.HasValue) parameters.Add("id", id.Value);
        _dialogService.ShowDialog(nameof(Views.Dialogs.ShelfEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadShelvesAsync();
        });
    }

    private void ApplyShelfPaging(bool resetToFirstPage = false)
    {
        if (ShelfPageSize <= 0) ShelfPageSize = 1;

        ShelfTotalCount = _all.Count;
        if (resetToFirstPage) ShelfCurrentPage = 1;

        var maxPage = Math.Max(1, ShelfTotalPages);
        if (ShelfCurrentPage > maxPage) ShelfCurrentPage = maxPage;

        PagedShelves.Clear();
        var skip = (ShelfCurrentPage - 1) * ShelfPageSize;
        foreach (var item in _all.Skip(skip).Take(ShelfPageSize))
            PagedShelves.Add(item);

        RaiseShelfPagingCommandStates();
    }

    private void RaiseShelfPagingCommandStates()
    {
        (ShelfFirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfPreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfNextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ShelfLastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    private void SetShelfPage(int page)
    {
        if (page < 1) page = 1;
        if (page > ShelfTotalPages) page = ShelfTotalPages;
        if (page == ShelfCurrentPage) return;
        ShelfCurrentPage = page;
    }

    private async Task DeleteShelfAsync(Guid id)
    {
        var r = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;
        await _svc.DeleteShelfAsync(id);
        await LoadShelvesAsync();
    }
}

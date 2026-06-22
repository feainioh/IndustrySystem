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

public class ContainerListDialogViewModel : DialogViewModel
{
    private readonly IShelfAppService _svc;
    private readonly IDialogService _dialogService;
    private List<ContainerInfoDto> _all = new();

    public ObservableCollection<ContainerInfoDto> PagedContainers { get; } = new();
    public ObservableCollection<int> ContainerPageSizes { get; } = new() { 10, 20, 50, 100 };

    private int _containerCurrentPage = 1;
    public int ContainerCurrentPage
    {
        get => _containerCurrentPage;
        set
        {
            if (SetProperty(ref _containerCurrentPage, value))
                ApplyContainerPaging();
        }
    }

    private int _containerPageSize = 20;
    public int ContainerPageSize
    {
        get => _containerPageSize;
        set
        {
            if (SetProperty(ref _containerPageSize, value))
                ApplyContainerPaging(resetToFirstPage: true);
        }
    }

    private int _containerTotalCount;
    public int ContainerTotalCount
    {
        get => _containerTotalCount;
        private set
        {
            if (SetProperty(ref _containerTotalCount, value))
                RaisePropertyChanged(nameof(ContainerTotalPages));
        }
    }

    public int ContainerTotalPages => Math.Max(1, (int)Math.Ceiling(ContainerTotalCount / (double)ContainerPageSize));

    public ICommand AddContainerCommand { get; }
    public ICommand EditContainerCommand { get; }
    public ICommand DeleteContainerCommand { get; }
    public ICommand ContainerFirstPageCommand { get; }
    public ICommand ContainerPreviousPageCommand { get; }
    public ICommand ContainerNextPageCommand { get; }
    public ICommand ContainerLastPageCommand { get; }

    public ContainerListDialogViewModel(IShelfAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;
        Title = Strings.Dialog_ContainerList_Title;

        AddContainerCommand = new DelegateCommand(() => OpenEditDialog(null));
        EditContainerCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenEditDialog(id.Value);
        });
        DeleteContainerCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteContainerAsync(id.Value);
        });

        ContainerFirstPageCommand = new DelegateCommand(() => SetContainerPage(1), () => ContainerCurrentPage > 1);
        ContainerPreviousPageCommand = new DelegateCommand(() => SetContainerPage(ContainerCurrentPage - 1), () => ContainerCurrentPage > 1);
        ContainerNextPageCommand = new DelegateCommand(() => SetContainerPage(ContainerCurrentPage + 1), () => ContainerCurrentPage < ContainerTotalPages);
        ContainerLastPageCommand = new DelegateCommand(() => SetContainerPage(ContainerTotalPages), () => ContainerCurrentPage < ContainerTotalPages);
    }

    public override async void OnDialogOpened(IDialogParameters parameters)
    {
        await LoadContainersAsync();
    }

    private async Task LoadContainersAsync()
    {
        var list = await _svc.GetContainerListAsync();
        _all = list.ToList();
        ApplyContainerPaging(resetToFirstPage: true);
    }

    private void OpenEditDialog(Guid? id)
    {
        var parameters = new DialogParameters();
        if (id.HasValue) parameters.Add("id", id.Value);
        _dialogService.ShowDialog(nameof(Views.Dialogs.ContainerEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
                await LoadContainersAsync();
        });
    }

    private void ApplyContainerPaging(bool resetToFirstPage = false)
    {
        if (ContainerPageSize <= 0) ContainerPageSize = 1;

        ContainerTotalCount = _all.Count;
        if (resetToFirstPage) ContainerCurrentPage = 1;

        var maxPage = Math.Max(1, ContainerTotalPages);
        if (ContainerCurrentPage > maxPage) ContainerCurrentPage = maxPage;

        PagedContainers.Clear();
        var skip = (ContainerCurrentPage - 1) * ContainerPageSize;
        foreach (var item in _all.Skip(skip).Take(ContainerPageSize))
            PagedContainers.Add(item);

        RaiseContainerPagingCommandStates();
    }

    private void RaiseContainerPagingCommandStates()
    {
        (ContainerFirstPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerPreviousPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerNextPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        (ContainerLastPageCommand as DelegateCommand)?.RaiseCanExecuteChanged();
    }

    private void SetContainerPage(int page)
    {
        if (page < 1) page = 1;
        if (page > ContainerTotalPages) page = ContainerTotalPages;
        if (page == ContainerCurrentPage) return;
        ContainerCurrentPage = page;
    }

    private async Task DeleteContainerAsync(Guid id)
    {
        var r = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;
        await _svc.DeleteContainerAsync(id);
        await LoadContainersAsync();
    }
}

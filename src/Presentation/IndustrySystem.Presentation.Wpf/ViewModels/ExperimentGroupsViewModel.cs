using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Presentation.Wpf.Resources;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentGroupsViewModel : CrudViewModel<ExperimentGroupDto>
{
    private readonly IExperimentGroupAppService _svc;
    private readonly IDialogService _dialogService;

    public ObservableCollection<ExperimentGroupDto> Groups { get; } = new();
    public ICollectionView GroupsView { get; }

    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentGroupsViewModel(IExperimentGroupAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;

        GroupsView = CollectionViewSource.GetDefaultView(Groups);
        GroupsView.Filter = FilterGroups;

        AddCommand = new DelegateCommand(() => OpenDialogAsync(null));
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

        _ = LoadAsync();
    }

    private bool FilterGroups(object item)
    {
        if (item is not ExperimentGroupDto group) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var key = SearchText.Trim();
        return group.GroupCode.Contains(key, StringComparison.OrdinalIgnoreCase)
               || group.Name.Contains(key, StringComparison.OrdinalIgnoreCase)
               || group.CreatedBy.Contains(key, StringComparison.OrdinalIgnoreCase);
    }

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync();

        await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
        {
            Groups.Clear();
            foreach (var item in list)
            {
                Groups.Add(item);
            }
        });
    }

    private void OpenDialogAsync(Guid? id)
    {
        var parameters = new DialogParameters();
        if (id.HasValue)
        {
            parameters.Add("id", id.Value);
        }

        _dialogService.ShowDialog(nameof(Views.Dialogs.ExperimentGroupEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                await LoadAsync();
            }
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var isConfirmed = await ConfirmDeleteAsync();
        if (!isConfirmed)
        {
            return;
        }

        await _svc.DeleteAsync(id);

        var target = Groups.FirstOrDefault(x => x.Id == id);
        if (target is not null)
        {
            Groups.Remove(target);
        }
    }

    private Task<bool> ConfirmDeleteAsync()
    {
        var completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var parameters = new DialogParameters
        {
            { "title", Strings.Msg_WarningTitle },
            { "message", Strings.Msg_ConfirmDelete }
        };

        _dialogService.ShowDialog(nameof(Views.Dialogs.ConfirmDialog), parameters, result =>
        {
            completionSource.TrySetResult(result.Result == ButtonResult.OK);
        });

        return completionSource.Task;
    }

    protected override void OnSearchTextChanged()
    {
        GroupsView.Refresh();
    }

    protected override async Task OnRefreshAsync()
    {
        await LoadAsync();
    }

    protected override async Task<IReadOnlyList<ExperimentGroupDto>> LoadItemsAsync()
        => await _svc.GetListAsync();
}

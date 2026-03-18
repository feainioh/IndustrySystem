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

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class ExperimentGroupsViewModel : BindableBase
{
    private readonly IExperimentGroupAppService _svc;

    public ObservableCollection<ExperimentGroupDto> Groups { get; } = new();
    public ICollectionView GroupsView { get; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                GroupsView.Refresh();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public ExperimentGroupsViewModel(IExperimentGroupAppService svc)
    {
        _svc = svc;

        GroupsView = CollectionViewSource.GetDefaultView(Groups);
        GroupsView.Filter = FilterGroups;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(async () => await OpenDialogAsync(null));
        EditCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await OpenDialogAsync(id.Value);
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

    private async Task OpenDialogAsync(Guid? id)
    {
        var vm = ContainerLocator.Current.Resolve<ExperimentGroupEditDialogViewModel>();
        await vm.LoadAsync(id);

        var dialog = new Views.Dialogs.ExperimentGroupEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (_, e) =>
        {
            if (e.PropertyName == nameof(DialogViewModel.DialogResult))
            {
                DialogHost.Close("RootDialogHost", vm.DialogResult);
            }
        };

        vm.PropertyChanged += handler;
        try
        {
            var result = await DialogHost.Show(dialog, "RootDialogHost");
            if (result is bool saved && saved)
            {
                await LoadAsync();
            }
        }
        finally
        {
            vm.PropertyChanged -= handler;
        }
    }

    private async Task DeleteAsync(Guid id)
    {
        var r = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (r != MessageBoxResult.Yes) return;

        await _svc.DeleteAsync(id);

        var target = Groups.FirstOrDefault(x => x.Id == id);
        if (target is not null)
        {
            Groups.Remove(target);
        }
    }
}

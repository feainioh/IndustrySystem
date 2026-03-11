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

public class MaterialInfoViewModel : BindableBase
{
    private readonly IMaterialAppService _svc;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                MaterialsView.Refresh();
            }
        }
    }

    public ObservableCollection<MaterialDto> Materials { get; } = new();
    public ICollectionView MaterialsView { get; }

    public ICommand RefreshCommand { get; }
    public ICommand AddCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public MaterialInfoViewModel(IMaterialAppService svc)
    {
        _svc = svc;
        MaterialsView = CollectionViewSource.GetDefaultView(Materials);
        MaterialsView.Filter = FilterMaterials;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(async () => await OpenMaterialDialogAsync(null));
        EditCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await OpenMaterialDialogAsync(id.Value);
        });
        DeleteCommand = new DelegateCommand<Guid?>(async id =>
        {
            if (id.HasValue) await DeleteAsync(id.Value);
        });

        _ = LoadAsync();
    }

    private bool FilterMaterials(object item)
    {
        if (item is not MaterialDto material) return false;
        if (string.IsNullOrWhiteSpace(SearchText)) return true;

        var key = SearchText.Trim();
        return material.MaterialCode.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.Name.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.FullName.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.MolecularFormula.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.CasNo.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.Supplier.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.Brand.Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.Category.ToString().Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.MaterialType.ToString().Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.HazardLevel.ToString().Contains(key, StringComparison.OrdinalIgnoreCase)
               || material.StorageCondition.ToString().Contains(key, StringComparison.OrdinalIgnoreCase);
    }

    public async Task LoadAsync()
    {
        var list = await _svc.GetListAsync();
        Materials.Clear();
        foreach (var item in list)
        {
            Materials.Add(item);
        }
    }

    private async Task OpenMaterialDialogAsync(Guid? id)
    {
        var vm = ContainerLocator.Current.Resolve<MaterialEditDialogViewModel>();
        await vm.LoadAsync(id);
        var dialog = new Views.Dialogs.MaterialEditDialog { DataContext = vm };

        PropertyChangedEventHandler handler = (s, e) =>
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
        var result = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        await _svc.DeleteAsync(id);
        await LoadAsync();
    }
}

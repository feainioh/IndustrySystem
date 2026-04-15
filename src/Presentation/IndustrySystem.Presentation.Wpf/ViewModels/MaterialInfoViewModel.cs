using System;
using System.Collections.Generic;
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
using Prism.Dialogs;
using Prism.Ioc;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class MaterialInfoViewModel : NagetiveCurdVeiwModel<MaterialDto>
{
    private readonly IMaterialAppService _svc;
    private readonly IDialogService _dialogService;

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

    public MaterialInfoViewModel(IMaterialAppService svc, IDialogService dialogService)
    {
        _svc = svc;
        _dialogService = dialogService;
        MaterialsView = CollectionViewSource.GetDefaultView(Materials);
        MaterialsView.Filter = FilterMaterials;

        RefreshCommand = new DelegateCommand(async () => await LoadAsync());
        AddCommand = new DelegateCommand(() => OpenMaterialDialogAsync(null));
        EditCommand = new DelegateCommand<Guid?>(id =>
        {
            if (id.HasValue) OpenMaterialDialogAsync(id.Value);
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

    private void OpenMaterialDialogAsync(Guid? id)
    {
        var parameters = new DialogParameters { { "id", id } };
        _dialogService.ShowDialog(nameof(Views.Dialogs.MaterialEditDialog), parameters, async result =>
        {
            if (result.Result == ButtonResult.OK)
            {
                await LoadAsync();
            }
        });
    }

    private async Task DeleteAsync(Guid id)
    {
        var result = MessageBox.Show(Strings.Msg_ConfirmDelete, Strings.Msg_WarningTitle,
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        await _svc.DeleteAsync(id);
        await LoadAsync();
    }

    protected override async Task<IReadOnlyList<MaterialDto>> LoadItemsAsync()
        => await _svc.GetListAsync();
}

using System;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums.MaterialEnums;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class MaterialEditDialogViewModel : DialogViewModel
{
    private readonly IMaterialAppService _svc;

    public Guid Id { get; set; }

    private string _materialCode = string.Empty;
    public string MaterialCode { get => _materialCode; set { if (SetProperty(ref _materialCode, value)) RaiseSaveCanExecuteChanged(); } }

    private string _name = string.Empty;
    public string Name { get => _name; set { if (SetProperty(ref _name, value)) RaiseSaveCanExecuteChanged(); } }

    private string _fullName = string.Empty;
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }

    private string _molecularFormula = string.Empty;
    public string MolecularFormula { get => _molecularFormula; set => SetProperty(ref _molecularFormula, value); }

    private MaterialCategory _category = MaterialCategory.Solid;
    public MaterialCategory Category { get => _category; set => SetProperty(ref _category, value); }

    private MaterialType _materialType = MaterialType.RawMaterial;
    public MaterialType MaterialType { get => _materialType; set => SetProperty(ref _materialType, value); }

    private string _casNo = string.Empty;
    public string CasNo { get => _casNo; set => SetProperty(ref _casNo, value); }

    private string _purity = string.Empty;
    public string Purity { get => _purity; set => SetProperty(ref _purity, value); }

    private string _density = string.Empty;
    public string Density { get => _density; set => SetProperty(ref _density, value); }

    private string _unit = string.Empty;
    public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

    private MaterialHazardLevel _hazardLevel = MaterialHazardLevel.None;
    public MaterialHazardLevel HazardLevel { get => _hazardLevel; set => SetProperty(ref _hazardLevel, value); }

    private MaterialStorageCondition _storageCondition = MaterialStorageCondition.RoomTemperature;
    public MaterialStorageCondition StorageCondition { get => _storageCondition; set => SetProperty(ref _storageCondition, value); }

    private string _precautions = string.Empty;
    public string Precautions { get => _precautions; set => SetProperty(ref _precautions, value); }

    private string _brand = string.Empty;
    public string Brand { get => _brand; set => SetProperty(ref _brand, value); }

    private string _supplier = string.Empty;
    public string Supplier { get => _supplier; set => SetProperty(ref _supplier, value); }

    public Array CategoryOptions => Enum.GetValues(typeof(MaterialCategory));
    public Array MaterialTypeOptions => Enum.GetValues(typeof(MaterialType));
    public Array HazardLevelOptions => Enum.GetValues(typeof(MaterialHazardLevel));
    public Array StorageConditionOptions => Enum.GetValues(typeof(MaterialStorageCondition));

    public MaterialEditDialogViewModel(IMaterialAppService svc)
    {
        _svc = svc;
        Title = "编辑物料";
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        var id = parameters.GetValue<Guid?>("id");
        _ = LoadAsync(id);
    }

    public async Task LoadAsync(Guid? id)
    {
        if (id is null)
        {
            Id = Guid.Empty;
            MaterialCode = string.Empty;
            Name = string.Empty;
            FullName = string.Empty;
            MolecularFormula = string.Empty;
            Category = MaterialCategory.Solid;
            MaterialType = MaterialType.RawMaterial;
            CasNo = string.Empty;
            Purity = string.Empty;
            Density = string.Empty;
            Unit = string.Empty;
            HazardLevel = MaterialHazardLevel.None;
            StorageCondition = MaterialStorageCondition.RoomTemperature;
            Precautions = string.Empty;
            Brand = string.Empty;
            Supplier = string.Empty;
            return;
        }

        var item = await _svc.GetAsync(id.Value);
        if (item == null)
        {
            Id = id.Value;
            return;
        }

        Id = item.Id;
        MaterialCode = item.MaterialCode;
        Name = item.Name;
        FullName = item.FullName;
        MolecularFormula = item.MolecularFormula;
        Category = item.Category;
        MaterialType = item.MaterialType;
        CasNo = item.CasNo;
        Purity = item.Purity;
        Density = item.Density;
        Unit = item.Unit;
        HazardLevel = item.HazardLevel;
        StorageCondition = item.StorageCondition;
        Precautions = item.Precautions;
        Brand = item.Brand;
        Supplier = item.Supplier;
    }

    protected override bool CanSave() => !string.IsNullOrWhiteSpace(MaterialCode) && !string.IsNullOrWhiteSpace(Name);

    protected override async Task OnSaveAsync()
    {
        var dto = new MaterialDto(
            Id,
            MaterialCode.Trim(),
            Name.Trim(),
            FullName.Trim(),
            MolecularFormula.Trim(),
            Category,
            MaterialType,
            CasNo.Trim(),
            Purity.Trim(),
            Density.Trim(),
            Unit.Trim(),
            HazardLevel,
            StorageCondition,
            Precautions.Trim(),
            Brand.Trim(),
            Supplier.Trim());

        if (Id == Guid.Empty)
        {
            _ = await _svc.CreateAsync(dto);
        }
        else
        {
            _ = await _svc.UpdateAsync(dto);
        }

        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
    }

    protected override void OnCancel() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
}

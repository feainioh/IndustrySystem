using IndustrySystem.Domain.Shared.Enums.MaterialEnums;

namespace IndustrySystem.Domain.Entities.Materials;

public class Material
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MaterialCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MolecularFormula { get; set; } = string.Empty;
    public MaterialCategory Category { get; set; }
    public MaterialType MaterialType { get; set; }
    public string CasNo { get; set; } = string.Empty;
    public string Purity { get; set; } = string.Empty;
    public string Density { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public MaterialHazardLevel HazardLevel { get; set; }
    public MaterialStorageCondition StorageCondition { get; set; }
    public string Precautions { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Supplier { get; set; } = string.Empty;
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

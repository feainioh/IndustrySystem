using IndustrySystem.Domain.Shared.Enums.MaterialEnums;

namespace IndustrySystem.Domain.Entities.Materials;

/// <summary>
/// 物料主数据实体。
/// </summary>
public class Material
{
    /// <summary>物料主键</summary>
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>物料编码（业务唯一）</summary>
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>物料简称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>物料全称</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>分子式</summary>
    public string MolecularFormula { get; set; } = string.Empty;

    /// <summary>物料分类</summary>
    public MaterialCategory Category { get; set; }

    /// <summary>物料类型</summary>
    public MaterialType MaterialType { get; set; }

    /// <summary>CAS 号</summary>
    public string CasNo { get; set; } = string.Empty;

    /// <summary>纯度描述</summary>
    public string Purity { get; set; } = string.Empty;

    /// <summary>密度描述</summary>
    public string Density { get; set; } = string.Empty;

    /// <summary>计量单位</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>危险等级</summary>
    public MaterialHazardLevel HazardLevel { get; set; }

    /// <summary>存储条件</summary>
    public MaterialStorageCondition StorageCondition { get; set; }

    /// <summary>注意事项</summary>
    public string Precautions { get; set; } = string.Empty;

    /// <summary>品牌</summary>
    public string Brand { get; set; } = string.Empty;

    /// <summary>供应商</summary>
    public string Supplier { get; set; } = string.Empty;

    /// <summary>创建时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间（UTC）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

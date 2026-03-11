namespace IndustrySystem.Domain.Entities.Inventory;

public class InventoryRecord
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>关联物料ID</summary>
    public Guid MaterialId { get; set; }

    /// <summary>物料编码（冗余便于查询）</summary>
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>物料名称（冗余便于列表显示）</summary>
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>批号</summary>
    public string BatchNo { get; set; } = string.Empty;

    /// <summary>总库存数量</summary>
    public decimal Quantity { get; set; }

    /// <summary>安全库存</summary>
    public decimal SafetyStock { get; set; }

    /// <summary>计量单位</summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>入库日期</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? InboundDate { get; set; }

    /// <summary>过期日期</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? ExpiryDate { get; set; }

    /// <summary>存放位置/货架</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>备注</summary>
    public string Remark { get; set; } = string.Empty;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

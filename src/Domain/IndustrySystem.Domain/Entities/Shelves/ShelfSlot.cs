namespace IndustrySystem.Domain.Entities.Shelves;

using IndustrySystem.Domain.Shared.Enums.ShelfEnums;

/// <summary>货架槽位（每个货架中每个位置的容器/库存绑定）</summary>
public class ShelfSlot
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>所属货架ID</summary>
    public Guid ShelfId { get; set; }

    /// <summary>行号（从1开始）</summary>
    public int Row { get; set; }

    /// <summary>列号（从1开始）</summary>
    public int Column { get; set; }

    /// <summary>此槽位允许的容器类型（逗号分隔的枚举名称，空字符串=不限制）</summary>
    [SqlSugar.SugarColumn(IsNullable = true, Length = 500)]
    public string AllowedContainerTypes { get; set; } = string.Empty;

    /// <summary>槽位绑定的容器ID（可空=未配置容器）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public Guid? ContainerId { get; set; }

    /// <summary>槽位关联的库存记录ID（可空=空位）</summary>
    [SqlSugar.SugarColumn(IsNullable = true)]
    public Guid? InventoryRecordId { get; set; }

    /// <summary>是否禁用该槽位</summary>
    public bool IsDisabled { get; set; }

    /// <summary>备注</summary>
    public string Remark { get; set; } = string.Empty;

    /// <summary>解析允许的容器类型列表</summary>
    [SqlSugar.SugarColumn(IsIgnore = true)]
    public List<ContainerType> AllowedContainerTypeList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AllowedContainerTypes)) return [];
            return AllowedContainerTypes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<ContainerType>(s.Trim(), out var ct) ? (ContainerType?)ct : null)
                .Where(ct => ct.HasValue)
                .Select(ct => ct!.Value)
                .ToList();
        }
        set => AllowedContainerTypes = value is { Count: > 0 }
            ? string.Join(",", value.Select(ct => ct.ToString()))
            : string.Empty;
    }
}

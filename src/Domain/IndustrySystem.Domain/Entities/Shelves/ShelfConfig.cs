namespace IndustrySystem.Domain.Entities.Shelves;

/// <summary>货架配置</summary>
public class ShelfConfig
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>货架编码（唯一标识）</summary>
    public string ShelfCode { get; set; } = string.Empty;

    /// <summary>货架名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>行数</summary>
    public int Rows { get; set; }

    /// <summary>列数</summary>
    public int Columns { get; set; }

    /// <summary>描述</summary>
    public string Description { get; set; } = string.Empty;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

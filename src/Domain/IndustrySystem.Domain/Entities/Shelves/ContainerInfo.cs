using IndustrySystem.Domain.Shared.Enums.ShelfEnums;

namespace IndustrySystem.Domain.Entities.Shelves;

/// <summary>容器信息（可放入货架槽位的容器模板）</summary>
public class ContainerInfo
{
    [SqlSugar.SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>容器名称</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>容器类型</summary>
    public ContainerType ContainerType { get; set; }

    /// <summary>容器内部行数（试剂盒等可自定义）</summary>
    public int Rows { get; set; } = 1;

    /// <summary>容器内部列数</summary>
    public int Columns { get; set; } = 1;

    /// <summary>描述</summary>
    public string Description { get; set; } = string.Empty;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [SqlSugar.SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }
}

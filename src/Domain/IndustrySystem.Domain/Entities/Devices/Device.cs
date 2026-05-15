using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices;

/// <summary>
/// 通用设备实体。
/// </summary>
public class Device
{
    /// <summary>
    /// 设备主键。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 设备名称。
    /// </summary>
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 设备类型标识。
    /// </summary>
    [SugarColumn(ColumnName = "type")]
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否在线。
    /// </summary>
    [SugarColumn(ColumnName = "isonline")]
    public bool IsOnline { get; set; }
}

using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices.Motors;

/// <summary>
/// CAN 总线电机实体。
/// </summary>
public class CanMotor
{
    /// <summary>
    /// 电机主键。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 电机名称。
    /// </summary>
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// CAN 节点号。
    /// </summary>
    [SugarColumn(ColumnName = "nodeid")]
    public int NodeId { get; set; }
    
    /// <summary>
    /// CAN 波特率。
    /// </summary>
    [SugarColumn(ColumnName = "baudrate")]
    public int BaudRate { get; set; }
}

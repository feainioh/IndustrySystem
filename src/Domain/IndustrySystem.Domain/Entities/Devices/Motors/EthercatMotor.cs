using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices.Motors;

/// <summary>
/// EtherCAT 电机实体。
/// </summary>
public class EthercatMotor
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
    /// 从站地址。
    /// </summary>
    [SugarColumn(ColumnName = "slaveaddress")]
    public int SlaveAddress { get; set; }
}

using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices.Motors;

public class EthercatMotor
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "slaveaddress")]
    public int SlaveAddress { get; set; }
}

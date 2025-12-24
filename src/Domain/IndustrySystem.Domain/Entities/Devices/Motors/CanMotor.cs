using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices.Motors;

public class CanMotor
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "nodeid")]
    public int NodeId { get; set; }
    
    [SugarColumn(ColumnName = "baudrate")]
    public int BaudRate { get; set; }
}

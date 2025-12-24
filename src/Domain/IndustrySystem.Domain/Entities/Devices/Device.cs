using SqlSugar;

namespace IndustrySystem.Domain.Entities.Devices;

public class Device
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "name")]
    public string Name { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "type")]
    public string Type { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "isonline")]
    public bool IsOnline { get; set; }
}

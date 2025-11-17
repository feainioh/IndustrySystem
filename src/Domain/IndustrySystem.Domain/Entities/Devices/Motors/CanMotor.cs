namespace IndustrySystem.Domain.Entities.Devices.Motors;

public class CanMotor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int NodeId { get; set; }
    public int BaudRate { get; set; }
}

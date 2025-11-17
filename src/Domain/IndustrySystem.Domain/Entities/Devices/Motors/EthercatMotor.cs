namespace IndustrySystem.Domain.Entities.Devices.Motors;

public class EthercatMotor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int SlaveAddress { get; set; }
}

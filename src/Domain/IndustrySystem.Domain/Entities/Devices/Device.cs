namespace IndustrySystem.Domain.Entities.Devices;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Motor/IO/Other
    public bool IsOnline { get; set; }
}

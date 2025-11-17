namespace IndustrySystem.Domain.Entities.Experiments;

public class Experiment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TemplateId { get; set; }
    public Guid GroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

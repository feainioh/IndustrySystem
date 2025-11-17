namespace IndustrySystem.Domain.Entities.Experiments;

public class ExperimentTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

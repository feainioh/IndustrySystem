namespace IndustrySystem.Domain.Entities.Experiments;

public class ExperimentGroup
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

namespace IndustrySystem.Application.Contracts.Dtos;

public record ExperimentTemplateDto(Guid Id, string Name, string? Description);
public record ExperimentGroupDto(Guid Id, string Name);
public record ExperimentDto(Guid Id, Guid TemplateId, Guid GroupId, string Name, DateTime CreatedAt);
public record InventoryItemDto(Guid Id, string Name, int Qty);

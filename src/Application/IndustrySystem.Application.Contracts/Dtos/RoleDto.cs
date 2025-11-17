namespace IndustrySystem.Application.Contracts.Dtos;

public record RoleDto(Guid Id, string Name, string? Description, bool IsDefault);

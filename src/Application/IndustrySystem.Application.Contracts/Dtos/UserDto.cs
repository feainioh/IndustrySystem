namespace IndustrySystem.Application.Contracts.Dtos;

public record UserDto(Guid Id, string UserName, string DisplayName, bool IsActive);

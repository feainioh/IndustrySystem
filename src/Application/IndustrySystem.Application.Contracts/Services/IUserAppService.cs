using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IUserAppService
{
    Task<UserDto?> GetAsync(Guid id);
    Task<List<UserDto>> GetListAsync();
    Task<UserDto> CreateAsync(UserDto input);
    Task<UserDto> UpdateAsync(UserDto input);
    Task DeleteAsync(Guid id);
    Task<Guid[]> GetRoleIdsAsync(Guid userId);
    Task SetRolesAsync(Guid userId, Guid[] roleIds);
}

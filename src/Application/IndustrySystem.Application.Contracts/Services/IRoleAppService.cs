using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IRoleAppService
{
    Task<RoleDto?> GetAsync(Guid id);
    Task<List<RoleDto>> GetListAsync();
    Task<RoleDto> CreateAsync(RoleDto input);
    Task<RoleDto> UpdateAsync(RoleDto input);
    Task DeleteAsync(Guid id);
}

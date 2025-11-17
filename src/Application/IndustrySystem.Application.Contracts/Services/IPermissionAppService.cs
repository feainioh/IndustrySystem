using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IPermissionAppService
{
    Task<PermissionDto?> GetAsync(Guid id);
    Task<List<PermissionDto>> GetListAsync();
    Task<PermissionDto> CreateAsync(PermissionDto input);
    Task<PermissionDto> UpdateAsync(PermissionDto input);
    Task DeleteAsync(Guid id);
}

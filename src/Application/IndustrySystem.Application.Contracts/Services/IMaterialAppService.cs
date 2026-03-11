using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IMaterialAppService
{
    Task<IReadOnlyList<MaterialDto>> GetListAsync();
    Task<MaterialDto?> GetAsync(Guid id);
    Task<MaterialDto> CreateAsync(MaterialDto input);
    Task<MaterialDto> UpdateAsync(MaterialDto input);
    Task DeleteAsync(Guid id);
}

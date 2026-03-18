using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentGroupAppService
{
    Task<IReadOnlyList<ExperimentGroupDto>> GetListAsync();
    Task<ExperimentGroupDto?> GetAsync(Guid id);
    Task<ExperimentGroupDto> CreateAsync(ExperimentGroupDto input);
    Task<ExperimentGroupDto> UpdateAsync(ExperimentGroupDto input);
    Task DeleteAsync(Guid id);

    Task<IReadOnlyList<ExperimentOptionDto>> GetExperimentOptionsAsync();
}

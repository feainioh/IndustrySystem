using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentParameterAppService
{
    Task<IReadOnlyList<ExperimentParameterOptionDto>> GetOptionsAsync(ExperimentType type);

    Task<IReadOnlyList<ExperimentParameterItemDto>> GetListAsync(ExperimentType type);
    Task<ExperimentParameterItemDto?> GetAsync(ExperimentType type, Guid id);
    Task<ExperimentParameterItemDto> CreateAsync(ExperimentParameterItemDto input);
    Task<ExperimentParameterItemDto> UpdateAsync(ExperimentParameterItemDto input);
    Task DeleteAsync(ExperimentType type, Guid id);
}

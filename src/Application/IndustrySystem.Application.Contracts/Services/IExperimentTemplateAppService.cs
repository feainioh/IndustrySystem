using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentTemplateAppService
{
    Task<ExperimentTemplateDto?> GetAsync(Guid id);
    Task<List<ExperimentTemplateDto>> GetListAsync();
    Task<ExperimentTemplateDto> CreateAsync(ExperimentTemplateDto input);
    Task<ExperimentTemplateDto> UpdateAsync(ExperimentTemplateDto input);
    Task DeleteAsync(Guid id);
}

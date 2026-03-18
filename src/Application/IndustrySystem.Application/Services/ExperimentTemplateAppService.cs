using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Repositories;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Services;

public class ExperimentTemplateAppService : IExperimentTemplateAppService
{
    private readonly IRepository<Experiment> _repo;
    private readonly IMapper _mapper;

    public ExperimentTemplateAppService(IRepository<Experiment> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<ExperimentTemplateDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null || !entity.IsTemplate) return null;
        return _mapper.Map<ExperimentTemplateDto>(entity);
    }

    public async Task<List<ExperimentTemplateDto>> GetListAsync()
        => (await _repo.GetListAsync())
            .Where(x => x.IsTemplate)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Select(_mapper.Map<ExperimentTemplateDto>)
            .ToList();

    public async Task<ExperimentTemplateDto> CreateAsync(ExperimentTemplateDto input)
    {
        var entity = _mapper.Map<Experiment>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.IsTemplate = true;
        entity.Name = string.IsNullOrWhiteSpace(input.Name) ? BuildAutoName(input.Type) : input.Name.Trim();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<ExperimentTemplateDto>(saved);
    }

    public async Task<ExperimentTemplateDto> UpdateAsync(ExperimentTemplateDto input)
    {
        var entity = await _repo.GetAsync(input.Id);
        if (entity is null)
        {
            return await CreateAsync(input);
        }

        entity.Type = input.Type;
        entity.ParameterId = input.ParameterId;
        entity.IsTemplate = true;
        entity.Name = string.IsNullOrWhiteSpace(input.Name) ? BuildAutoName(input.Type) : input.Name.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<ExperimentTemplateDto>(saved);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    private static string BuildAutoName(ExperimentType type)
        => $"{GetTypeDisplayName(type)}-{DateTime.Now:yyyyMMddHHmmss}";

    private static string GetTypeDisplayName(ExperimentType type) => type switch
    {
        ExperimentType.Reaction => "反应",
        ExperimentType.RotaryEvaporation => "旋蒸",
        ExperimentType.Detection => "检测",
        ExperimentType.Filtration => "过滤",
        ExperimentType.Drying => "干燥",
        ExperimentType.Quenching => "淬灭",
        ExperimentType.Extraction => "萃取",
        ExperimentType.Sampling => "取样",
        ExperimentType.Centrifugation => "离心",
        ExperimentType.CustomDetection => "自定义检测",
        _ => type.ToString()
    };
}

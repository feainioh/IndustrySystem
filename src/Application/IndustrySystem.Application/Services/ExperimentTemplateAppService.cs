using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class ExperimentTemplateAppService : IExperimentTemplateAppService
{
    private readonly IRepository<ExperimentTemplate> _repo;
    private readonly IMapper _mapper;
    public ExperimentTemplateAppService(IRepository<ExperimentTemplate> repo, IMapper mapper)
    { _repo = repo; _mapper = mapper; }

    public async Task<ExperimentTemplateDto?> GetAsync(Guid id)
        => (await _repo.GetAsync(id)) is { } e ? _mapper.Map<ExperimentTemplateDto>(e) : null;

    public async Task<List<ExperimentTemplateDto>> GetListAsync()
        => (await _repo.GetListAsync()).Select(_mapper.Map<ExperimentTemplateDto>).ToList();

    public async Task<ExperimentTemplateDto> CreateAsync(ExperimentTemplateDto input)
    {
        var entity = _mapper.Map<ExperimentTemplate>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<ExperimentTemplateDto>(saved);
    }

    public async Task<ExperimentTemplateDto> UpdateAsync(ExperimentTemplateDto input)
    {
        var saved = await _repo.UpdateAsync(_mapper.Map<ExperimentTemplate>(input));
        return _mapper.Map<ExperimentTemplateDto>(saved);
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);
}

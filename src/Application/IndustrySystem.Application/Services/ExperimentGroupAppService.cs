using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class ExperimentGroupAppService : IExperimentGroupAppService
{
    private readonly IRepository<ExperimentGroup> _groupRepo;
    private readonly IRepository<Experiment> _experimentRepo;
    private readonly IMapper _mapper;

    public ExperimentGroupAppService(
        IRepository<ExperimentGroup> groupRepo,
        IRepository<Experiment> experimentRepo,
        IMapper mapper)
    {
        _groupRepo = groupRepo;
        _experimentRepo = experimentRepo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ExperimentGroupDto>> GetListAsync()
    {
        var groups = await _groupRepo.GetListAsync();
        var experiments = await _experimentRepo.GetListAsync();
        var nameMap = experiments.ToDictionary(x => x.Id, x => x.Name);

        return groups
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.GroupCode)
            .Select(x => MapToDto(x, nameMap))
            .ToList();
    }

    public async Task<ExperimentGroupDto?> GetAsync(Guid id)
    {
        var entity = await _groupRepo.GetAsync(id);
        if (entity is null) return null;

        var experiments = await _experimentRepo.GetListAsync();
        var nameMap = experiments.ToDictionary(x => x.Id, x => x.Name);
        return MapToDto(entity, nameMap);
    }

    public async Task<ExperimentGroupDto> CreateAsync(ExperimentGroupDto input)
    {
        var entity = _mapper.Map<ExperimentGroup>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.GroupCode = string.IsNullOrWhiteSpace(input.GroupCode)
            ? await GenerateNextGroupCodeAsync(DateTime.Now)
            : input.GroupCode.Trim();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.StepExperimentIdList = input.StepExperimentIds.ToList();
        entity.Description = input.Description?.Trim() ?? string.Empty;

        var saved = await _groupRepo.InsertAsync(entity);

        var experiments = await _experimentRepo.GetListAsync();
        var nameMap = experiments.ToDictionary(x => x.Id, x => x.Name);
        return MapToDto(saved, nameMap);
    }

    public async Task<ExperimentGroupDto> UpdateAsync(ExperimentGroupDto input)
    {
        var existing = await _groupRepo.GetAsync(input.Id);
        if (existing is null)
        {
            return await CreateAsync(input);
        }

        existing.GroupCode = string.IsNullOrWhiteSpace(input.GroupCode)
            ? existing.GroupCode
            : input.GroupCode.Trim();
        existing.Name = input.Name;
        existing.Description = input.Description?.Trim() ?? string.Empty;
        existing.StepExperimentIdList = input.StepExperimentIds.ToList();
        existing.IsEnabled = input.IsEnabled;
        existing.CreatedBy = input.CreatedBy;
        existing.UpdatedAt = DateTime.UtcNow;

        var saved = await _groupRepo.UpdateAsync(existing);

        var experiments = await _experimentRepo.GetListAsync();
        var nameMap = experiments.ToDictionary(x => x.Id, x => x.Name);
        return MapToDto(saved, nameMap);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _groupRepo.DeleteAsync(id);
    }

    public async Task<IReadOnlyList<ExperimentOptionDto>> GetExperimentOptionsAsync()
    {
        var list = await _experimentRepo.GetListAsync();
        return list
            .OrderBy(x => x.Name)
            .Select(x => new ExperimentOptionDto(x.Id, x.Name))
            .ToList();
    }

    private async Task<string> GenerateNextGroupCodeAsync(DateTime now)
    {
        var prefix = $"EG{now:yyyyMM}";
        var list = await _groupRepo.GetListAsync();

        var maxSeq = list
            .Where(x => !string.IsNullOrWhiteSpace(x.GroupCode) && x.GroupCode.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(x =>
            {
                var suffix = x.GroupCode.Length >= prefix.Length + 3
                    ? x.GroupCode.Substring(prefix.Length, 3)
                    : string.Empty;
                return int.TryParse(suffix, out var seq) ? seq : 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{(maxSeq + 1):000}";
    }

    private static ExperimentGroupDto MapToDto(ExperimentGroup entity, IReadOnlyDictionary<Guid, string> experimentNameMap)
    {
        var stepNames = entity.StepExperimentIdList
            .Select(id => experimentNameMap.TryGetValue(id, out var name) ? name : null)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Cast<string>()
            .ToList();

        var display = stepNames.Count switch
        {
            0 => string.Empty,
            <= 3 => string.Join("、", stepNames),
            _ => $"{string.Join("、", stepNames.Take(3))} +{stepNames.Count - 3}"
        };

        return new ExperimentGroupDto(
            entity.Id,
            entity.GroupCode,
            entity.Name,
            entity.Description,
            entity.StepExperimentIdList,
            entity.IsEnabled,
            entity.CreatedBy,
            entity.CreatedAt,
            entity.UpdatedAt,
            display);
    }
}

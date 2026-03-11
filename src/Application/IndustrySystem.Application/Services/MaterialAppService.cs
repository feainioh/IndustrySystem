using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Materials;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class MaterialAppService : IMaterialAppService
{
    private readonly IRepository<Material> _repo;
    private readonly IMapper _mapper;

    public MaterialAppService(IRepository<Material> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<MaterialDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.OrderBy(x => x.Name)
                   .Select(_mapper.Map<MaterialDto>)
                   .ToList();
    }

    public async Task<MaterialDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<MaterialDto>(entity);
    }

    public async Task<MaterialDto> CreateAsync(MaterialDto input)
    {
        var entity = _mapper.Map<Material>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<MaterialDto>(saved);
    }

    public async Task<MaterialDto> UpdateAsync(MaterialDto input)
    {
        var entity = _mapper.Map<Material>(input);
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<MaterialDto>(saved);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }
}

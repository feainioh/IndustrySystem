using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Inventory;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class InventoryAppService : IInventoryAppService
{
    private readonly IRepository<InventoryRecord> _repo;
    private readonly IMapper _mapper;

    public InventoryAppService(IRepository<InventoryRecord> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<InventoryRecordDto>> GetListAsync()
    {
        var list = await _repo.GetListAsync();
        return list.OrderBy(x => x.MaterialName)
                   .Select(_mapper.Map<InventoryRecordDto>)
                   .ToList();
    }

    public async Task<InventoryRecordDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        return entity is null ? null : _mapper.Map<InventoryRecordDto>(entity);
    }

    public async Task<InventoryRecordDto> CreateAsync(InventoryRecordDto input)
    {
        var entity = _mapper.Map<InventoryRecord>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _repo.InsertAsync(entity);
        return _mapper.Map<InventoryRecordDto>(saved);
    }

    public async Task<InventoryRecordDto> UpdateAsync(InventoryRecordDto input)
    {
        var entity = _mapper.Map<InventoryRecord>(input);
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _repo.UpdateAsync(entity);
        return _mapper.Map<InventoryRecordDto>(saved);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task InboundAsync(Guid id, decimal qty)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null) return;
        entity.Quantity += qty;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }

    public async Task OutboundAsync(Guid id, decimal qty)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null) return;
        if (entity.Quantity < qty) return;
        entity.Quantity -= qty;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity);
    }
}

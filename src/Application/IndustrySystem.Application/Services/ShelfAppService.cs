using AutoMapper;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Inventory;
using IndustrySystem.Domain.Entities.Shelves;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class ShelfAppService : IShelfAppService
{
    private readonly IRepository<ContainerInfo> _containerRepo;
    private readonly IRepository<ShelfConfig> _shelfRepo;
    private readonly IRepository<ShelfSlot> _slotRepo;
    private readonly IRepository<InventoryRecord> _invRepo;
    private readonly IMapper _mapper;

    public ShelfAppService(
        IRepository<ContainerInfo> containerRepo,
        IRepository<ShelfConfig> shelfRepo,
        IRepository<ShelfSlot> slotRepo,
        IRepository<InventoryRecord> invRepo,
        IMapper mapper)
    {
        _containerRepo = containerRepo;
        _shelfRepo = shelfRepo;
        _slotRepo = slotRepo;
        _invRepo = invRepo;
        _mapper = mapper;
    }

    // ── 容器 ──

    public async Task<IReadOnlyList<ContainerInfoDto>> GetContainerListAsync()
    {
        var list = await _containerRepo.GetListAsync();
        return list.OrderBy(c => c.Name).Select(_mapper.Map<ContainerInfoDto>).ToList();
    }

    public async Task<ContainerInfoDto?> GetContainerAsync(Guid id)
    {
        var e = await _containerRepo.GetAsync(id);
        return e is null ? null : _mapper.Map<ContainerInfoDto>(e);
    }

    public async Task<ContainerInfoDto> CreateContainerAsync(ContainerInfoDto input)
    {
        var entity = _mapper.Map<ContainerInfo>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        var saved = await _containerRepo.InsertAsync(entity);
        return _mapper.Map<ContainerInfoDto>(saved);
    }

    public async Task<ContainerInfoDto> UpdateContainerAsync(ContainerInfoDto input)
    {
        var entity = _mapper.Map<ContainerInfo>(input);
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _containerRepo.UpdateAsync(entity);
        return _mapper.Map<ContainerInfoDto>(saved);
    }

    public async Task DeleteContainerAsync(Guid id)
        => await _containerRepo.DeleteAsync(id);

    // ── 货架 ──

    public async Task<IReadOnlyList<ShelfConfigDto>> GetShelfListAsync()
    {
        var list = await _shelfRepo.GetListAsync();
        return list.OrderBy(s => s.ShelfCode).Select(_mapper.Map<ShelfConfigDto>).ToList();
    }

    public async Task<ShelfConfigDto?> GetShelfAsync(Guid id)
    {
        var e = await _shelfRepo.GetAsync(id);
        return e is null ? null : _mapper.Map<ShelfConfigDto>(e);
    }

    public async Task<ShelfConfigDto> CreateShelfAsync(ShelfConfigDto input)
    {
        var entity = _mapper.Map<ShelfConfig>(input);
        entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
        entity.CreatedAt = DateTime.UtcNow;
        var saved = await _shelfRepo.InsertAsync(entity);

        // 自动生成全部槽位
        for (int r = 1; r <= entity.Rows; r++)
        for (int c = 1; c <= entity.Columns; c++)
            await _slotRepo.InsertAsync(new ShelfSlot { ShelfId = saved.Id, Row = r, Column = c });

        return _mapper.Map<ShelfConfigDto>(saved);
    }

    public async Task<ShelfConfigDto> UpdateShelfAsync(ShelfConfigDto input)
    {
        var entity = _mapper.Map<ShelfConfig>(input);
        entity.UpdatedAt = DateTime.UtcNow;
        var saved = await _shelfRepo.UpdateAsync(entity);
        return _mapper.Map<ShelfConfigDto>(saved);
    }

    public async Task DeleteShelfAsync(Guid id)
    {
        // 删除关联槽位
        var allSlots = await _slotRepo.GetListAsync();
        foreach (var s in allSlots.Where(s => s.ShelfId == id))
            await _slotRepo.DeleteAsync(s.Id);
        await _shelfRepo.DeleteAsync(id);
    }

    // ── 槽位 ──

    public async Task<IReadOnlyList<ShelfSlotDto>> GetSlotsByShelfAsync(Guid shelfId)
    {
        var allSlots = await _slotRepo.GetListAsync();
        var slots = allSlots.Where(s => s.ShelfId == shelfId).OrderBy(s => s.Row).ThenBy(s => s.Column).ToList();

        var containers = await _containerRepo.GetListAsync();
        var invRecords = await _invRepo.GetListAsync();

        return slots.Select(s =>
        {
            var container = s.ContainerId.HasValue ? containers.FirstOrDefault(c => c.Id == s.ContainerId.Value) : null;
            var inv = s.InventoryRecordId.HasValue ? invRecords.FirstOrDefault(i => i.Id == s.InventoryRecordId.Value) : null;
            return new ShelfSlotDto(
                s.Id, s.ShelfId, s.Row, s.Column,
                s.ContainerId, s.InventoryRecordId, s.IsDisabled, s.Remark,
                container?.Name, container?.ContainerType, container?.Rows, container?.Columns,
                inv?.MaterialName, inv?.Quantity, inv?.Unit);
        }).ToList();
    }

    public async Task SaveSlotsAsync(Guid shelfId, IReadOnlyList<ShelfSlotDto> slots)
    {
        foreach (var dto in slots)
        {
            var entity = await _slotRepo.GetAsync(dto.Id);
            if (entity is null) continue;
            entity.ContainerId = dto.ContainerId;
            entity.InventoryRecordId = dto.InventoryRecordId;
            entity.IsDisabled = dto.IsDisabled;
            entity.Remark = dto.Remark;
            await _slotRepo.UpdateAsync(entity);
        }
    }
}

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

        // 按槽位分组所有库存记录（通过 InventoryRecord.ShelfSlotId 关联）
        var invBySlot = invRecords
            .Where(i => i.ShelfSlotId.HasValue)
            .GroupBy(i => i.ShelfSlotId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<ShelfSlotDto>();

        foreach (var s in slots)
        {
            var container = s.ContainerId.HasValue
                ? containers.FirstOrDefault(c => c.Id == s.ContainerId.Value)
                : null;

            // 从库存表中查询该槽位关联的所有库存记录
            invBySlot.TryGetValue(s.Id, out var slotInvRecords);
            slotInvRecords ??= [];

            // 孔位级别的库存记录（WellRow > 0 && WellColumn > 0）
            var wellRecords = slotInvRecords
                .Where(i => i.WellRow > 0 && i.WellColumn > 0)
                .Select(i => new WellOccupancyDto(i.WellRow, i.WellColumn, i.MaterialName, i.Quantity, i.Unit))
                .ToList();

            // 从所有关联库存记录中汇总信息作为槽位级别显示
            string? materialName = null;
            decimal? totalQuantity = null;
            string? unit = null;
            int inventoryRecordCount = slotInvRecords.Count;

            if (slotInvRecords.Count > 0)
            {
                totalQuantity = slotInvRecords.Sum(i => i.Quantity);
                unit = slotInvRecords[0].Unit;

                // 获取不重复的物料名称
                var distinctMaterials = slotInvRecords
                    .Select(i => i.MaterialName)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();
                materialName = distinctMaterials.Count switch
                {
                    1 => distinctMaterials[0],
                    > 1 => $"{distinctMaterials[0]} 等{distinctMaterials.Count}种",
                    _ => null
                };
            }

            // 自动同步：将首条库存记录ID回写到槽位的 InventoryRecordId
            var primaryInvId = slotInvRecords.Count > 0 ? slotInvRecords[0].Id : (Guid?)null;
            if (s.InventoryRecordId != primaryInvId)
            {
                s.InventoryRecordId = primaryInvId;
                await _slotRepo.UpdateAsync(s);
            }

            result.Add(new ShelfSlotDto(
                s.Id, s.ShelfId, s.Row, s.Column,
                s.AllowedContainerTypeList,
                s.ContainerId, s.InventoryRecordId, s.IsDisabled, s.Remark,
                container?.Name, container?.ContainerType, container?.Rows, container?.Columns,
                materialName, totalQuantity, unit, wellRecords, inventoryRecordCount));
        }

        return result;
    }

    public async Task SaveSlotsAsync(Guid shelfId, IReadOnlyList<ShelfSlotDto> slots)
    {
        foreach (var dto in slots)
        {
            var entity = await _slotRepo.GetAsync(dto.Id);
            if (entity is null) continue;
            entity.AllowedContainerTypeList = dto.AllowedContainerTypes.ToList();
            entity.ContainerId = dto.ContainerId;
            entity.InventoryRecordId = dto.InventoryRecordId;
            entity.IsDisabled = dto.IsDisabled;
            entity.Remark = dto.Remark;
            await _slotRepo.UpdateAsync(entity);
        }
    }
}

using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IShelfAppService
{
    // 容器
    Task<IReadOnlyList<ContainerInfoDto>> GetContainerListAsync();
    Task<ContainerInfoDto?> GetContainerAsync(Guid id);
    Task<ContainerInfoDto> CreateContainerAsync(ContainerInfoDto input);
    Task<ContainerInfoDto> UpdateContainerAsync(ContainerInfoDto input);
    Task DeleteContainerAsync(Guid id);

    // 货架配置
    Task<IReadOnlyList<ShelfConfigDto>> GetShelfListAsync();
    Task<ShelfConfigDto?> GetShelfAsync(Guid id);
    Task<ShelfConfigDto> CreateShelfAsync(ShelfConfigDto input);
    Task<ShelfConfigDto> UpdateShelfAsync(ShelfConfigDto input);
    Task DeleteShelfAsync(Guid id);

    // 槽位
    Task<IReadOnlyList<ShelfSlotDto>> GetSlotsByShelfAsync(Guid shelfId);
    Task SaveSlotsAsync(Guid shelfId, IReadOnlyList<ShelfSlotDto> slots);
}

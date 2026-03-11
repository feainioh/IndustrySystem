using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IInventoryAppService
{
    Task<IReadOnlyList<InventoryRecordDto>> GetListAsync();
    Task<InventoryRecordDto?> GetAsync(Guid id);
    Task<InventoryRecordDto> CreateAsync(InventoryRecordDto input);
    Task<InventoryRecordDto> UpdateAsync(InventoryRecordDto input);
    Task DeleteAsync(Guid id);
    Task InboundAsync(Guid id, decimal qty);
    Task OutboundAsync(Guid id, decimal qty);
}

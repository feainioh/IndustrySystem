using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IInventoryAppService
{
    Task<IReadOnlyList<InventoryItemDto>> GetListAsync();
    Task InAsync(Guid id);
    Task OutAsync(Guid id);
}

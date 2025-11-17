using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

public class InventoryAppService : IInventoryAppService
{
    private static readonly ConcurrentDictionary<Guid, InventoryItemDto> _store = new();

    static InventoryAppService()
    {
        var a = new InventoryItemDto(Guid.NewGuid(), "物料A", 5);
        var b = new InventoryItemDto(Guid.NewGuid(), "物料B", 2);
        _store[a.Id] = a; _store[b.Id] = b;
    }

    public Task<IReadOnlyList<InventoryItemDto>> GetListAsync()
        => Task.FromResult<IReadOnlyList<InventoryItemDto>>(_store.Values.ToList());

    public Task InAsync(Guid id)
    {
        if (_store.TryGetValue(id, out var item)) _store[id] = item with { Qty = item.Qty + 1 };
        return Task.CompletedTask;
    }

    public Task OutAsync(Guid id)
    {
        if (_store.TryGetValue(id, out var item) && item.Qty > 0) _store[id] = item with { Qty = item.Qty - 1 };
        return Task.CompletedTask;
    }
}

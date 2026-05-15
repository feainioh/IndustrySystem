using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 告警应用服务（内存版示例实现）。
/// </summary>
public class AlarmAppService : IAlarmAppService
{
    // ===== State =====
    private static readonly ConcurrentDictionary<Guid, AlarmDto> _alarms = new();

    // ===== Initialization =====
    static AlarmAppService()
    {
        var a = new AlarmDto(Guid.NewGuid(), "过温", DateTime.Now.AddSeconds(-10), false);
        var b = new AlarmDto(Guid.NewGuid(), "欠压", DateTime.Now.AddSeconds(-5), false);
        _alarms[a.Id] = a; _alarms[b.Id] = b;
    }

    // ===== Queries =====
    public Task<IReadOnlyList<AlarmDto>> GetActiveAsync()
        => Task.FromResult<IReadOnlyList<AlarmDto>>(_alarms.Values.Where(x => !x.Acknowledged).ToList());

    // ===== Commands =====
    public Task AcknowledgeAsync(Guid id)
    {
        if (_alarms.TryGetValue(id, out var a))
            _alarms[id] = a with { Acknowledged = true };
        return Task.CompletedTask;
    }
}

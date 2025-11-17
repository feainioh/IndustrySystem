using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

public class AlarmAppService : IAlarmAppService
{
    private static readonly ConcurrentDictionary<Guid, AlarmDto> _alarms = new();

    static AlarmAppService()
    {
        var a = new AlarmDto(Guid.NewGuid(), "过温", DateTime.Now.AddSeconds(-10), false);
        var b = new AlarmDto(Guid.NewGuid(), "欠压", DateTime.Now.AddSeconds(-5), false);
        _alarms[a.Id] = a; _alarms[b.Id] = b;
    }

    public Task<IReadOnlyList<AlarmDto>> GetActiveAsync()
        => Task.FromResult<IReadOnlyList<AlarmDto>>(_alarms.Values.Where(x => !x.Acknowledged).ToList());

    public Task AcknowledgeAsync(Guid id)
    {
        if (_alarms.TryGetValue(id, out var a))
            _alarms[id] = a with { Acknowledged = true };
        return Task.CompletedTask;
    }
}

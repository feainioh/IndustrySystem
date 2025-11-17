using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

public class ExperimentAppService : IExperimentAppService
{
    private static readonly ConcurrentDictionary<Guid, ExperimentSummaryDto> Store = new();

    static ExperimentAppService()
    {
        var a = new ExperimentSummaryDto(Guid.NewGuid(), "示例实验A", "Idle");
        var b = new ExperimentSummaryDto(Guid.NewGuid(), "示例实验B", "Idle");
        Store[a.Id] = a; Store[b.Id] = b;
    }

    public Task<IReadOnlyList<ExperimentSummaryDto>> GetListAsync()
        => Task.FromResult<IReadOnlyList<ExperimentSummaryDto>>(Store.Values.ToList());

    public Task DeleteAsync(Guid id)
    {
        Store.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}

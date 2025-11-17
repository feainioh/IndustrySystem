using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

public class ExperimentHistoryAppService : IExperimentHistoryAppService
{
    private static readonly List<ExperimentHistoryDto> _hist = new()
    {
        new ExperimentHistoryDto(DateTime.Now.AddMinutes(-5), "示例实验A", "OK"),
        new ExperimentHistoryDto(DateTime.Now.AddMinutes(-2), "示例实验B", "Fail")
    };

    public Task<IReadOnlyList<ExperimentHistoryDto>> GetRecentAsync(int take = 50)
        => Task.FromResult<IReadOnlyList<ExperimentHistoryDto>>(_hist.OrderByDescending(x => x.Time).Take(take).ToList());
}

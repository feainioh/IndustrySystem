using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentHistoryAppService
{
    Task<IReadOnlyList<ExperimentHistoryDto>> GetRecentAsync(int take = 50);
}

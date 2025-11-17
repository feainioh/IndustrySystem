using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentAppService
{
    Task<IReadOnlyList<ExperimentSummaryDto>> GetListAsync();
    Task DeleteAsync(Guid id);
}

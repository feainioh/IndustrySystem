using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IAlarmAppService
{
    Task<IReadOnlyList<AlarmDto>> GetActiveAsync();
    Task AcknowledgeAsync(Guid id);
}

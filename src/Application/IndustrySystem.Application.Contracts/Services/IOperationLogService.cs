using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IOperationLogService
{
    Task<OperationLogDto> LogAsync(CreateOperationLogDto input);
    Task<List<OperationLogDto>> GetListAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task DeleteAsync(Guid id);
}

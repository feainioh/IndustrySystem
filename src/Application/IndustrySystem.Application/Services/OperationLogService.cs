using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities;
using IndustrySystem.Domain.Repositories;

namespace IndustrySystem.Application.Services;

public class OperationLogService : IOperationLogService
{
    private readonly IRepository<OperationLog> _repo;

    public OperationLogService(IRepository<OperationLog> repo)
    {
        _repo = repo;
    }

    public async Task<OperationLogDto> LogAsync(CreateOperationLogDto input)
    {
        var entity = new OperationLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.Now,
            Level = input.Level,
            OperationType = input.OperationType,
            Operator = input.Operator,
            Description = input.Description,
            IPAddress = input.IPAddress,
            Logger = input.Logger,
            ElapsedMs = input.ElapsedMs,
            IsSuccess = input.IsSuccess,
            ErrorMessage = input.ErrorMessage
        };
        var saved = await _repo.InsertAsync(entity);
        return MapToDto(saved);
    }

    public async Task<List<OperationLogDto>> GetListAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var list = await _repo.GetListAsync();
        var query = list.AsEnumerable();
        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);
        return query.OrderByDescending(l => l.Timestamp).Select(MapToDto).ToList();
    }

    public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

    private static OperationLogDto MapToDto(OperationLog entity) => new(
        entity.Id,
        entity.Timestamp,
        entity.Level,
        entity.OperationType,
        entity.Operator,
        entity.Description,
        entity.IPAddress,
        entity.Logger,
        entity.ElapsedMs,
        entity.IsSuccess,
        entity.ErrorMessage
    );
}

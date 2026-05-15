using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExternalDataSyncAppService
{
    Task<ExternalSyncRuntimeStatusDto> GetStatusAsync(CancellationToken ct = default);
    Task<int> SyncOnceAsync(CancellationToken ct = default);
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

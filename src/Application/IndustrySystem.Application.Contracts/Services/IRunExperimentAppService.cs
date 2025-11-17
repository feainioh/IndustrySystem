using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IRunExperimentAppService
{
    Task StartAsync();
    Task PauseAsync();
    Task ResumeAsync();
    Task StopAsync();
    Task<RunStatusDto> GetStatusAsync();
}

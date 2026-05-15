using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 实验运行控制服务契约。
/// </summary>
public interface IRunExperimentAppService
{
    /// <summary>
    /// 启动实验运行。
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 暂停实验运行。
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// 继续实验运行。
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// 停止实验运行。
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 获取当前运行状态快照。
    /// </summary>
    Task<RunStatusDto> GetStatusAsync();
}

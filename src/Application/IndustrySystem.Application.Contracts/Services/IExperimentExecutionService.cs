using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 实验组执行编排服务接口（仅业务逻辑，不含UI/状态轮询）。
/// </summary>
public interface IExperimentExecutionService
{
    /// <summary>
    /// 按实验组启动执行，返回执行上下文ID。
    /// </summary>
    Task<Guid> StartGroupAsync(Guid experimentGroupId);

    /// <summary>
    /// 获取指定组的执行状态。
    /// </summary>
    Task<ExperimentGroupExecutionStatusDto> GetGroupStatusAsync(Guid executionId);

    /// <summary>
    /// 暂停指定组的执行。
    /// </summary>
    Task PauseGroupAsync(Guid executionId);

    /// <summary>
    /// 恢复指定组的执行。
    /// </summary>
    Task ResumeGroupAsync(Guid executionId);

    /// <summary>
    /// 停止指定组的执行。
    /// </summary>
    Task StopGroupAsync(Guid executionId);
}

/// <summary>
/// 实验组执行状态DTO。
/// </summary>
public record ExperimentGroupExecutionStatusDto(
    Guid ExecutionId,
    Guid GroupId,
    string GroupName,
    int CurrentStepIndex,
    int TotalSteps,
    RunState State,
    string? Message = null
);
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 实验组执行模拟服务，仅用于界面开发与联调。
/// </summary>
public class MockExperimentExecutionService : IExperimentExecutionService
{
    private readonly ConcurrentDictionary<Guid, (Guid groupId, string groupName, int totalSteps, int currentStep, RunState state)> _executions = new();

    public Task<Guid> StartGroupAsync(Guid experimentGroupId)
    {
        // 模拟：每次启动新上下文
        var execId = Guid.NewGuid();
        _executions[execId] = (experimentGroupId, $"模拟组-{experimentGroupId.ToString()[..8]}", 5, 0, RunState.Running);
        return Task.FromResult(execId);
    }

    public Task<ExperimentGroupExecutionStatusDto> GetGroupStatusAsync(Guid executionId)
    {
        if (_executions.TryGetValue(executionId, out var ctx))
        {
            // 模拟进度推进
            if (ctx.state == RunState.Running && ctx.currentStep < ctx.totalSteps)
            {
                _executions[executionId] = (ctx.groupId, ctx.groupName, ctx.totalSteps, ctx.currentStep + 1, ctx.currentStep + 1 == ctx.totalSteps ? RunState.Completed : RunState.Running);
            }
            var updated = _executions[executionId];
            return Task.FromResult(new ExperimentGroupExecutionStatusDto(
                executionId,
                updated.groupId,
                updated.groupName,
                updated.currentStep,
                updated.totalSteps,
                updated.state,
                updated.state == RunState.Completed ? "已完成" : null
            ));
        }
        return Task.FromResult(new ExperimentGroupExecutionStatusDto(executionId, Guid.Empty, "未知组", 0, 0, RunState.Stopped, "未找到"));
    }

    public Task PauseGroupAsync(Guid executionId)
    {
        if (_executions.TryGetValue(executionId, out var ctx))
            _executions[executionId] = (ctx.groupId, ctx.groupName, ctx.totalSteps, ctx.currentStep, RunState.Paused);
        return Task.CompletedTask;
    }

    public Task ResumeGroupAsync(Guid executionId)
    {
        if (_executions.TryGetValue(executionId, out var ctx))
            _executions[executionId] = (ctx.groupId, ctx.groupName, ctx.totalSteps, ctx.currentStep, RunState.Running);
        return Task.CompletedTask;
    }

    public Task StopGroupAsync(Guid executionId)
    {
        if (_executions.TryGetValue(executionId, out var ctx))
            _executions[executionId] = (ctx.groupId, ctx.groupName, ctx.totalSteps, ctx.currentStep, RunState.Stopped);
        return Task.CompletedTask;
    }
}
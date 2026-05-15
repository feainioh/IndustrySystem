using System;
using System.Threading;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 实验运行模拟服务。
/// 用于在未接入真实执行器前提供可观测的运行状态。
/// </summary>
public class RunExperimentAppService : IRunExperimentAppService
{
    // ===== Configuration =====
    private const int ProgressIncrementPerTick = 2;
    private static readonly TimeSpan ProgressTickInterval = TimeSpan.FromMilliseconds(100);

    // ===== State =====
    private readonly object _lock = new();
    private RunState _state = RunState.Idle;
    private int _progress = 0;
    private string? _message = null;
    private CancellationTokenSource? _cts;
    private Task? _runner;

    // ===== Queries =====
    public Task<RunStatusDto> GetStatusAsync()
        => Task.FromResult(new RunStatusDto(_state, _progress, _message));

    // ===== Commands =====
    public Task PauseAsync()
    {
        lock (_lock)
        {
            if (_state == RunState.Running) _state = RunState.Paused;
        }
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        lock (_lock)
        {
            if (_state == RunState.Paused) _state = RunState.Running;
        }
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        lock (_lock)
        {
            _cts?.Cancel();
            _state = RunState.Stopped;
        }
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        lock (_lock)
        {
            if (_state == RunState.Running || _state == RunState.Paused) return Task.CompletedTask;
            _progress = 0;
            _message = null;
            _state = RunState.Running;
            _cts = new CancellationTokenSource();
            _runner = Task.Run(async () => await RunLoop(_cts.Token));
        }
        return Task.CompletedTask;
    }

    // ===== Execution Loop =====
    private async Task RunLoop(CancellationToken ct)
    {
        try
        {
            while (_progress < 100 && !ct.IsCancellationRequested)
            {
                await Task.Delay(ProgressTickInterval, ct);
                lock (_lock)
                {
                    if (_state == RunState.Paused) continue;
                    if (_state != RunState.Running) break;
                    _progress += ProgressIncrementPerTick;
                }
            }
            lock (_lock)
            {
                if (_progress >= 100 && !ct.IsCancellationRequested)
                {
                    _progress = 100;
                    _state = RunState.Completed;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // stopped
        }
    }
}

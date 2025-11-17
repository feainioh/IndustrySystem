using System;
using System.Threading;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;

namespace IndustrySystem.Application.Services;

public class RunExperimentAppService : IRunExperimentAppService
{
    private readonly object _lock = new();
    private RunState _state = RunState.Idle;
    private int _progress = 0;
    private string? _message = null;
    private CancellationTokenSource? _cts;
    private Task? _runner;

    public Task<RunStatusDto> GetStatusAsync()
        => Task.FromResult(new RunStatusDto(_state, _progress, _message));

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

    private async Task RunLoop(CancellationToken ct)
    {
        try
        {
            while (_progress < 100 && !ct.IsCancellationRequested)
            {
                await Task.Delay(100, ct);
                lock (_lock)
                {
                    if (_state == RunState.Paused) continue;
                    if (_state != RunState.Running) break;
                    _progress += 2;
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

namespace IndustrySystem.Application.Contracts.Dtos;

public enum RunState { Idle, Running, Paused, Stopped, Completed }
public enum StepState { Pending, Running, Completed }

public record RunStatusDto(RunState State, int Progress, string? Message);

/// <summary>实验执行记录</summary>
public record ExperimentExecutionDto(
    string ExecId,
    string ExperimentName,
    RunState State,
    string StepProgress,
    TimeSpan Elapsed,
    DateTime? StartTime);

/// <summary>实验步骤</summary>
public record ExperimentStepDto(int Index, string Name, StepState State);

/// <summary>关键指标</summary>
public record KeyMetricDto(string Label, string Value, string Unit);

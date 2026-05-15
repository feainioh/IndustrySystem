namespace IndustrySystem.Application.Contracts.Dtos;

public enum RunState { Idle, Running, Paused, Stopped, Completed }
public enum StepState { Pending, Running, Completed, Failed }
public record RunStatusDto(RunState State, int Progress, string? Message);
public record ExperimentExecutionDto(string ExecId, string ExperimentName, RunState State, string StepProgress, TimeSpan Elapsed, DateTime? StartTime);
public record ExperimentStepDto(int Order, string Name, StepState State);
public record KeyMetricDto(string Label, string Value, string Unit);

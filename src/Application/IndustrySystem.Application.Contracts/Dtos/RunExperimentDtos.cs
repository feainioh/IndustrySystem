namespace IndustrySystem.Application.Contracts.Dtos;

public enum RunState { Idle, Running, Paused, Stopped, Completed }
public record RunStatusDto(RunState State, int Progress, string? Message);

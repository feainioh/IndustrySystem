using System;

namespace IndustrySystem.Application.Contracts.Dtos;

public record ExperimentSummaryDto(Guid Id, string Name, string Status);
public record ExperimentHistoryDto(DateTime Time, string Name, string Result);
public record AlarmDto(Guid Id, string Message, DateTime Time, bool Acknowledged);

using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Repositories;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Services;

public class ExperimentParameterAppService : IExperimentParameterAppService
{
    private readonly IRepository<ReactionParameter> _reactionRepo;
    private readonly IRepository<RotaryEvaporationParameter> _rotaryRepo;
    private readonly IRepository<DetectionParameter> _detectionRepo;
    private readonly IRepository<FiltrationParameter> _filtrationRepo;
    private readonly IRepository<DryingParameter> _dryingRepo;
    private readonly IRepository<QuenchingParameter> _quenchingRepo;
    private readonly IRepository<ExtractionParameter> _extractionRepo;
    private readonly IRepository<SamplingParameter> _samplingRepo;
    private readonly IRepository<CentrifugationParameter> _centrifugationRepo;
    private readonly IRepository<CustomDetectionParameter> _customDetectionRepo;

    public ExperimentParameterAppService(
        IRepository<ReactionParameter> reactionRepo,
        IRepository<RotaryEvaporationParameter> rotaryRepo,
        IRepository<DetectionParameter> detectionRepo,
        IRepository<FiltrationParameter> filtrationRepo,
        IRepository<DryingParameter> dryingRepo,
        IRepository<QuenchingParameter> quenchingRepo,
        IRepository<ExtractionParameter> extractionRepo,
        IRepository<SamplingParameter> samplingRepo,
        IRepository<CentrifugationParameter> centrifugationRepo,
        IRepository<CustomDetectionParameter> customDetectionRepo)
    {
        _reactionRepo = reactionRepo;
        _rotaryRepo = rotaryRepo;
        _detectionRepo = detectionRepo;
        _filtrationRepo = filtrationRepo;
        _dryingRepo = dryingRepo;
        _quenchingRepo = quenchingRepo;
        _extractionRepo = extractionRepo;
        _samplingRepo = samplingRepo;
        _centrifugationRepo = centrifugationRepo;
        _customDetectionRepo = customDetectionRepo;
    }

    public async Task<IReadOnlyList<ExperimentParameterOptionDto>> GetOptionsAsync(ExperimentType type)
        => (await GetListAsync(type)).Select(x => new ExperimentParameterOptionDto(x.Id, x.Name, x.Type)).ToList();

    public async Task<IReadOnlyList<ExperimentParameterItemDto>> GetListAsync(ExperimentType type)
    {
        return type switch
        {
            ExperimentType.Reaction => (await _reactionRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.RotaryEvaporation => (await _rotaryRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Detection => (await _detectionRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Filtration => (await _filtrationRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Drying => (await _dryingRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Quenching => (await _quenchingRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Extraction => (await _extractionRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Sampling => (await _samplingRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.Centrifugation => (await _centrifugationRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            ExperimentType.CustomDetection => (await _customDetectionRepo.GetListAsync()).OrderBy(x => x.Name).Select(Map).ToList(),
            _ => []
        };
    }

    public async Task<ExperimentParameterItemDto?> GetAsync(ExperimentType type, Guid id)
    {
        return type switch
        {
            ExperimentType.Reaction => (await _reactionRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.RotaryEvaporation => (await _rotaryRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Detection => (await _detectionRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Filtration => (await _filtrationRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Drying => (await _dryingRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Quenching => (await _quenchingRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Extraction => (await _extractionRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Sampling => (await _samplingRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.Centrifugation => (await _centrifugationRepo.GetAsync(id)) is { } r ? Map(r) : null,
            ExperimentType.CustomDetection => (await _customDetectionRepo.GetAsync(id)) is { } r ? Map(r) : null,
            _ => null
        };
    }

    public async Task<ExperimentParameterItemDto> CreateAsync(ExperimentParameterItemDto input)
    {
        var id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id;
        return input.Type switch
        {
            ExperimentType.Reaction => Map(await _reactionRepo.InsertAsync(new ReactionParameter
            {
                Id = id, Name = input.Name,
                RawMaterial = input.RawMaterial, StockSolution = input.StockSolution,
                TemperatureC = input.TemperatureC, PressureKpa = input.PressureKpa,
                DurationMinutes = input.DurationMinutes, StirSpeedRpm = input.StirSpeedRpm,
                LiquidAddSpeedMlMin = input.LiquidAddSpeedMlMin, PowderAddSpeedGMin = input.PowderAddSpeedGMin,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.RotaryEvaporation => Map(await _rotaryRepo.InsertAsync(new RotaryEvaporationParameter
            {
                Id = id, Name = input.Name,
                BathTemperatureC = input.BathTemperatureC, VaporTemperatureC = input.VaporTemperatureC,
                VacuumKpa = input.VacuumKpa, RotationRpm = input.RotationRpm,
                LiftStrokeMm = input.LiftStrokeMm, CoolantTemperatureC = input.CoolantTemperatureC,
                CollectCondensate = input.CollectCondensate ?? false, ContinuousFeed = input.ContinuousFeed ?? false,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Detection => Map(await _detectionRepo.InsertAsync(new DetectionParameter
            {
                Id = id, Name = input.Name,
                Method = input.Method, WavelengthNm = input.WavelengthNm,
                DurationMinutes = input.DurationMinutes, Notes = input.Notes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Filtration => Map(await _filtrationRepo.InsertAsync(new FiltrationParameter
            {
                Id = id, Name = input.Name,
                DurationMinutes = input.DurationMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Drying => Map(await _dryingRepo.InsertAsync(new DryingParameter
            {
                Id = id, Name = input.Name,
                DesiccantId = input.DesiccantId, DesiccantVolumeMl = input.DesiccantVolumeMl,
                ShakeSpeedRpm = input.ShakeSpeedRpm, ShakeDurationMinutes = input.ShakeDurationMinutes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Quenching => Map(await _quenchingRepo.InsertAsync(new QuenchingParameter
            {
                Id = id, Name = input.Name,
                QuenchingAgent = input.QuenchingAgent, QuenchingAgentVolumeMl = input.QuenchingAgentVolumeMl,
                QuenchingAgentDripSpeedMlMin = input.QuenchingAgentDripSpeedMlMin,
                AddQuenchingAgentFirst = input.AddQuenchingAgentFirst ?? false,
                PreTemperatureC = input.PreTemperatureC, MaxTemperatureC = input.MaxTemperatureC,
                StirSpeedRpm = input.StirSpeedRpm, DurationMinutes = input.DurationMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                TotalProductVolumeMl = input.TotalProductVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Extraction => Map(await _extractionRepo.InsertAsync(new ExtractionParameter
            {
                Id = id, Name = input.Name,
                ExtractAgent = input.ExtractAgent, ExtractAgentVolumeMl = input.ExtractAgentVolumeMl,
                StirSpeedRpm = input.StirSpeedRpm, StirDurationMinutes = input.StirDurationMinutes,
                SettlingMinutes = input.SettlingMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Sampling => Map(await _samplingRepo.InsertAsync(new SamplingParameter
            {
                Id = id, Name = input.Name,
                SampleVolumeMl = input.SampleVolumeMl,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Centrifugation => Map(await _centrifugationRepo.InsertAsync(new CentrifugationParameter
            {
                Id = id, Name = input.Name,
                SpeedRpm = input.SpeedRpm, TemperatureC = input.TemperatureC,
                DurationMinutes = input.DurationMinutes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.CustomDetection => Map(await _customDetectionRepo.InsertAsync(new CustomDetectionParameter
            {
                Id = id, Name = input.Name,
                Method = input.Method, ParameterJson = input.ParameterJson,
                Notes = input.Notes,
                UpdatedAt = DateTime.UtcNow
            })),
            _ => throw new NotSupportedException($"Unsupported type: {input.Type}")
        };
    }

    public async Task<ExperimentParameterItemDto> UpdateAsync(ExperimentParameterItemDto input)
    {
        if (input.Id == Guid.Empty) return await CreateAsync(input);

        return input.Type switch
        {
            ExperimentType.Reaction => Map(await _reactionRepo.UpdateAsync(new ReactionParameter
            {
                Id = input.Id, Name = input.Name,
                RawMaterial = input.RawMaterial, StockSolution = input.StockSolution,
                TemperatureC = input.TemperatureC, PressureKpa = input.PressureKpa,
                DurationMinutes = input.DurationMinutes, StirSpeedRpm = input.StirSpeedRpm,
                LiquidAddSpeedMlMin = input.LiquidAddSpeedMlMin, PowderAddSpeedGMin = input.PowderAddSpeedGMin,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.RotaryEvaporation => Map(await _rotaryRepo.UpdateAsync(new RotaryEvaporationParameter
            {
                Id = input.Id, Name = input.Name,
                BathTemperatureC = input.BathTemperatureC, VaporTemperatureC = input.VaporTemperatureC,
                VacuumKpa = input.VacuumKpa, RotationRpm = input.RotationRpm,
                LiftStrokeMm = input.LiftStrokeMm, CoolantTemperatureC = input.CoolantTemperatureC,
                CollectCondensate = input.CollectCondensate ?? false, ContinuousFeed = input.ContinuousFeed ?? false,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Detection => Map(await _detectionRepo.UpdateAsync(new DetectionParameter
            {
                Id = input.Id, Name = input.Name,
                Method = input.Method, WavelengthNm = input.WavelengthNm,
                DurationMinutes = input.DurationMinutes, Notes = input.Notes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Filtration => Map(await _filtrationRepo.UpdateAsync(new FiltrationParameter
            {
                Id = input.Id, Name = input.Name,
                DurationMinutes = input.DurationMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Drying => Map(await _dryingRepo.UpdateAsync(new DryingParameter
            {
                Id = input.Id, Name = input.Name,
                DesiccantId = input.DesiccantId, DesiccantVolumeMl = input.DesiccantVolumeMl,
                ShakeSpeedRpm = input.ShakeSpeedRpm, ShakeDurationMinutes = input.ShakeDurationMinutes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Quenching => Map(await _quenchingRepo.UpdateAsync(new QuenchingParameter
            {
                Id = input.Id, Name = input.Name,
                QuenchingAgent = input.QuenchingAgent, QuenchingAgentVolumeMl = input.QuenchingAgentVolumeMl,
                QuenchingAgentDripSpeedMlMin = input.QuenchingAgentDripSpeedMlMin,
                AddQuenchingAgentFirst = input.AddQuenchingAgentFirst ?? false,
                PreTemperatureC = input.PreTemperatureC, MaxTemperatureC = input.MaxTemperatureC,
                StirSpeedRpm = input.StirSpeedRpm, DurationMinutes = input.DurationMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                TotalProductVolumeMl = input.TotalProductVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Extraction => Map(await _extractionRepo.UpdateAsync(new ExtractionParameter
            {
                Id = input.Id, Name = input.Name,
                ExtractAgent = input.ExtractAgent, ExtractAgentVolumeMl = input.ExtractAgentVolumeMl,
                StirSpeedRpm = input.StirSpeedRpm, StirDurationMinutes = input.StirDurationMinutes,
                SettlingMinutes = input.SettlingMinutes,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Sampling => Map(await _samplingRepo.UpdateAsync(new SamplingParameter
            {
                Id = input.Id, Name = input.Name,
                SampleVolumeMl = input.SampleVolumeMl,
                Detergent = input.Detergent, DetergentVolumeMl = input.DetergentVolumeMl,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.Centrifugation => Map(await _centrifugationRepo.UpdateAsync(new CentrifugationParameter
            {
                Id = input.Id, Name = input.Name,
                SpeedRpm = input.SpeedRpm, TemperatureC = input.TemperatureC,
                DurationMinutes = input.DurationMinutes,
                UpdatedAt = DateTime.UtcNow
            })),
            ExperimentType.CustomDetection => Map(await _customDetectionRepo.UpdateAsync(new CustomDetectionParameter
            {
                Id = input.Id, Name = input.Name,
                Method = input.Method, ParameterJson = input.ParameterJson,
                Notes = input.Notes,
                UpdatedAt = DateTime.UtcNow
            })),
            _ => throw new NotSupportedException($"Unsupported type: {input.Type}")
        };
    }

    public async Task DeleteAsync(ExperimentType type, Guid id)
    {
        switch (type)
        {
            case ExperimentType.Reaction: await _reactionRepo.DeleteAsync(id); break;
            case ExperimentType.RotaryEvaporation: await _rotaryRepo.DeleteAsync(id); break;
            case ExperimentType.Detection: await _detectionRepo.DeleteAsync(id); break;
            case ExperimentType.Filtration: await _filtrationRepo.DeleteAsync(id); break;
            case ExperimentType.Drying: await _dryingRepo.DeleteAsync(id); break;
            case ExperimentType.Quenching: await _quenchingRepo.DeleteAsync(id); break;
            case ExperimentType.Extraction: await _extractionRepo.DeleteAsync(id); break;
            case ExperimentType.Sampling: await _samplingRepo.DeleteAsync(id); break;
            case ExperimentType.Centrifugation: await _centrifugationRepo.DeleteAsync(id); break;
            case ExperimentType.CustomDetection: await _customDetectionRepo.DeleteAsync(id); break;
            default: throw new NotSupportedException($"Unsupported type: {type}");
        }
    }

    // ========== Map methods ==========

    private static ExperimentParameterItemDto Map(ReactionParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Reaction, Name = x.Name,
        RawMaterial = x.RawMaterial, StockSolution = x.StockSolution,
        TemperatureC = x.TemperatureC, PressureKpa = x.PressureKpa,
        DurationMinutes = x.DurationMinutes, StirSpeedRpm = x.StirSpeedRpm,
        LiquidAddSpeedMlMin = x.LiquidAddSpeedMlMin, PowderAddSpeedGMin = x.PowderAddSpeedGMin,
        Detergent = x.Detergent, DetergentVolumeMl = x.DetergentVolumeMl,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(RotaryEvaporationParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.RotaryEvaporation, Name = x.Name,
        BathTemperatureC = x.BathTemperatureC, VaporTemperatureC = x.VaporTemperatureC,
        VacuumKpa = x.VacuumKpa, RotationRpm = x.RotationRpm,
        LiftStrokeMm = x.LiftStrokeMm, CoolantTemperatureC = x.CoolantTemperatureC,
        CollectCondensate = x.CollectCondensate, ContinuousFeed = x.ContinuousFeed,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(DetectionParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Detection, Name = x.Name,
        Method = x.Method, WavelengthNm = x.WavelengthNm,
        DurationMinutes = x.DurationMinutes, Notes = x.Notes,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(FiltrationParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Filtration, Name = x.Name,
        DurationMinutes = x.DurationMinutes,
        Detergent = x.Detergent, DetergentVolumeMl = x.DetergentVolumeMl,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(DryingParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Drying, Name = x.Name,
        DesiccantId = x.DesiccantId, DesiccantVolumeMl = x.DesiccantVolumeMl,
        ShakeSpeedRpm = x.ShakeSpeedRpm, ShakeDurationMinutes = x.ShakeDurationMinutes,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(QuenchingParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Quenching, Name = x.Name,
        QuenchingAgent = x.QuenchingAgent, QuenchingAgentVolumeMl = x.QuenchingAgentVolumeMl,
        QuenchingAgentDripSpeedMlMin = x.QuenchingAgentDripSpeedMlMin,
        AddQuenchingAgentFirst = x.AddQuenchingAgentFirst,
        PreTemperatureC = x.PreTemperatureC, MaxTemperatureC = x.MaxTemperatureC,
        StirSpeedRpm = x.StirSpeedRpm, DurationMinutes = x.DurationMinutes,
        Detergent = x.Detergent, DetergentVolumeMl = x.DetergentVolumeMl,
        TotalProductVolumeMl = x.TotalProductVolumeMl,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(ExtractionParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Extraction, Name = x.Name,
        ExtractAgent = x.ExtractAgent, ExtractAgentVolumeMl = x.ExtractAgentVolumeMl,
        StirSpeedRpm = x.StirSpeedRpm, StirDurationMinutes = x.StirDurationMinutes,
        SettlingMinutes = x.SettlingMinutes,
        Detergent = x.Detergent, DetergentVolumeMl = x.DetergentVolumeMl,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(SamplingParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Sampling, Name = x.Name,
        SampleVolumeMl = x.SampleVolumeMl,
        Detergent = x.Detergent, DetergentVolumeMl = x.DetergentVolumeMl,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(CentrifugationParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.Centrifugation, Name = x.Name,
        SpeedRpm = x.SpeedRpm, TemperatureC = x.TemperatureC,
        DurationMinutes = x.DurationMinutes,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ExperimentParameterItemDto Map(CustomDetectionParameter x) => new()
    {
        Id = x.Id, Type = ExperimentType.CustomDetection, Name = x.Name,
        Method = x.Method, ParameterJson = x.ParameterJson, Notes = x.Notes,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Repositories;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Services;

/// <summary>
/// 实验配置应用服务实现（持久化版）。
/// </summary>
public class ExperimentAppService : IExperimentAppService
{
    private readonly IRepository<Experiment> _repo;

    public ExperimentAppService(IRepository<Experiment> repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<ExperimentSummaryDto>> GetListAsync()
        => (await _repo.GetListAsync())
            .Where(x => !x.IsTemplate)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Select(MapSummary)
            .ToList();

    public async Task<ExperimentConfigItemDto?> GetAsync(Guid id)
    {
        var entity = await _repo.GetAsync(id);
        if (entity is null || entity.IsTemplate)
        {
            return null;
        }

        return MapConfig(entity);
    }

    public async Task<ExperimentConfigItemDto> CreateAsync(ExperimentConfigUpsertDto input)
    {
        var now = DateTime.UtcNow;
        var entity = new Experiment
        {
            Id = input.Id == Guid.Empty ? Guid.NewGuid() : input.Id,
            Name = NormalizeName(input.Name, input.Type),
            Type = input.Type,
            ParameterId = input.ParameterId,
            IsTemplate = false,
            GroupId = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var saved = await _repo.InsertAsync(entity);
        return MapConfig(saved);
    }

    public async Task<ExperimentConfigItemDto> UpdateAsync(ExperimentConfigUpsertDto input)
    {
        var existing = await _repo.GetAsync(input.Id);
        if (existing is null || existing.IsTemplate)
        {
            return await CreateAsync(input with { Id = Guid.Empty });
        }

        existing.Name = NormalizeName(input.Name, input.Type);
        existing.Type = input.Type;
        existing.ParameterId = input.ParameterId;
        existing.IsTemplate = false;
        existing.UpdatedAt = DateTime.UtcNow;

        var saved = await _repo.UpdateAsync(existing);
        return MapConfig(saved);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _repo.GetAsync(id);
        if (existing is null || existing.IsTemplate)
        {
            return;
        }

        await _repo.DeleteAsync(id);
    }

    private static ExperimentSummaryDto MapSummary(Experiment entity)
    {
        var status = entity.ParameterId.HasValue ? "已配置" : "待配置";
        return new ExperimentSummaryDto(entity.Id, entity.Name, status);
    }

    private static ExperimentConfigItemDto MapConfig(Experiment entity)
        => new(
            entity.Id,
            entity.Name,
            entity.Type,
            entity.ParameterId,
            entity.CreatedAt,
            entity.UpdatedAt);

    private static string NormalizeName(string? rawName, ExperimentType type)
    {
        if (!string.IsNullOrWhiteSpace(rawName))
        {
            return rawName.Trim();
        }

        return $"{GetTypeDisplayName(type)}-{DateTime.Now:yyyyMMddHHmmss}";
    }

    private static string GetTypeDisplayName(ExperimentType type) => type switch
    {
        ExperimentType.Reaction => "反应",
        ExperimentType.RotaryEvaporation => "旋蒸",
        ExperimentType.Detection => "检测",
        ExperimentType.Filtration => "过滤",
        ExperimentType.Drying => "干燥",
        ExperimentType.Quenching => "淬灭",
        ExperimentType.Extraction => "萃取",
        ExperimentType.Sampling => "取样",
        ExperimentType.Centrifugation => "离心",
        ExperimentType.CustomDetection => "自定义检测",
        _ => type.ToString()
    };
}

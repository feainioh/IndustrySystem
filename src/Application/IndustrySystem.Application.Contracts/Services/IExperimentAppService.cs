using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

public interface IExperimentAppService
{
    /// <summary>
    /// 获取实验简要列表（仅包含可执行实验，不含模板）。
    /// </summary>
    Task<IReadOnlyList<ExperimentSummaryDto>> GetListAsync();

    /// <summary>
    /// 按标识获取实验配置详情。
    /// </summary>
    Task<ExperimentConfigItemDto?> GetAsync(Guid id);

    /// <summary>
    /// 新建实验配置。
    /// </summary>
    Task<ExperimentConfigItemDto> CreateAsync(ExperimentConfigUpsertDto input);

    /// <summary>
    /// 更新实验配置。
    /// </summary>
    Task<ExperimentConfigItemDto> UpdateAsync(ExperimentConfigUpsertDto input);

    /// <summary>
    /// 删除实验配置。
    /// </summary>
    Task DeleteAsync(Guid id);
}

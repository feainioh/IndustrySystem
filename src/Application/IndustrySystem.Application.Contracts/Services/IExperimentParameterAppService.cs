using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 实验参数应用服务契约。
/// </summary>
public interface IExperimentParameterAppService
{
    /// <summary>
    /// 获取某一实验类型的参数选项（用于下拉选择）。
    /// </summary>
    Task<IReadOnlyList<ExperimentParameterOptionDto>> GetOptionsAsync(ExperimentType type);

    /// <summary>
    /// 获取某一实验类型的参数列表。
    /// </summary>
    Task<IReadOnlyList<ExperimentParameterItemDto>> GetListAsync(ExperimentType type);

    /// <summary>
    /// 按实验类型与参数标识查询参数详情。
    /// </summary>
    Task<ExperimentParameterItemDto?> GetAsync(ExperimentType type, Guid id);

    /// <summary>
    /// 创建实验参数。
    /// </summary>
    Task<ExperimentParameterItemDto> CreateAsync(ExperimentParameterItemDto input);

    /// <summary>
    /// 更新实验参数。
    /// </summary>
    Task<ExperimentParameterItemDto> UpdateAsync(ExperimentParameterItemDto input);

    /// <summary>
    /// 删除指定实验类型下的参数。
    /// </summary>
    Task DeleteAsync(ExperimentType type, Guid id);
}

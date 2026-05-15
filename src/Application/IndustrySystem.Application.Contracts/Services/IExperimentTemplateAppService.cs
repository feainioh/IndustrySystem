using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 实验模板应用服务契约。
/// </summary>
public interface IExperimentTemplateAppService
{
    /// <summary>
    /// 按模板标识查询模板详情。
    /// </summary>
    Task<ExperimentTemplateDto?> GetAsync(Guid id);

    /// <summary>
    /// 获取全部实验模板列表。
    /// </summary>
    Task<List<ExperimentTemplateDto>> GetListAsync();

    /// <summary>
    /// 新建实验模板。
    /// </summary>
    Task<ExperimentTemplateDto> CreateAsync(ExperimentTemplateDto input);

    /// <summary>
    /// 更新实验模板。
    /// </summary>
    Task<ExperimentTemplateDto> UpdateAsync(ExperimentTemplateDto input);

    /// <summary>
    /// 删除指定模板。
    /// </summary>
    Task DeleteAsync(Guid id);
}

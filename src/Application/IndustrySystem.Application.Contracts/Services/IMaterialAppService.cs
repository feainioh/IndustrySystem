using IndustrySystem.Application.Contracts.Dtos;

namespace IndustrySystem.Application.Contracts.Services;

/// <summary>
/// 物料主数据应用服务契约。
/// </summary>
public interface IMaterialAppService
{
    /// <summary>
    /// 获取全部物料。
    /// </summary>
    Task<IReadOnlyList<MaterialDto>> GetListAsync();

    /// <summary>
    /// 按标识查询物料。
    /// </summary>
    Task<MaterialDto?> GetAsync(Guid id);

    /// <summary>
    /// 新建物料。
    /// </summary>
    Task<MaterialDto> CreateAsync(MaterialDto input);

    /// <summary>
    /// 更新物料。
    /// </summary>
    Task<MaterialDto> UpdateAsync(MaterialDto input);

    /// <summary>
    /// 删除物料。
    /// </summary>
    Task DeleteAsync(Guid id);
}

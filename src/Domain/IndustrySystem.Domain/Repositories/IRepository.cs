namespace IndustrySystem.Domain.Repositories;

/// <summary>
/// 通用仓储契约，定义最小化 CRUD 操作。
/// </summary>
/// <typeparam name="TEntity">聚合根或实体类型。</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// 按主键查询实体。
    /// </summary>
    Task<TEntity?> GetAsync(Guid id);

    /// <summary>
    /// 查询全部实体。
    /// </summary>
    Task<List<TEntity>> GetListAsync();

    /// <summary>
    /// 插入实体。
    /// </summary>
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// 更新实体。
    /// </summary>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// 按主键删除实体。
    /// </summary>
    Task DeleteAsync(Guid id);
}

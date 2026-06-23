using System.Linq.Expressions;

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
    /// 按条件查询第一个匹配实体。
    /// </summary>
    Task<TEntity?> FirstAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 按条件查询实体列表。
    /// </summary>
    Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);

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

namespace IndustrySystem.Domain.Repositories;

public interface IRepository<TEntity>
{
    Task<TEntity?> GetAsync(Guid id);
    Task<List<TEntity>> GetListAsync();
    Task<TEntity> InsertAsync(TEntity entity);
    Task<TEntity> UpdateAsync(TEntity entity);
    Task DeleteAsync(Guid id);
}

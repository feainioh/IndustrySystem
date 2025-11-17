using IndustrySystem.Domain.Repositories;
using SqlSugar;

namespace IndustrySystem.Infrastructure.SqlSugar.Repositories;

public class SqlSugarRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
{
    private readonly ISqlSugarClient _db;
    public SqlSugarRepository(ISqlSugarClient db) => _db = db;

    public async Task<TEntity?> GetAsync(Guid id)
        => await _db.Queryable<TEntity>().InSingleAsync(id);

    public async Task<List<TEntity>> GetListAsync()
        => await _db.Queryable<TEntity>().ToListAsync();

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        await _db.Insertable(entity).ExecuteCommandAsync();
        return entity;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity)
    {
        await _db.Updateable(entity).ExecuteCommandAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
        => await _db.Deleteable<TEntity>().In(id).ExecuteCommandAsync();
}

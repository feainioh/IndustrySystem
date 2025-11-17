namespace IndustrySystem.Infrastructure.SqlSugar.Abstractions;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

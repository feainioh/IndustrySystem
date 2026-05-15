namespace IndustrySystem.Infrastructure.SqlSugar.Abstractions;

/// <summary>
/// 数据库初始化器契约。
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// 初始化数据库结构与种子数据。
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}

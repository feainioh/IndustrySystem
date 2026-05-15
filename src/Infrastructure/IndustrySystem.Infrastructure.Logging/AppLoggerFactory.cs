namespace IndustrySystem.Infrastructure.Logging;

/// <summary>
/// 应用日志工厂。
/// </summary>
public static class AppLoggerFactory
{
	public static IAppLogger Create<T>() => new NLogAppLogger(typeof(T));

	public static IAppLogger Create(Type categoryType) => new NLogAppLogger(categoryType);
}

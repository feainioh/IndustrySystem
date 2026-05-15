namespace IndustrySystem.Infrastructure.Logging;

/// <summary>
/// 应用日志抽象。
/// </summary>
public interface IAppLogger
{
	void Debug(string message);

	void Info(string message);

	void Warn(string message);

	void Error(string message, Exception? exception = null);
}

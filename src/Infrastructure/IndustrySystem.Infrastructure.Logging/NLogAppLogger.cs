using NLog;

namespace IndustrySystem.Infrastructure.Logging;

/// <summary>
/// 基于 NLog 的日志实现。
/// </summary>
public sealed class NLogAppLogger : IAppLogger
{
	private readonly Logger _logger;

	public NLogAppLogger(Type categoryType)
	{
		ArgumentNullException.ThrowIfNull(categoryType);
		_logger = LogManager.GetLogger(categoryType.FullName ?? categoryType.Name);
	}

	public void Debug(string message) => _logger.Debug(message);

	public void Info(string message) => _logger.Info(message);

	public void Warn(string message) => _logger.Warn(message);

	public void Error(string message, Exception? exception = null)
	{
		if (exception is null)
		{
			_logger.Error(message);
			return;
		}

		_logger.Error(exception, message);
	}
}

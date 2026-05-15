using NLog;
using NLog.Config;
using NLog.Targets;

namespace IndustrySystem.Infrastructure.Logging;

/// <summary>
/// NLog 基础配置入口。
/// </summary>
public static class LoggingConfiguration
{
	/// <summary>
	/// 初始化日志配置。
	/// </summary>
	/// <param name="configFilePath">可选的 NLog 配置文件路径。</param>
	public static void Configure(string? configFilePath = null)
	{
		if (!string.IsNullOrWhiteSpace(configFilePath) && File.Exists(configFilePath))
		{
			LogManager.Configuration = new XmlLoggingConfiguration(configFilePath);
			return;
		}

		var config = new NLog.Config.LoggingConfiguration();
		var consoleTarget = new ColoredConsoleTarget("console")
		{
			Layout = "${longdate}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}"
		};

		config.AddTarget(consoleTarget);
		config.AddRuleForAllLevels(consoleTarget);

		LogManager.Configuration = config;
	}
}

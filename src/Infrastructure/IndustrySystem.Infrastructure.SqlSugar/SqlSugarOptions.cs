namespace IndustrySystem.Infrastructure.SqlSugar;

/// <summary>
/// SqlSugar 基础设施配置。
/// </summary>
public class SqlSugarOptions
{
    /// <summary>
    /// 数据库连接字符串。
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 数据库初始化总开关。
    /// </summary>
    public bool InitOnStartup { get; set; } = true;

    /// <summary>
    /// Development 环境是否允许初始化。
    /// </summary>
    public bool InitOnStartupDevelopment { get; set; } = true;

    /// <summary>
    /// Production 环境是否允许初始化。
    /// </summary>
    public bool InitOnStartupProduction { get; set; } = false;

    /// <summary>
    /// 初始化时是否构建/更新表结构。
    /// </summary>
    public bool InitBuildSchema { get; set; } = true;

    /// <summary>
    /// 初始化时是否写入种子数据。
    /// </summary>
    public bool InitSeedData { get; set; } = true;

    /// <summary>
    /// 单次初始化超时秒数。
    /// </summary>
    public int InitTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 初始化失败重试次数。
    /// </summary>
    public int InitRetryCount { get; set; } = 3;

    /// <summary>
    /// 初始化重试间隔（毫秒）。
    /// </summary>
    public int InitRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// 初始化管理员用户名。
    /// </summary>
    public string AdminUserName { get; set; } = "admin";

    /// <summary>
    /// 初始化管理员显示名。
    /// </summary>
    public string AdminDisplayName { get; set; } = "管理员";

    /// <summary>
    /// 管理员初始密码环境变量名。
    /// </summary>
    public string AdminPasswordEnvVar { get; set; } = "ADMIN_INITIAL_PASSWORD";

    /// <summary>
    /// 管理员默认密码（仅在未提供环境变量时使用）。
    /// </summary>
    public string? AdminDefaultPassword { get; set; }

    /// <summary>
    /// 启动时是否重置管理员密码。
    /// </summary>
    public bool AdminResetPasswordOnStartup { get; set; } = false;
}

namespace IndustrySystem.Infrastructure.SqlSugar;

public class SqlSugarOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    // Global init switch
    public bool InitOnStartup { get; set; } = true;
    // Per-environment defaults (applied along with InitOnStartup)
    public bool InitOnStartupDevelopment { get; set; } = true;
    public bool InitOnStartupProduction { get; set; } = false;

    // Phase toggles
    public bool InitBuildSchema { get; set; } = true;
    public bool InitSeedData { get; set; } = true;

    public int InitTimeoutSeconds { get; set; } = 30;
    public int InitRetryCount { get; set; } = 3;
    public int InitRetryDelayMs { get; set; } = 1000;

    // Admin seed settings
    public string AdminUserName { get; set; } = "admin";
    public string AdminDisplayName { get; set; } = "管理员";
    public string AdminPasswordEnvVar { get; set; } = "ADMIN_INITIAL_PASSWORD";
    public string? AdminDefaultPassword { get; set; } = null; // Only used if env var not set
    public bool AdminResetPasswordOnStartup { get; set; } = false;
}

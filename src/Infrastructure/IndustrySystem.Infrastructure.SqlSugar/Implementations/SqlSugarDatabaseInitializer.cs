using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Entities.Permissions;
using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Entities.Users;
using IndustrySystem.Infrastructure.SqlSugar.Abstractions;
using NLog;
using SqlSugar;

namespace IndustrySystem.Infrastructure.SqlSugar.Implementations;

public class SqlSugarDatabaseInitializer : IDatabaseInitializer
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISqlSugarClient _db;
    private readonly SqlSugarOptions _options;

    public SqlSugarDatabaseInitializer(ISqlSugarClient db, SqlSugarOptions options)
    {
        _db = db; _options = options;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Resolve effective init switch per environment
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
        // Backward compatible defaults: if both env toggles are false (unset), treat as true
        var devToggle = _options.InitOnStartupDevelopment;
        var prodToggle = _options.InitOnStartupProduction;
        if (_options.InitOnStartup && !devToggle && !prodToggle)
        {
            devToggle = true;
            prodToggle = true;
        }
        var initEnabled = _options.InitOnStartup && (env.Equals("Development", StringComparison.OrdinalIgnoreCase) ? devToggle : prodToggle);
        Logger.Info($"[DB Init] Environment={env}, InitOnStartup={_options.InitOnStartup}, DevToggle={devToggle}, ProdToggle={prodToggle}, BuildSchema={_options.InitBuildSchema}, SeedData={_options.InitSeedData}");

        if (!initEnabled)
        {
            Logger.Info("[DB Init] Skipped (SqlSugar:InitOnStartup = false)");
            return;
        }

        var totalRetries = Math.Max(0, _options.InitRetryCount);
        var delayMs = Math.Max(0, _options.InitRetryDelayMs);
        var timeout = TimeSpan.FromSeconds(Math.Max(1, _options.InitTimeoutSeconds));

        for (int attempt = 1; attempt <= totalRetries + 1; attempt++)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);
            Logger.Info($"[DB Init] Attempt {attempt}/{totalRetries + 1} starting...");
            try
            {
                await Task.Run(() =>
                {
                    if (_options.InitBuildSchema)
                    {
                        _db.CodeFirst.InitTables(
                            typeof(Role), typeof(Permission), typeof(ExperimentTemplate), typeof(ExperimentGroup), typeof(Experiment), typeof(User)
                        );
                    }

                    if (_options.InitSeedData)
                    {
                        SeedIdempotent();
                    }
                }, cts.Token);

                Logger.Info("[DB Init] Initialization completed successfully.");
                return;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"[DB Init] Attempt {attempt} failed.");
                if (attempt <= totalRetries)
                {
                    if (delayMs > 0) Thread.Sleep(delayMs);
                    continue;
                }
                throw;
            }
        }
    }

    private void SeedIdempotent()
    {
        // Roles (Upsert by Name)
        var roleRepo = _db.GetSimpleClient<Role>();
        UpsertRole(roleRepo, new Role { Name = "Admin", Description = "Administrator", IsDefault = true });

        // Permissions (Upsert by Name)
        var permRepo = _db.GetSimpleClient<Permission>();
        UpsertPermission(permRepo, new Permission{ Name = "Users.View", DisplayName = "查看用户" });
        UpsertPermission(permRepo, new Permission{ Name = "Users.Edit", DisplayName = "编辑用户" });
        UpsertPermission(permRepo, new Permission{ Name = "Roles.View", DisplayName = "查看角色" });
        UpsertPermission(permRepo, new Permission{ Name = "Roles.Edit", DisplayName = "编辑角色" });
        UpsertPermission(permRepo, new Permission{ Name = "Templates.Edit", DisplayName = "编辑模板" });

        // Admin user (Upsert by UserName)
        var userRepo = _db.GetSimpleClient<User>();
        UpsertAdminUser(userRepo);
    }

    private static void UpsertRole(SimpleClient<Role> repo, Role input)
    {
        var existing = repo.GetSingle(r => r.Name == input.Name);
        if (existing == null)
        {
            repo.Insert(input);
        }
        else
        {
            existing.Description = input.Description;
            existing.IsDefault = input.IsDefault;
            repo.Update(existing);
        }
    }

    private static void UpsertPermission(SimpleClient<Permission> repo, Permission input)
    {
        var existing = repo.GetSingle(p => p.Name == input.Name);
        if (existing == null)
        {
            repo.Insert(input);
        }
        else
        {
            existing.DisplayName = input.DisplayName;
            repo.Update(existing);
        }
    }

    private void UpsertAdminUser(SimpleClient<User> repo)
    {
        var userName = string.IsNullOrWhiteSpace(_options.AdminUserName) ? "admin" : _options.AdminUserName;
        var displayName = string.IsNullOrWhiteSpace(_options.AdminDisplayName) ? "管理员" : _options.AdminDisplayName;

        var existing = repo.GetSingle(u => u.UserName == userName);
        if (existing == null)
        {
            var pwd = ResolveAdminPassword();
            repo.Insert(new User
            {
                UserName = userName,
                DisplayName = displayName,
                PasswordHash = HashPassword(pwd),
                IsActive = true
            });
        }
        else
        {
            existing.DisplayName = displayName;
            if (_options.AdminResetPasswordOnStartup)
            {
                var pwd = ResolveAdminPassword();
                existing.PasswordHash = HashPassword(pwd);
            }
            repo.Update(existing);
        }
    }

    private string ResolveAdminPassword()
    {
        var envVar = string.IsNullOrWhiteSpace(_options.AdminPasswordEnvVar) ? null : Environment.GetEnvironmentVariable(_options.AdminPasswordEnvVar);
        if (!string.IsNullOrEmpty(envVar)) return envVar;
        if (!string.IsNullOrWhiteSpace(_options.AdminDefaultPassword)) return _options.AdminDefaultPassword!;
        // fallback to a strong random password if neither provided (logged once)
        var pwd = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        Logger.Warn("[DB Init] Admin password not provided. A random password has been generated. Please reset immediately.");
        return pwd;
    }

    private static string HashPassword(string plain)
    {
        if (string.IsNullOrEmpty(plain)) return string.Empty;
        // Simple SHA256 hash as placeholder; replace with a stronger hash (e.g., BCrypt/Argon2) in production
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(plain);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}

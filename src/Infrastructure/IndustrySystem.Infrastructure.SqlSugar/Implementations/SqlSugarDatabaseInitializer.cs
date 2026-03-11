using IndustrySystem.Domain.Entities.Experiments;
using IndustrySystem.Domain.Entities.Inventory;
using IndustrySystem.Domain.Entities.Materials;
using IndustrySystem.Domain.Entities.Permissions;
using IndustrySystem.Domain.Entities.Roles;
using IndustrySystem.Domain.Entities.Shelves;
using IndustrySystem.Domain.Entities.Users;
using IndustrySystem.Domain.Shared.Enums.MaterialEnums;
using IndustrySystem.Domain.Shared.Enums.ShelfEnums;
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
                            typeof(Role), typeof(Permission), typeof(ExperimentTemplate), typeof(ExperimentGroup), typeof(Experiment), typeof(User), typeof(Material), typeof(InventoryRecord),
                            typeof(ContainerInfo), typeof(ShelfConfig), typeof(ShelfSlot)
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
        UpsertPermission(permRepo, new Permission{ Name = "Users.View", DisplayName = "查看用户", GroupName = "用户" });
        UpsertPermission(permRepo, new Permission{ Name = "Users.Edit", DisplayName = "编辑用户", GroupName = "用户" });
        UpsertPermission(permRepo, new Permission{ Name = "Roles.View", DisplayName = "查看角色", GroupName = "角色" });
        UpsertPermission(permRepo, new Permission{ Name = "Roles.Edit", DisplayName = "编辑角色", GroupName = "角色" });
        UpsertPermission(permRepo, new Permission{ Name = "Templates.Edit", DisplayName = "编辑模板", GroupName = "模板" });

        // Admin user (Upsert by UserName)
        var userRepo = _db.GetSimpleClient<User>();
        UpsertAdminUser(userRepo);

        // Materials (Upsert by MaterialCode)
        var materialRepo = _db.GetSimpleClient<Material>();
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0001", Name = "乙醇", FullName = "无水乙醇", MolecularFormula = "C2H6O", Category = MaterialCategory.Liquid, MaterialType = MaterialType.RawMaterial, CasNo = "64-17-5", Purity = "AR", Density = "0.789", Unit = "L", HazardLevel = MaterialHazardLevel.High, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "远离火源", Brand = "国药", Supplier = "本地供应商A" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0002", Name = "氯化钠", FullName = "分析纯氯化钠", MolecularFormula = "NaCl", Category = MaterialCategory.Solid, MaterialType = MaterialType.Consumable, CasNo = "7647-14-5", Purity = "AR", Density = "2.165", Unit = "kg", HazardLevel = MaterialHazardLevel.None, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "防潮", Brand = "阿拉丁", Supplier = "本地供应商B" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0003", Name = "丙酮", FullName = "分析纯丙酮", MolecularFormula = "C3H6O", Category = MaterialCategory.Liquid, MaterialType = MaterialType.RawMaterial, CasNo = "67-64-1", Purity = "AR", Density = "0.791", Unit = "L", HazardLevel = MaterialHazardLevel.High, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "远离火源，避免吸入", Brand = "国药", Supplier = "本地供应商A" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0004", Name = "硫酸", FullName = "浓硫酸", MolecularFormula = "H2SO4", Category = MaterialCategory.Liquid, MaterialType = MaterialType.RawMaterial, CasNo = "7664-93-9", Purity = "GR", Density = "1.84", Unit = "L", HazardLevel = MaterialHazardLevel.High, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "强腐蚀性，穿戴防护装备", Brand = "沪试", Supplier = "本地供应商C" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0005", Name = "氮气", FullName = "高纯氮气", MolecularFormula = "N2", Category = MaterialCategory.Gas, MaterialType = MaterialType.Consumable, CasNo = "7727-37-9", Purity = "99.999%", Density = "1.251", Unit = "m³", HazardLevel = MaterialHazardLevel.Low, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "高压钢瓶，防止窒息", Brand = "林德", Supplier = "本地供应商D" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0006", Name = "双氧水", FullName = "双氧水", MolecularFormula = "H2O2", Category = MaterialCategory.Liquid, MaterialType = MaterialType.Intermediate, CasNo = "7722-84-1", Purity = "30%", Density = "1.11", Unit = "L", HazardLevel = MaterialHazardLevel.Medium, StorageCondition = MaterialStorageCondition.Refrigerated, Precautions = "避光保存，远离可燃物", Brand = "国药", Supplier = "本地供应商A" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0007", Name = "维生素C", FullName = "L-抗坏血酸", MolecularFormula = "C6H8O6", Category = MaterialCategory.Solid, MaterialType = MaterialType.Product, CasNo = "50-81-7", Purity = "99%", Density = "1.65", Unit = "kg", HazardLevel = MaterialHazardLevel.None, StorageCondition = MaterialStorageCondition.LightProtected, Precautions = "避光密封保存", Brand = "阿拉丁", Supplier = "本地供应商B" });
        UpsertMaterial(materialRepo, new Material { MaterialCode = "MAT-0008", Name = "活性炭", FullName = "粒状活性炭", MolecularFormula = "C", Category = MaterialCategory.Solid, MaterialType = MaterialType.Consumable, CasNo = "7440-44-0", Purity = "工业级", Density = "0.45", Unit = "kg", HazardLevel = MaterialHazardLevel.None, StorageCondition = MaterialStorageCondition.RoomTemperature, Precautions = "防潮密封", Brand = "沪试", Supplier = "本地供应商C" });

        // Inventory Records (Upsert by BatchNo)
        var invRepo = _db.GetSimpleClient<InventoryRecord>();
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0001", MaterialName = "乙醇", BatchNo = "B20250101-01", Quantity = 50, SafetyStock = 10, Unit = "L", InboundDate = new DateTime(2025, 1, 1), ExpiryDate = new DateTime(2026, 1, 1), Location = "A-01-01", Remark = "无水乙醇" });
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0002", MaterialName = "氯化钠", BatchNo = "B20250115-01", Quantity = 200, SafetyStock = 50, Unit = "kg", InboundDate = new DateTime(2025, 1, 15), ExpiryDate = new DateTime(2027, 1, 15), Location = "A-02-01", Remark = "分析纯" });
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0003", MaterialName = "丙酮", BatchNo = "B20250201-01", Quantity = 30, SafetyStock = 5, Unit = "L", InboundDate = new DateTime(2025, 2, 1), ExpiryDate = new DateTime(2026, 2, 1), Location = "A-01-02", Remark = "" });
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0004", MaterialName = "硫酸", BatchNo = "B20250210-01", Quantity = 20, SafetyStock = 5, Unit = "L", InboundDate = new DateTime(2025, 2, 10), ExpiryDate = new DateTime(2026, 6, 10), Location = "B-01-01", Remark = "浓硫酸，强腐蚀性" });
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0006", MaterialName = "双氧水", BatchNo = "B20250301-01", Quantity = 8, SafetyStock = 10, Unit = "L", InboundDate = new DateTime(2025, 3, 1), ExpiryDate = new DateTime(2025, 9, 1), Location = "C-01-01", Remark = "库存不足，需补货" });
        UpsertInventory(invRepo, new InventoryRecord { MaterialCode = "MAT-0007", MaterialName = "维生素C", BatchNo = "B20250315-01", Quantity = 100, SafetyStock = 20, Unit = "kg", InboundDate = new DateTime(2025, 3, 15), ExpiryDate = new DateTime(2026, 3, 15), Location = "A-03-01", Remark = "成品区" });

        // Containers (Upsert by Name)
        var containerRepo = _db.GetSimpleClient<ContainerInfo>();
        UpsertContainer(containerRepo, new ContainerInfo { Name = "50mL离心瓶", ContainerType = ContainerType.CentrifugeBottle, Rows = 1, Columns = 1, Description = "50mL标准离心瓶" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "2mL取样瓶", ContainerType = ContainerType.SamplingBottle, Rows = 1, Columns = 1, Description = "2mL色谱取样瓶" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "500mL原液瓶", ContainerType = ContainerType.StockBottle, Rows = 1, Columns = 1, Description = "500mL原液储存瓶" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "96孔试剂盒", ContainerType = ContainerType.ReagentKit, Rows = 8, Columns = 12, Description = "96孔标准微孔板" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "24孔试剂盒", ContainerType = ContainerType.ReagentKit, Rows = 4, Columns = 6, Description = "24孔深孔板" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "粉筒-100g", ContainerType = ContainerType.PowderCylinder, Rows = 1, Columns = 1, Description = "100g粉末储存筒" });
        UpsertContainer(containerRepo, new ContainerInfo { Name = "移液器吸头盒", ContainerType = ContainerType.Consumable, Rows = 8, Columns = 12, Description = "96位移液器吸头盒" });

        // Shelves (Upsert by ShelfCode)
        var shelfRepo = _db.GetSimpleClient<ShelfConfig>();
        UpsertShelf(shelfRepo, _db.GetSimpleClient<ShelfSlot>(), new ShelfConfig { ShelfCode = "SHELF-A", Name = "A号货架", Rows = 4, Columns = 6, Description = "主存储区" });
        UpsertShelf(shelfRepo, _db.GetSimpleClient<ShelfSlot>(), new ShelfConfig { ShelfCode = "SHELF-B", Name = "B号货架", Rows = 3, Columns = 4, Description = "试剂区" });
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
            existing.GroupName = input.GroupName;
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

    private static void UpsertMaterial(SimpleClient<Material> repo, Material input)
    {
        var existing = repo.GetSingle(m => m.MaterialCode == input.MaterialCode);
        if (existing == null)
        {
            repo.Insert(input);
        }
        else
        {
            existing.Name = input.Name;
            existing.FullName = input.FullName;
            existing.MolecularFormula = input.MolecularFormula;
            existing.Category = input.Category;
            existing.MaterialType = input.MaterialType;
            existing.CasNo = input.CasNo;
            existing.Purity = input.Purity;
            existing.Density = input.Density;
            existing.Unit = input.Unit;
            existing.HazardLevel = input.HazardLevel;
            existing.StorageCondition = input.StorageCondition;
            existing.Precautions = input.Precautions;
            existing.Brand = input.Brand;
            existing.Supplier = input.Supplier;
            existing.UpdatedAt = DateTime.UtcNow;
            repo.Update(existing);
        }
    }
    private static void UpsertInventory(SimpleClient<InventoryRecord> repo, InventoryRecord input)
    {
        var existing = repo.GetSingle(i => i.BatchNo == input.BatchNo);
        if (existing == null)
        {
            repo.Insert(input);
        }
        else
        {
            existing.MaterialCode = input.MaterialCode;
            existing.MaterialName = input.MaterialName;
            existing.Quantity = input.Quantity;
            existing.SafetyStock = input.SafetyStock;
            existing.Unit = input.Unit;
            existing.InboundDate = input.InboundDate;
            existing.ExpiryDate = input.ExpiryDate;
            existing.Location = input.Location;
            existing.Remark = input.Remark;
            existing.UpdatedAt = DateTime.UtcNow;
            repo.Update(existing);
        }
    }

    private static void UpsertContainer(SimpleClient<ContainerInfo> repo, ContainerInfo input)
    {
        var existing = repo.GetSingle(c => c.Name == input.Name);
        if (existing == null)
        {
            repo.Insert(input);
        }
        else
        {
            existing.ContainerType = input.ContainerType;
            existing.Rows = input.Rows;
            existing.Columns = input.Columns;
            existing.Description = input.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            repo.Update(existing);
        }
    }

    private static void UpsertShelf(SimpleClient<ShelfConfig> repo, SimpleClient<ShelfSlot> slotRepo, ShelfConfig input)
    {
        var existing = repo.GetSingle(s => s.ShelfCode == input.ShelfCode);
        if (existing == null)
        {
            repo.Insert(input);
            // 生成槽位
            for (int r = 1; r <= input.Rows; r++)
            for (int c = 1; c <= input.Columns; c++)
                slotRepo.Insert(new ShelfSlot { ShelfId = input.Id, Row = r, Column = c });
        }
        else
        {
            existing.Name = input.Name;
            existing.Rows = input.Rows;
            existing.Columns = input.Columns;
            existing.Description = input.Description;
            existing.UpdatedAt = DateTime.UtcNow;
            repo.Update(existing);
        }
    }
}


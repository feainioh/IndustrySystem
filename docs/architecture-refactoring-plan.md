# 架构改造计划

> 目标：从 10 项目 DDD 分层 → 5~6 项目扁平化架构，消除贫血模型和过度抽象

---

## 零、目标架构

```
┌─────────────────────────────────────────────────────┐
│  IndustrySystem.Presentation.Wpf (不变)               │
│  Views / ViewModels / Converters / Resources         │
│  DI: Prism 9 + DryIoc                                │
├─────────────────────────────────────────────────────┤
│  IndustrySystem.AppServices (新·合并 Application)      │
│  ├─ Services/    聚合业务服务（~8个）                   │
│  ├─ Dtos/        DTO & ViewModel 数据载体              │
│  └─ MappingProfile.cs                                │
├─────────────────────────────────────────────────────┤
│  IndustrySystem.Domain (精简)                         │
│  ├─ Entities/    纯数据实体 + 枚举                      │
│  └─ Enums/                                            │
├─────────────────────────────────────────────────────┤
│  IndustrySystem.Data (新·合并 Infrastructure)          │
│  ├─ SqlSugar/    数据库配置 + 种子数据 + 仓储            │
│  ├─ Communication/  Modbus/HTTP/CAN 客户端              │
│  └─ Logging/     NLog 封装                             │
├─────────────────────────────────────────────────────┤
│  IndustrySystem.MotionDesigner (不变)                  │
└─────────────────────────────────────────────────────┘

移除:  Application.Contracts, Domain.Shared, 各层 AbpModules
精简:  IRolePermissionRepository / IUserRoleRepository → ISqlSugarClient 直接注入
```

---

## 第一阶段：止血优化（1~2天，低风险）

> 不改架构，只修复当前最痛的问题

### 1.1 给 IRepository<T> 增加条件查询

**问题**: 所有服务调用 `GetListAsync()` → 全表加载 → 内存过滤

**操作**:
- `IRepository<T>` 新增 `Task<T?> FirstAsync(Expression<Func<T, bool>> predicate)`
- `IRepository<T>` 新增 `Task<List<T>> GetListAsync(Expression<Func<T, bool>> predicate)`
- `SqlSugarRepository<T>` 实现上述方法，直接转发到 `_db.Queryable<T>()`

**影响文件**: `IRepository<T>.cs`, `SqlSugarRepository<T>.cs`

**使用处替换示例**:
```csharp
// 改前: 全表扫描
var users = await _repo.GetListAsync();
var user = users.FirstOrDefault(u => u.UserName == userName);

// 改后: 条件查询
var user = await _repo.FirstAsync(u => u.UserName == userName);
```

### 1.2 移除空占位 ViewModel

**操作**:
- 删除 `DeviceParamsViewModel.cs`（3 行: `class DeviceParamsViewModel : NavigationViewModel { }`）
- 删除 `PeripheralDebugViewModel.cs`（3 行）
- `App.xaml.cs` 中对应的 `RegisterForNavigation` 改用 `NavigationViewModel` 替代（或保留注册但指向基类）

### 1.3 `_ = LoadAsync()` 改安全调用

**问题**: fire-and-forget 吞异常，数据加载失败无感知

**操作**:
- 在所有 ViewModel 构造函数中将 `_ = LoadAsync()` 改为:
```csharp
Task.Run(async () =>
{
    try { await LoadAsync(); }
    catch (Exception ex) { _logger.Error(ex, "Initial data load failed"); }
});
```
- 影响：`InventoryViewModel`, `MaterialInfoViewModel`, `UsersViewModel`, `PermissionsViewModel`, `RoleManageViewModel` 等

### 1.4 补全 ViewModel 中 `new` 遮蔽残留

- `UsersViewModel` — `new ICommand RefreshCommand` + 独立 `Users` 集合
- `PermissionsViewModel` — 同上模式
- `RoleManageViewModel` — 同上模式

改为与 `InventoryViewModel` 一致的统一模式（使用基类 `_all` + `Items`）。

### 第一阶段验收标准
- [ ] 登录校验走 WHERE 条件查询（非全表扫描）
- [ ] 所有空 ViewModel 已移除
- [ ] 日志中不存在未捕获的数据加载异常
- [ ] 编译 0 错误

---

## 第二阶段：消融合并（3~5天，中风险）

> 合并薄透传服务为聚合服务，消除 1:1 接口/实现

### 2.1 合并 Application.Contracts → Application

直接删除 `IndustrySystem.Application.Contracts.csproj`，将 DTO 和接口定义迁入 `IndustrySystem.Application`。

### 2.2 聚合服务设计

**现状**: 20 个服务接口 + 20 个实现，大量薄透传

**目标**: 7~8 个聚合服务

| 原服务（20个） | 合并后（~8个） | 说明 |
|---------------|---------------|------|
| `MaterialAppService` | **`MaterialService`** | 物料 CRUD + 库存 + 货架 + 容器 |
| `InventoryAppService` | ↑ 合并到上 | |
| `ShelfAppService` | ↑ 合并到上 | |
| `ExperimentAppService` | **`ExperimentService`** | 实验 CRUD |
| `ExperimentGroupAppService` | ↑ 合并到上 | |
| `ExperimentTemplateAppService` | ↑ 合并到上 | |
| `ExperimentParameterAppService` | ↑ 合并到上 | |
| `ExperimentHistoryAppService` | ↑ 合并到上 | |
| `ExperimentExecutionService` | **`ExperimentExecutionService`** | 执行引擎（独立，有真正业务逻辑） |
| `MockExperimentExecutionService` | ↑ 合并到上 | |
| `MotionProgramAppService` | **`MotionProgramService`** | 运动程序 |
| `MotionProgramExecutor` | ↑ 合并到上 | |
| `UserAppService` | **`UserService`** | 用户 + 角色 + 权限 + 认证 |
| `RoleAppService` | ↑ 合并到上 | |
| `PermissionAppService` | ↑ 合并到上 | |
| `AlarmAppService` | **`AlarmService`** | 告警 |
| `OperationLogService` | **`AuditService`** | 操作日志 |
| `CommunicationAppService` | **`CommunicationService`** | 通信管理 |
| `ExternalDataSyncAppService` | **`ExternalSyncService`** | 外部同步（保留接口，按需启用） |

### 2.3 服务接口去重

合并后的聚合服务不再需要独立接口文件。每个聚合服务一个 `.cs` 文件，包含接口定义 + 实现:

```csharp
// MaterialService.cs
public interface IMaterialService { ... }
public class MaterialService : IMaterialService { ... }
```

DI 注册:
```csharp
containerRegistry.Register<IMaterialService, MaterialService>();
// 从 6 行 → 1 行
```

### 2.4 去掉 IRepository<T> → 直接注入 ISqlSugarClient

所有服务层直接注入 `ISqlSugarClient`:

```csharp
// 改前:
IRepository<Material> _repo;
var list = await _repo.GetListAsync();

// 改后:
ISqlSugarClient _db;
var list = await _db.Queryable<Material>().ToListAsync();
```

删除 `SqlSugarRepository<T>`（4 个文件缩减到 0）。

保留 `IRolePermissionRepository` 和 `IUserRoleRepository`（有非标查询），但合并为 `IRolePermissionRepository` 和 `IUserRoleRepository` → 直接在服务中用 `_db.Queryable<RolePermission>()`。

### 第二阶段验收标准
- [ ] 项目数从 10 → 7
- [ ] 服务文件从 40（20接口+20实现） → ~16（8接口+8实现）
- [ ] `App.xaml.cs` DI 注册从 104 行 → ~40 行
- [ ] 编译 0 错误，功能回归通过

---

## 第三阶段：移除 Abp 依赖（2~3天，中风险）

### 3.1 现状

Abp 的使用仅限于:
- `AbpModule` 类（6 个，仅做逻辑分组）
- `AbpApplicationFactory.Create<T>()` 在 `App.xaml.cs` 中初始化
- `Volo.Abp.AutoMapper` 集成

### 3.2 操作

**步骤 1**: 移除 Abp AutoMapper → 直接用 AutoMapper 原生 DI
```csharp
// App.xaml.cs
var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
containerRegistry.RegisterInstance<IMapper>(mapperConfig.CreateMapper());
```

**步骤 2**: 删除 6 个 `*Module.cs` 文件

**步骤 3**: 合并模块中的 `ConfigureServices` 到 `App.xaml.cs` 的 Prism `RegisterTypes`

**步骤 4**: 移除 NuGet 引用 `Volo.Abp.Core`, `Volo.Abp.AutoMapper`, `Volo.Abp.Modularity`

### 3.3 收益
- 移除 6 个依赖包
- 消除双 DI 容器隐患
- `App.xaml.cs` 逻辑集中化

### 第三阶段验收标准
- [ ] `dotnet build` 无 Abp 相关依赖警告
- [ ] AutoMapper 正常工作
- [ ] DI 仅使用 DryIoc

---

## 第四阶段：ExternalSync 按需化（1~2天，低风险）

### 4.1 现状

23 个文件、8 个接口、多种传输协议实现，但 `Enabled: false`。

### 4.2 操作

**保留**:
- 接口定义（`IExternalSyncChannel`, `IExternalSyncChannelFactory` 等）
- `ModbusTcpClient`（生产环境在用）

**移除或标记**:
- `CanClient` / `EthercatClient`（从未实现的桩代码）
- `WebApiExternalSyncChannel` / `SocketExternalSyncChannel` / `SignalRExternalSyncChannel` → 移到 `Samples/` 或独立 NuGet 包
- 减少 `appsettings.json` 中的配置段

**改为条件编译**或插件式加载（可选，更彻底）。

### 第四阶段验收标准
- [ ] `Communication` 项目文件数减少 50%
- [ ] Modbus 通信仍正常工作
- [ ] 按需重新引入 ExternalSync 通道的实现路径清晰

---

## 最终状态对比

| 维度 | 改造前 | 改造后 |
|------|--------|--------|
| 项目数 | 10 | 5~6 |
| 接口文件 | 60 | ~10 |
| 服务文件 | 40 | ~16 |
| DI 注册行 | 104 | ~30 |
| 仓储实现 | 4 | 0 |
| Abp 模块 | 6 | 0 |
| 空 ViewModel | 2 | 0 |
| 编译时间 | 基准 | -30% |
| 新人上手天数 | ~5 | ~2 |

---

## 执行顺序建议

```
Week 1: 第一阶段 (止血)         ← 立即见效，零风险
Week 2: 第二阶段·服务合并 (前半)  ← 最大的收益
Week 3: 第二阶段·去仓储 (后半)
Week 4: 第三阶段 (去Abp)       ← 需充分测试
Week 5: 第四阶段 (ExternalSync)  ← 锦上添花
```

每个阶段完成后独立提交 + 打 tag，确保任何时刻可回滚。

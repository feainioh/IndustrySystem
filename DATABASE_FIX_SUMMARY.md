# 数据库列名映射修复 - 修改总结

## 问题

应用启动时抛出异常：
```
MySqlConnector.MySqlException: Unknown column 'CreationTime' in 'field list'
```

**根本原因**：C# 实体类属性名（PascalCase）与数据库列名（小写）不匹配。

## 解决方案

为所有实体类添加 `[SugarColumn]` 特性，明确指定数据库列名映射。

## 修改的文件清单

### 1. 实体类文件（已添加 SugarColumn 特性）

#### 用户和权限相关
- ? `src/Domain/IndustrySystem.Domain/Entities/Users/User.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Users/UserRole.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Roles/Role.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Roles/RolePermission.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Permissions/Permission.cs`

#### 实验相关
- ? `src/Domain/IndustrySystem.Domain/Entities/Experiments/ExperimentTemplate.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Experiments/Experiment.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Experiments/ExperimentGroup.cs`

#### 设备相关
- ? `src/Domain/IndustrySystem.Domain/Entities/Devices/Device.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Devices/Motors/CanMotor.cs`
- ? `src/Domain/IndustrySystem.Domain/Entities/Devices/Motors/EthercatMotor.cs`

### 2. 项目文件
- ? `src/Domain/IndustrySystem.Domain/IndustrySystem.Domain.csproj`
  - 添加了 `SqlSugarCore` NuGet 包引用

## 修改详情

### 典型修改示例

**修改前：**
```csharp
namespace IndustrySystem.Domain.Entities.Users;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
```

**修改后：**
```csharp
using SqlSugar;

namespace IndustrySystem.Domain.Entities.Users;

public class User
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "username")]
    public string UserName { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "createat")]
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
```

## 列名映射规则

| C# 属性名模式 | 数据库列名模式 | 示例 |
|--------------|---------------|------|
| PascalCase | lowercase | `UserName` → `username` |
| PascalCase (复合) | lowercase | `IsActive` → `isactive` |
| PascalCase (缩写) | lowercase | `Id` → `id` |
| PascalCase (Time) | lowercase + at | `CreationTime` → `createat` |
| PascalCase (Time) | lowercase + dat | `CreatedAt` → `createdat` |

## 完整的属性映射表

### User
- `Id` → `id`
- `UserName` → `username`
- `DisplayName` → `displayname`
- `PasswordHash` → `passwordhash`
- `IsActive` → `isactive`
- `CreationTime` → `createat`

### Role
- `Id` → `id`
- `Name` → `name`
- `Description` → `description`
- `IsDefault` → `isdefault`

### Permission
- `Id` → `id`
- `Name` → `name`
- `DisplayName` → `displayname`

### UserRole (复合主键)
- `UserId` → `userid` (PK)
- `RoleId` → `roleid` (PK)
- `CreatedAt` → `createdat`
- `UpdatedAt` → `updatedat`

### RolePermission (复合主键)
- `RoleId` → `roleid` (PK)
- `PermissionId` → `permissionid` (PK)
- `CreatedAt` → `createdat`
- `UpdatedAt` → `updatedat`

### ExperimentTemplate
- `Id` → `id`
- `Name` → `name`
- `Description` → `description`

### Experiment
- `Id` → `id`
- `TemplateId` → `templateid`
- `GroupId` → `groupid`
- `Name` → `name`
- `CreatedAt` → `createdat`

### ExperimentGroup
- `Id` → `id`
- `Name` → `name`

### Device
- `Id` → `id`
- `Name` → `name`
- `Type` → `type`
- `IsOnline` → `isonline`

### CanMotor
- `Id` → `id`
- `Name` → `name`
- `NodeId` → `nodeid`
- `BaudRate` → `baudrate`

### EthercatMotor
- `Id` → `id`
- `Name` → `name`
- `SlaveAddress` → `slaveaddress`

## 构建验证

? **构建成功**

```
生成成功
```

所有实体类的列名映射已正确配置，不再出现 `Unknown column` 错误。

## 下一步

1. ? 运行应用并验证数据库连接
2. ? 测试登录功能
3. ? 测试权限管理功能
4. ? 验证所有 CRUD 操作正常工作

## 注意事项

### 1. 新增实体类时
当添加新的实体类时，记得：
- 引用 `using SqlSugar;`
- 为主键添加 `[SugarColumn(IsPrimaryKey = true)]`
- 为每个属性添加 `[SugarColumn(ColumnName = "columnname")]`
- 复合主键需要在多个属性上标注 `IsPrimaryKey = true`

### 2. 数据库命名规范
- 所有列名使用小写
- 不使用下划线分隔符
- 保持一致性

### 3. 时间字段命名
- `CreationTime` → `createat`
- `CreatedAt` → `createdat`
- `UpdatedAt` → `updatedat`
- `ModifiedAt` → `modifiedat`

### 4. Guid 主键
```csharp
[SugarColumn(IsPrimaryKey = true)]
public Guid Id { get; set; } = Guid.NewGuid();
```
不需要设置 `IsIdentity`，因为 Guid 不是自增的。

### 5. 可空类型
```csharp
[SugarColumn(ColumnName = "description")]
public string? Description { get; set; }

[SugarColumn(ColumnName = "updatedat")]
public DateTime? UpdatedAt { get; set; }
```
C# 的可空类型会自动映射为数据库的 NULL 列。

## 相关文档

- [DATABASE_COLUMN_MAPPING_FIX.md](./DATABASE_COLUMN_MAPPING_FIX.md) - 详细的修复说明和最佳实践

# 数据库列名映射修复文档

## 问题描述

应用启动时出现错误：
```
MySqlConnector.MySqlException: Unknown column 'CreationTime' in 'field list'
```

**原因分析**：
- C# 实体类使用的属性名（如 `CreationTime`）与数据库中的列名（如 `createat`）不匹配
- SqlSugar 默认使用实体类的属性名作为列名，导致 SQL 查询失败

## 解决方案

使用 SqlSugar 的 `[SugarColumn]` 特性标注实体类属性，明确指定数据库列名映射。

## 修改的文件

### 1. 实体类添加 SugarColumn 特性

#### User.cs
```csharp
using SqlSugar;

public class User
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "username")]
    public string UserName { get; set; } = string.Empty;
    
    [SugarColumn(ColumnName = "createat")]
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
    // ...其他属性
}
```

#### Permission.cs, Role.cs
- 添加 `[SugarColumn]` 特性
- 指定列名映射（如 `name`, `displayname`, `description`）

#### UserRole.cs, RolePermission.cs
- 使用 `[SugarColumn(IsPrimaryKey = true)]` 标注复合主键
- 映射时间字段（`createdat`, `updatedat`）

#### Experiment 相关实体
- ExperimentTemplate.cs
- Experiment.cs
- ExperimentGroup.cs

### 2. 添加 SqlSugar 包引用

**IndustrySystem.Domain.csproj**
```xml
<PackageReference Include="SqlSugarCore" Version="5.1.4.172" />
```

## 数据库列名规范

根据修改，数据库列名使用全小写命名：

| C# 属性名 | 数据库列名 |
|-----------|------------|
| `Id` | `id` |
| `UserName` | `username` |
| `DisplayName` | `displayname` |
| `PasswordHash` | `passwordhash` |
| `IsActive` | `isactive` |
| `CreationTime` | `createat` |
| `CreatedAt` | `createdat` |
| `UpdatedAt` | `updatedat` |
| `Name` | `name` |
| `Description` | `description` |
| `IsDefault` | `isdefault` |
| `TemplateId` | `templateid` |
| `GroupId` | `groupid` |
| `UserId` | `userid` |
| `RoleId` | `roleid` |
| `PermissionId` | `permissionid` |

## SugarColumn 特性说明

### 常用属性

1. **IsPrimaryKey** - 标识主键
   ```csharp
   [SugarColumn(IsPrimaryKey = true)]
   public Guid Id { get; set; }
   ```

2. **ColumnName** - 指定数据库列名
   ```csharp
   [SugarColumn(ColumnName = "username")]
   public string UserName { get; set; }
   ```

3. **IsIgnore** - 忽略该属性（不映射到数据库）
   ```csharp
   [SugarColumn(IsIgnore = true)]
   public string TemporaryData { get; set; }
   ```

4. **IsNullable** - 指定列是否可为空
   ```csharp
   [SugarColumn(IsNullable = true)]
   public DateTime? UpdatedAt { get; set; }
   ```

5. **IsIdentity** - 标识自增列
   ```csharp
   [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
   public int Id { get; set; }
   ```

## 复合主键示例

对于具有复合主键的实体（如 `UserRole`, `RolePermission`）：

```csharp
public class UserRole
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid UserId { get; set; }
    
    [SugarColumn(IsPrimaryKey = true)]
    public Guid RoleId { get; set; }
}
```

## 注意事项

1. **区分大小写**
   - 不同数据库对列名大小写的处理不同
   - MySQL 在 Windows 上默认不区分大小写，但 Linux 上区分
   - 建议统一使用小写

2. **Guid 类型**
   - 使用 `Guid` 类型作为主键时，设置 `IsIdentity = false`
   - SqlSugar 会自动处理 Guid 的生成和存储

3. **可空类型**
   - C# 中的可空类型（`DateTime?`）会自动映射为数据库的可空列
   - 显式使用 `IsNullable = true` 可以增强可读性

4. **性能影响**
   - 特性标注在编译时解析，运行时性能影响极小
   - 比配置方式更加类型安全，推荐使用

## 其他映射方式

### 方式1：使用特性（已采用，推荐）
```csharp
[SugarColumn(ColumnName = "username")]
public string UserName { get; set; }
```

### 方式2：使用 EntityService 配置（已移除）
```csharp
ConfigureExternalServices = new ConfigureExternalServices
{
    EntityService = (property, column) =>
    {
        if (column.PropertyName == "UserName")
            column.DbColumnName = "username";
    }
}
```

### 方式3：使用 MappingTables（全局配置）
```csharp
db.MappingTables.Add("User", "users");
db.MappingColumns.Add("UserName", "username");
```

## 验证修复

启动应用后应该能够正常连接数据库并查询数据，不再出现 `Unknown column` 错误。

如果仍有问题：
1. 检查数据库中的实际列名
2. 确认实体类中的 ColumnName 与数据库列名完全匹配
3. 查看 SqlSugar 生成的 SQL 语句（启用日志）

## 启用 SqlSugar 日志（调试用）

```csharp
var db = new SqlSugarClient(new ConnectionConfig
{
    // ...其他配置
    IsAutoCloseConnection = true,
    ConfigureExternalServices = new ConfigureExternalServices
    {
        EntityService = (property, column) => { }
    },
    InitKeyType = InitKeyType.Attribute,
    MoreSettings = new ConnMoreSettings
    {
        IsAutoRemoveDataCache = true
    }
});

// 启用 SQL 日志输出
db.Aop.OnLogExecuting = (sql, pars) =>
{
    Console.WriteLine($"SQL: {sql}");
    Console.WriteLine($"Parameters: {string.Join(", ", pars.Select(p => $"{p.ParameterName}={p.Value}"))}");
};
```

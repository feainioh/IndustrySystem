# ?? 数据库列名映射快速参考

## ?? 问题
```
MySqlConnector.MySqlException: Unknown column 'CreationTime' in 'field list'
```

## ? 解决方案
为所有实体类添加 `[SugarColumn]` 特性映射

## ?? 修改清单

### 已修改的实体类（11个）
- ? User
- ? Role  
- ? Permission
- ? UserRole
- ? RolePermission
- ? ExperimentTemplate
- ? Experiment
- ? ExperimentGroup
- ? Device
- ? CanMotor
- ? EthercatMotor

### 已修改的项目文件（1个）
- ? IndustrySystem.Domain.csproj (添加 SqlSugarCore 包)

## ?? 修改模板

```csharp
using SqlSugar;

public class EntityName
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [SugarColumn(ColumnName = "propertyname")]
    public string PropertyName { get; set; } = string.Empty;
}
```

## ??? 命名映射规则

| C# 格式 | 数据库格式 |
|---------|-----------|
| `PascalCase` | `lowercase` |
| `UserName` | `username` |
| `IsActive` | `isactive` |
| `CreationTime` | `createat` |
| `CreatedAt` | `createdat` |

## ?? 验证
```bash
# 构建成功
生成成功

# 下一步
1. 运行应用
2. 测试登录
3. 验证数据库操作
```

## ?? 详细文档
- `DATABASE_COLUMN_MAPPING_FIX.md` - 完整修复指南
- `DATABASE_FIX_SUMMARY.md` - 修改总结

# 权限控制使用指南

## 如何在菜单项中添加权限控制

### 1. 单个权限控制

如果菜单项只需要一个权限，使用单值绑定：

```xaml
<ui:NavigationViewItem Content="用户管理" Tag="Users">
    <ui:NavigationViewItem.Visibility>
        <MultiBinding Converter="{StaticResource PermissionToVisibility}">
            <Binding Path="AuthState"/>
            <Binding><Binding.Source>Users.View</Binding.Source></Binding>
        </MultiBinding>
    </ui:NavigationViewItem.Visibility>
</ui:NavigationViewItem>
```

### 2. 多个权限控制（OR 逻辑）

如果菜单项需要多个权限之一即可显示（OR 逻辑）：

```xaml
<ui:NavigationViewItem Content="系统管理">
    <ui:NavigationViewItem.Visibility>
        <MultiBinding Converter="{StaticResource PermissionToVisibility}">
            <Binding Path="AuthState"/>
            <Binding><Binding.Source>SuperAdmin</Binding.Source></Binding>
            <Binding><Binding.Source>Admin</Binding.Source></Binding>
        </MultiBinding>
    </ui:NavigationViewItem.Visibility>
</ui:NavigationViewItem>
```

上述示例中，只要用户拥有 `SuperAdmin` 或 `Admin` 权限之一，菜单项就会显示。

### 3. 无需权限控制

如果菜单项对所有登录用户可见，不要添加 Visibility 绑定：

```xaml
<ui:NavigationViewItem Content="运行看板" Tag="Dashboard">
    <!-- 无 Visibility 绑定，所有人可见 -->
</ui:NavigationViewItem>
```

## 如何在代码中检查权限

### 1. 在 ViewModel 中

```csharp
public class MyViewModel : BindableBase
{
    private readonly IAuthState _authState;
    
    public MyViewModel(IAuthState authState)
    {
        _authState = authState;
    }
    
    private void SomeMethod()
    {
        if (_authState.HasPermission("Users.Edit"))
        {
            // 有编辑用户权限
        }
        
        if (_authState.IsAuthenticated)
        {
            // 已登录
        }
    }
}
```

### 2. 在 View Code-Behind 中

```csharp
public partial class MyView : UserControl
{
    private readonly IAuthState _authState;
    
    public MyView()
    {
        InitializeComponent();
        _authState = ContainerLocator.Current.Resolve<IAuthState>();
        
        // 监听权限变化
        _authState.AuthChanged += OnAuthChanged;
    }
    
    private void OnAuthChanged(object sender, EventArgs e)
    {
        // 更新 UI 状态
        UpdateButtonsVisibility();
    }
    
    private void UpdateButtonsVisibility()
    {
        EditButton.IsEnabled = _authState.HasPermission("Users.Edit");
        DeleteButton.IsEnabled = _authState.HasPermission("Users.Delete");
    }
}
```

## 权限命名约定

建议使用以下命名模式：

- `{Resource}.View` - 查看资源的权限（例如：`Users.View`）
- `{Resource}.Edit` - 编辑资源的权限（例如：`Users.Edit`）
- `{Resource}.Delete` - 删除资源的权限（例如：`Users.Delete`）
- `{Resource}.Create` - 创建资源的权限（例如：`Users.Create`）
- `{Action}` - 特殊操作权限（例如：`SuperAdmin`, `Admin`）

## 添加新权限到数据库

在 `SqlSugarDatabaseInitializer.cs` 中添加新权限：

```csharp
private async Task SeedPermissionsAsync()
{
    var permissions = new[]
    {
        new Permission { Id = Guid.NewGuid(), Name = "Users.View", DisplayName = "查看用户" },
        new Permission { Id = Guid.NewGuid(), Name = "Users.Edit", DisplayName = "编辑用户" },
        // 添加新权限
        new Permission { Id = Guid.NewGuid(), Name = "Reports.View", DisplayName = "查看报表" },
        new Permission { Id = Guid.NewGuid(), Name = "Reports.Export", DisplayName = "导出报表" },
    };
    
    // ...保存到数据库
}
```

## 为角色分配权限

在 `SqlSugarDatabaseInitializer.cs` 中配置角色权限：

```csharp
private async Task SeedRolePermissionsAsync(Guid adminRoleId, Guid[] permissionIds)
{
    var rolePermissions = new List<RolePermission>();
    
    foreach (var permId in permissionIds)
    {
        rolePermissions.Add(new RolePermission
        {
            RoleId = adminRoleId,
            PermissionId = permId
        });
    }
    
    await _rolePermissionRepo.InsertRangeAsync(rolePermissions);
}
```

## 在 Strings.resx 中添加权限显示名

确保在 `Strings.resx` 和 `Strings.Designer.cs` 中添加对应的本地化字符串：

```xml
<!-- Strings.resx -->
<data name="Permission_Reports_View" xml:space="preserve">
    <value>查看报表</value>
</data>
```

```csharp
// Strings.Designer.cs
public static string Permission_Reports_View {
    get {
        return ResourceManager.GetString("Permission_Reports_View", resourceCulture);
    }
}
```

## 常见问题

### Q: 权限检查是否区分大小写？
A: 不区分。权限名称使用 `OrdinalIgnoreCase` 比较。

### Q: 如何实现 AND 逻辑（同时需要多个权限）？
A: 当前转换器只支持 OR 逻辑。如需 AND 逻辑，需要创建新的转换器或在 ViewModel 中检查。

### Q: 如何隐藏整个功能模块？
A: 在父级 `NavigationViewItem` 上添加权限控制，所有子项会自动隐藏。

### Q: 权限变化后如何刷新 UI？
A: `AuthState.AuthChanged` 事件会自动触发，绑定会自动更新。如需手动刷新，调用 `RaisePropertyChanged(nameof(AuthState))`。

### Q: 如何在运行时动态修改用户权限？
A: 
1. 更新数据库中的角色-权限关联
2. 调用 `AuthService.GetIdentityAsync()` 重新获取权限
3. 调用 `AuthState.SetAuthenticated()` 更新状态
4. UI 会自动刷新

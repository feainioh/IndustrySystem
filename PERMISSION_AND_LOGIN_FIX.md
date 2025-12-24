# 权限管理和登录功能修复说明

## 修复的问题

1. **应用启动时未显示登录界面** - 之前应用直接显示主窗口，没有进行身份验证
2. **权限管理未生效** - 主窗口中的菜单项没有根据用户权限显示/隐藏
3. **缺少登出功能** - 用户无法登出并切换账户

## 主要修改

### 1. App.xaml.cs
- **修改 `CreateShell()` 方法** - 不再直接创建 Shell 窗口，而是返回 null
- **修改 `OnStartup()` 方法** - 启动时先初始化数据库，然后显示登录窗口
- **添加 `ShowLoginWindow()` 方法** - 创建并显示登录窗口，监听认证状态变化
- **添加 `ShowShellWindow()` 方法** - 登录成功后显示主窗口
- **注册所有 ViewModel** - 为所有 View 注册对应的 ViewModel

### 2. Shell.xaml.cs
- **添加 ShellViewModel** - 设置 ViewModel 作为 DataContext
- **监听认证状态变化** - 更新用户信息显示
- **添加登出功能** - `OnLogoutClick()` 方法处理登出操作

### 3. Shell.xaml
- **添加转换器资源** - 添加 `PermissionToVisibilityConverter` 用于权限控制
- **添加用户信息显示** - 在底部显示当前登录用户名
- **添加登出按钮** - 在用户信息区域添加登出按钮

### 4. AuthService.cs & AuthState.cs
这两个服务已存在并正常工作：
- `AuthService` - 处理登录验证和权限查询
- `AuthState` - 维护全局认证状态和权限信息
- `AuthChanged` 事件 - 通知认证状态变化

## 工作流程

### 启动流程
1. 应用启动 → 初始化数据库
2. 显示登录窗口
3. 用户输入账号密码 → 点击登录
4. AuthService 验证用户 → 获取角色和权限
5. AuthState 更新认证状态 → 触发 AuthChanged 事件
6. 关闭登录窗口 → 显示 Shell 主窗口
7. Shell 根据用户权限显示可用菜单

### 权限控制
Shell.xaml 中使用 `PermissionToVisibilityConverter` 控制菜单项的可见性：

```xaml
<ui:NavigationViewItem.Visibility>
    <MultiBinding Converter="{StaticResource PermissionToVisibility}">
        <Binding Path="AuthState"/>
        <Binding><Binding.Source>Users.View</Binding.Source></Binding>
    </MultiBinding>
</ui:NavigationViewItem.Visibility>
```

### 登出流程
1. 用户点击登出按钮
2. AuthState.SignOut() - 清除认证状态
3. 关闭 Shell 窗口
4. 重新显示登录窗口

## 权限定义

系统预定义的权限包括（在 SqlSugarDatabaseInitializer 中配置）：
- `SuperAdmin` - 超级管理员权限
- `Admin` - 管理员权限
- `Users.View` - 查看用户
- `Users.Edit` - 编辑用户
- `Experiments.View` - 查看实验
- `Templates.View` - 查看模板
- `Templates.Edit` - 编辑模板
- `Material.View` - 查看物料
- `Material.Edit` - 编辑物料
- `Inventory.View` - 查看库存
- `Device.Maintain` - 设备维护

## 测试步骤

### 1. 测试登录功能
1. 启动应用 - 应该显示登录窗口
2. 使用默认账户登录：
   - 用户名: `admin`
   - 密码: `admin`
3. 点击登录 - 应该进入主窗口

### 2. 测试权限控制
1. 登录后检查左侧菜单
2. 根据当前用户的角色和权限，某些菜单项应该显示/隐藏
3. 超级管理员应该能看到所有菜单
4. 普通用户只能看到有权限的菜单

### 3. 测试登出功能
1. 在主窗口底部找到用户信息区域
2. 点击"登出"按钮
3. 应该返回登录窗口
4. 可以使用不同账户重新登录

### 4. 测试快速登录
1. 在登录窗口点击"快速登录"按钮
2. 应该使用默认的 admin/admin 账户登录

## 数据库初始化

系统会在首次启动时自动初始化以下数据：
- 默认管理员账户: admin/admin
- 超级管理员角色
- 管理员角色
- 默认权限列表
- 用户-角色关联
- 角色-权限关联

## 注意事项

1. **权限字符串大小写不敏感** - 权限检查时忽略大小写
2. **OR 逻辑** - MultiBinding 中的多个权限采用 OR 逻辑（任一满足即可）
3. **认证状态是单例** - IAuthState 注册为 Singleton，全局共享
4. **密码未加密** - 当前密码是明文存储，生产环境需要加密

## 后续改进建议

1. **密码加密** - 实现密码哈希存储
2. **记住登录** - 实现自动登录功能（已有界面，需完善逻辑）
3. **会话超时** - 添加会话超时自动登出
4. **权限缓存** - 优化权限查询性能
5. **审计日志** - 记录登录/登出操作
6. **多语言支持** - 完善国际化资源文件

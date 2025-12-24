# ?? 重要说明：MaterialDesign 优化文件使用指南

## 文件说明

由于项目已有 `Shell.xaml` 和相关视图文件，新的 MaterialDesign 优化版本使用不同的文件名以避免冲突。

## 文件对应关系

### 主界面

| 原文件 | MaterialDesign 版本 | 说明 |
|--------|---------------------|------|
| Shell.xaml | Shell_MaterialDesign.xaml | 新的抽屉式导航主界面 |
| Shell.xaml.cs | Shell_MaterialDesign.xaml.cs | 对应的 Code-Behind |

### 视图文件

| 原文件 | MaterialDesign 版本 | 说明 |
|--------|---------------------|------|
| UsersView.xaml | UsersView_Material.xaml | 用户管理（标准模板） |
| RoleManageView.xaml | RoleManageView_Material.xaml | 角色管理（标准模板） |
| RealtimeDataView.xaml | RealtimeDataView_Material.xaml | 实时数据（仪表板模板） |

## 使用方式

### 方式一：完全切换（推荐）

1. **备份现有文件**
```bash
# 重命名现有文件为 .old
Shell.xaml → Shell.xaml.old
Shell.xaml.cs → Shell.xaml.cs.old
UsersView.xaml → UsersView.xaml.old
...
```

2. **重命名 MaterialDesign 文件**
```bash
# 使用新的 MaterialDesign 文件
Shell_MaterialDesign.xaml → Shell.xaml
Shell_MaterialDesign.xaml.cs → Shell.xaml.cs
UsersView_Material.xaml → UsersView.xaml
...
```

3. **更新项目引用**
- 在项目文件中更新对 Shell 的引用
- 在 App.xaml.cs 中更新 ShellView 的创建

### 方式二：并行使用（测试用）

保持当前文件名，可以在代码中选择性加载：

```csharp
// App.xaml.cs 或 Startup 逻辑
bool useMaterialDesign = true; // 切换标志

if (useMaterialDesign)
{
    shell = container.Resolve<Shell_MaterialDesign>();
}
else
{
    shell = container.Resolve<Shell>();
}
```

### 方式三：逐步迁移（稳妥）

1. 先迁移登录界面（已完成）
2. 创建新的视图时直接使用 Material 模板
3. 逐个更新现有视图
4. 最后更新 Shell 主界面

## 需要创建的 Code-Behind 文件

由于我们只创建了 XAML 模板，还需要对应的 `.xaml.cs` 文件：

### UsersView_Material.xaml.cs
```csharp
using System;
using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class UsersView_Material : UserControl
    {
        public UsersView_Material()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.UsersViewModel vm && 
                UserNameBox != null && DisplayNameBox != null)
            {
                _ = vm.AddAsync(UserNameBox.Text, DisplayNameBox.Text);
                UserNameBox.Text = string.Empty;
                DisplayNameBox.Text = string.Empty;
            }
        }

        private void OnResetPassword(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.UsersViewModel vm)
            {
                _ = vm.ResetPasswordAsync(id);
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            // TODO: Implement edit dialog
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.UsersViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}
```

### RoleManageView_Material.xaml.cs
```csharp
using System;
using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RoleManageView_Material : UserControl
    {
        public RoleManageView_Material()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RoleManageViewModel vm && 
                RoleNameBox != null)
            {
                _ = vm.AddAsync(RoleNameBox.Text);
                RoleNameBox.Text = string.Empty;
                DescBox.Text = string.Empty;
            }
        }

        private void OnPermissions(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.RoleManageViewModel vm)
            {
                _ = vm.ManagePermissionsAsync(id);
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            // TODO: Implement edit dialog
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.RoleManageViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}
```

### RealtimeDataView_Material.xaml.cs
```csharp
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RealtimeDataView_Material : UserControl
    {
        public RealtimeDataView_Material()
        {
            InitializeComponent();
        }
    }
}
```

## 更新 ShellViewModel 导航

如果使用 MaterialDesign 版本的视图，需要更新 `ShellViewModel.cs` 的 `Navigate` 方法：

```csharp
private void Navigate(string tag)
{
    object? next = tag switch
    {
        "Users" => _container.Resolve<UsersView_Material>(), // 新版本
        "Roles" => _container.Resolve<RoleManageView_Material>(), // 新版本
        "RealtimeData" => _container.Resolve<RealtimeDataView_Material>(), // 新版本
        // ... 其他路由
        _ => null
    };
    if (next != null)
    {
        CurrentContent = next;
    }
}
```

## 注册视图

在 Prism 容器注册中添加新视图（如果使用依赖注入）：

```csharp
// App.xaml.cs 或 Module 注册
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // 注册 MaterialDesign 视图
    containerRegistry.Register<UsersView_Material>();
    containerRegistry.Register<RoleManageView_Material>();
    containerRegistry.Register<RealtimeDataView_Material>();
    containerRegistry.Register<Shell_MaterialDesign>();
}
```

## 常见问题

### Q1: 编译错误 "未找到类型"
**A**: 需要创建对应的 `.xaml.cs` Code-Behind 文件

### Q2: Shell 冲突错误
**A**: 使用 Shell_MaterialDesign 而不是直接替换 Shell

### Q3: PackIcon 无法识别 Kind
**A**: 确保安装了 MaterialDesignThemes.Wpf 包，版本 >= 5.0

### Q4: 样式未应用
**A**: 确保 MaterialTheme.xaml 在 App.xaml 中被正确引用

### Q5: 特殊字符编码错误（如 °C）
**A**: 检查文件编码为 UTF-8，或使用 HTML 实体如 &#176;C

## 推荐迁移步骤

### 第1天：准备工作
1. ? 确认 MaterialTheme.xaml 已创建并在 App.xaml 中引用
2. ? 确认 LoginView 已更新（已完成）
3. ? 创建所有 Code-Behind 文件

### 第2天：核心视图
4. ? 创建 UsersView_Material.xaml.cs
5. ? 创建 RoleManageView_Material.xaml.cs
6. ? 测试用户和角色管理功能

### 第3天：数据视图
7. ? 创建 RealtimeDataView_Material.xaml.cs
8. ? 更新其他管理类视图
9. ? 测试所有功能

### 第4天：主界面和收尾
10. ? 集成 Shell_MaterialDesign
11. ? 全面测试导航和交互
12. ? 性能优化和调整

## 快速开始命令

```bash
# 1. 创建 Code-Behind 文件（手动创建或使用以下内容）
# 在 Visual Studio 中右键 -> Add -> New Item -> Code File

# 2. 更新 ShellViewModel 导航
# 编辑 ShellViewModel.cs，更新 Navigate 方法

# 3. 注册新视图
# 编辑 App.xaml.cs 或对应的 Module

# 4. 编译和测试
# Ctrl+Shift+B 编译
# F5 运行和测试
```

## 联系和支持

如果在迁移过程中遇到问题：
1. 查看 MATERIALDESIGN_INTEGRATION_GUIDE.md
2. 参考 MATERIALDESIGN_QUICK_REF.md
3. 检查 MATERIALDESIGN_IMPLEMENTATION_SUMMARY.md

---

**重要提示**: 建议在版本控制系统（Git）中提交当前代码后再进行大规模迁移，以便随时回退。

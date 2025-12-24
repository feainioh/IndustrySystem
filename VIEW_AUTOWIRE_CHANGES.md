# View AutoWireViewModel 修改清单

## 需要将 AutoWireViewModel 设置为 False 的文件

根据代码审查，以下 XAML 文件需要将 `prism:ViewModelLocator.AutoWireViewModel="True"` 修改为 `prism:ViewModelLocator.AutoWireViewModel="False"`：

### 1. UsersView.xaml
**文件路径**: `src\Presentation\IndustrySystem.Presentation.Wpf\Views\UsersView.xaml`

**当前代码**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.UsersView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="True">
```

**应修改为**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.UsersView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="False">
```

**Code-behind 中手动设置 DataContext**:
```csharp
public UsersView()
{
    InitializeComponent();
    if (DataContext == null)
    {
        DataContext = ContainerLocator.Current.Resolve<ViewModels.UsersViewModel>();
    }
}
```

---

### 2. PermissionsView.xaml
**文件路径**: `src\Presentation\IndustrySystem.Presentation.Wpf\Views\PermissionsView.xaml`

**当前代码**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.PermissionsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="True">
```

**应修改为**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.PermissionsView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="False">
```

**Code-behind 应包含**:
```csharp
public PermissionsView()
{
    InitializeComponent();
    if (DataContext == null)
    {
        DataContext = ContainerLocator.Current.Resolve<ViewModels.PermissionsViewModel>();
    }
}
```

---

### 3. RoleManageView.xaml
**文件路径**: `src\Presentation\IndustrySystem.Presentation.Wpf\Views\RoleManageView.xaml`

**当前代码**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.RoleManageView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:md="http://materialdesigninxaml.net/winfx/xaml/themes"
             prism:ViewModelLocator.AutoWireViewModel="True"
             xmlns:prism="http://prismlibrary.com/">
```

**应修改为**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.RoleManageView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:md="http://materialdesigninxaml.net/winfx/xaml/themes"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="False">
```

**Code-behind 应包含**:
```csharp
public RoleManageView()
{
    InitializeComponent();
    if (DataContext == null)
    {
        DataContext = ContainerLocator.Current.Resolve<ViewModels.RoleManageViewModel>();
    }
}
```

---

## 已正确配置的 Views (手动设置 DataContext)

以下 Views 已经在 code-behind 中手动设置 DataContext，不使用 AutoWireViewModel：

1. ? **DeviceParamsView.xaml.cs** - 手动 Resolve
2. ? **InventoryView.xaml.cs** - 手动 Resolve
3. ? **RealtimeDataView.xaml.cs** - 手动 Resolve
4. ? **LoginView.xaml.cs** - 手动 Resolve（还包含 PasswordBox 特殊处理）
5. ? **ExperimentConfigView.xaml.cs** - 手动 Resolve
6. ? **RunExperimentView.xaml** - 没有 AutoWireViewModel 属性（需要添加 code-behind）

---

## App.xaml.cs 中的 ViewModel 注册

以下 ViewModels 已在 `App.xaml.cs` 中使用 `ViewModelLocationProvider.Register` 注册：

```csharp
ViewModelLocationProvider.Register<Views.RoleManageView, RoleManageViewModel>();
ViewModelLocationProvider.Register<Views.ExperimentTemplateView, ExperimentTemplateViewModel>();
ViewModelLocationProvider.Register<Views.PermissionsView, PermissionsViewModel>();
ViewModelLocationProvider.Register<Views.UsersView, UsersViewModel>();
ViewModelLocationProvider.Register<Views.LoginView, LoginViewModel>();
```

**注意**: 这些注册在使用手动 DataContext 设置时可能不是必需的，但保留它们不会造成问题。

---

## 修改原因

将 `AutoWireViewModel` 设置为 `False` 并手动设置 DataContext 的好处：

1. **更明确的控制** - 开发者清楚地知道 DataContext 何时和如何被设置
2. **更容易调试** - 可以在构造函数中设置断点查看 ViewModel 的创建
3. **更灵活** - 可以添加条件逻辑或特殊处理（如 LoginView 中的 PasswordBox）
4. **一致性** - 项目中所有 Views 使用相同的模式

---

## 建议的统一模式

对于所有 UserControl Views，建议使用以下统一模式：

**XAML**:
```xaml
<UserControl x:Class="IndustrySystem.Presentation.Wpf.Views.YourView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:prism="http://prismlibrary.com/"
             prism:ViewModelLocator.AutoWireViewModel="False">
    <!-- Content -->
</UserControl>
```

**Code-behind**:
```csharp
using Prism.Ioc;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class YourView : UserControl
    {
        public YourView()
        {
            InitializeComponent();
            if (DataContext == null)
            {
                DataContext = ContainerLocator.Current.Resolve<ViewModels.YourViewModel>();
            }
        }
    }
}
```

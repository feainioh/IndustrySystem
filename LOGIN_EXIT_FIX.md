# 登录后程序退出问题修复

## ?? 问题描述

登录成功后，程序直接退出，没有显示 Shell 主窗口，也没有任何错误提示。

## ?? 问题原因

WPF 应用程序的默认 `ShutdownMode` 是 `OnLastWindowClose`，这意味着：

1. 用户登录成功
2. `AuthChanged` 事件触发
3. 代码先关闭登录窗口：`loginWindow.Close()`
4. 此时没有其他窗口在显示
5. **WPF 检测到所有窗口都关闭，自动退出应用程序**
6. `ShowShellWindow()` 还没来得及执行
7. 程序退出

## ? 解决方案

### 修改 1: App.xaml - 设置 ShutdownMode

```xaml
<prism:PrismApplication x:Class="IndustrySystem.Presentation.Wpf.App"
             ...
             ShutdownMode="OnExplicitShutdown">
```

**说明**：
- `OnExplicitShutdown` 模式下，应用程序不会因为窗口关闭而自动退出
- 必须显式调用 `Application.Shutdown()` 才能退出应用

### 修改 2: App.xaml.cs - 调整窗口显示顺序

**修改前：**
```csharp
authState.AuthChanged += (s, e) =>
{
    if (authState.IsAuthenticated)
    {
        // 先关闭登录窗口 - 导致应用退出
        loginWindow.Close();
        ShowShellWindow(); // 来不及执行
    }
};
```

**修改后：**
```csharp
EventHandler? authChangedHandler = null;
authChangedHandler = (s, e) =>
{
    if (authState.IsAuthenticated)
    {
        // 先取消订阅，防止多次触发
        authState.AuthChanged -= authChangedHandler;
        
        // 先显示 Shell 窗口，再关闭登录窗口
        Dispatcher.Invoke(() =>
        {
            ShowShellWindow();     // 先显示新窗口
            loginWindow.Close();   // 再关闭登录窗口
        });
    }
};
authState.AuthChanged += authChangedHandler;
```

**关键改进**：
1. ? 先显示 Shell 窗口，再关闭登录窗口
2. ? 使用 `Dispatcher.Invoke` 确保在 UI 线程执行
3. ? 取消订阅事件处理器，防止多次触发

### 修改 3: Shell.xaml.cs - 添加应用退出逻辑

```csharp
public partial class Shell : Window
{
    private bool _isLoggingOut = false;

    public Shell(IContainerProvider container)
    {
        // ...初始化代码
        
        // 监听窗口关闭事件
        Closed += OnWindowClosed;
    }

    private void OnWindowClosed(object? sender, EventArgs e)
    {
        // 如果不是登出操作，就关闭应用
        if (!_isLoggingOut)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
        var authState = _container.Resolve<IAuthState>();
        authState.SignOut();
        
        // 标记为登出操作，防止应用退出
        _isLoggingOut = true;
        
        // 关闭 Shell 并显示登录窗口
        Close();
        ((App)System.Windows.Application.Current).ShowLoginWindow();
    }
}
```

**说明**：
- 使用 `_isLoggingOut` 标志区分正常关闭和登出操作
- 正常关闭 Shell：调用 `Application.Shutdown()` 退出应用
- 登出操作：关闭 Shell 但不退出应用，重新显示登录窗口

## ?? 修改的文件

1. ? `App.xaml` - 添加 `ShutdownMode="OnExplicitShutdown"`
2. ? `App.xaml.cs` - 调整窗口显示顺序和事件处理
3. ? `Shell.xaml.cs` - 添加应用退出和登出逻辑

## ?? WPF ShutdownMode 说明

WPF 应用程序有三种关闭模式：

| 模式 | 说明 | 使用场景 |
|------|------|----------|
| **OnLastWindowClose** (默认) | 最后一个窗口关闭时退出 | 简单应用，只有主窗口 |
| **OnMainWindowClose** | 主窗口关闭时退出 | 有多个子窗口的应用 |
| **OnExplicitShutdown** | 必须显式调用 Shutdown() | 需要精确控制退出时机 |

我们选择 `OnExplicitShutdown` 是因为：
- ? 可以在登录窗口和 Shell 窗口之间切换而不退出应用
- ? 可以精确控制何时退出应用程序
- ? 支持登出后重新登录的工作流程

## ?? 应用程序生命周期

### 启动流程
```
1. 应用启动
   ↓
2. 初始化数据库
   ↓
3. 显示登录窗口
   ↓
4. 用户登录成功
   ↓
5. 显示 Shell 窗口 ← 先执行
   ↓
6. 关闭登录窗口 ← 后执行
   ↓
7. Shell 窗口运行中
```

### 登出流程
```
1. 用户点击登出按钮
   ↓
2. 清除认证状态
   ↓
3. 设置 _isLoggingOut = true
   ↓
4. 关闭 Shell 窗口 (不退出应用)
   ↓
5. 重新显示登录窗口
```

### 正常退出流程
```
1. 用户关闭 Shell 窗口
   ↓
2. OnWindowClosed 检测到 _isLoggingOut = false
   ↓
3. 调用 Application.Shutdown()
   ↓
4. 应用程序退出
```

## ? 验证步骤

1. **启动应用**
   - 应该显示登录窗口
   
2. **登录成功**
   - 应该看到 Shell 主窗口
   - 登录窗口应该关闭
   - 应用程序不应该退出
   
3. **登出**
   - 点击登出按钮
   - Shell 窗口关闭
   - 重新显示登录窗口
   - 应用程序不应该退出
   
4. **关闭 Shell**
   - 点击 Shell 窗口的关闭按钮
   - 应用程序应该退出

## ?? 常见问题

### Q: 为什么不用 OnMainWindowClose？
A: 因为我们有两个"主"窗口（登录和 Shell），需要在它们之间切换。`OnMainWindowClose` 会在 MainWindow 关闭时立即退出。

### Q: Dispatcher.Invoke 的作用是什么？
A: 确保窗口操作在 UI 线程上执行。`AuthChanged` 事件可能在后台线程触发，而 WPF 窗口操作必须在 UI 线程执行。

### Q: 为什么要取消订阅 authChangedHandler？
A: 防止事件处理器被多次触发。一旦登录成功，就不需要再监听这个事件了。

## ?? 注意事项

1. **线程安全**
   - 使用 `Dispatcher.Invoke` 确保 UI 操作在主线程执行
   
2. **事件订阅**
   - 登录成功后取消订阅 `AuthChanged` 事件，防止内存泄漏
   
3. **退出控制**
   - 使用标志位 `_isLoggingOut` 区分不同的关闭场景
   - 显式调用 `Application.Shutdown()` 退出应用

## ?? 构建结果

? **生成成功**

应用现在可以正常：
- 启动并显示登录窗口
- 登录后显示 Shell 主窗口
- 登出后返回登录窗口
- 关闭 Shell 时正常退出应用

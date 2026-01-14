# 设备调试界面使用指南

## 目录
1. [概述](#概述)
2. [新增功能](#新增功能)
3. [使用说明](#使用说明)
4. [开发指南](#开发指南)

## 概述

设备调试界面经过重构优化，现在具有：
- ✨ 更现代化的UI设计
- 📦 模块化的设备控件
- 🔄 DebugView和PositionSettings之间的数据同步
- ➕ 位置点的新增和删除功能

## 新增功能

### 1. 优化的布局设计

#### 主要改进：
- **更大的间距**：从8px增加到16-24px，提供更好的视觉呼吸感
- **卡片式设计**：使用Material Design卡片，带有阴影效果
- **彩色按钮**：重要操作使用绿色/红色等有意义的颜色
- **图标增强**：使用更大的图标（20-28px）和更好的对比度
- **状态指示器**：清晰的设备在线/离线状态显示

### 2. 模块化设备控件

每种设备类型现在有独立的UserControl：

| 设备类型 | 控件名称 | 状态 |
|---------|---------|------|
| 电机 | MotorDebugControl | ✅ 已完成 |
| 通信设备 | CommunicationDeviceControl | ✅ 已完成 |
| IO模块 | IODeviceControl | ✅ 已完成 |
| 注射泵 | SyringePumpDebugControl | ✅ 已完成 |
| 蠕动泵 | PeristalticPumpDebugControl | ⏳ 待创建 |
| 离心机 | CentrifugalDebugControl | ⏳ 待创建 |
| TCU温控 | TcuDebugControl | ⏳ 待创建 |
| 其他 | ... | ⏳ 待创建 |

### 3. 位置设置功能增强

#### 新增位置点：
```csharp
// 点击"新增"按钮后：
1. 自动选择可用设备
2. 生成默认位置名称（Position_1, Position_2, ...）
3. 设置默认值（位置=0, 速度=100）
4. 同时更新到配置和UI列表
5. 自动选中新建的位置点供编辑
```

#### 删除位置点：
```csharp
// 点击"删除"按钮后：
1. 弹出确认对话框
2. 从配置文件中删除
3. 从UI列表中删除
4. 更新统计信息
5. 清除选择状态
```

### 4. 数据同步机制

使用Prism的EventAggregator实现跨ViewModel通信：

```csharp
// 事件类型
- DeviceConfigImportedEvent    // 配置导入事件
- DeviceConfigSavedEvent        // 配置保存事件
- PositionUpdatedEvent          // 位置更新事件
- PositionAddedEvent            // 位置添加事件
- PositionDeletedEvent          // 位置删除事件
```

## 使用说明

### 导入配置

1. 点击顶部工具栏的"导入配置"按钮
2. 选择JSON配置文件
3. 系统自动加载所有设备和位置点
4. 左侧设备树自动展开分类

### 选择设备进行调试

1. 在左侧设备树中选择设备
2. 右侧自动显示对应的调试控件
3. 可以进行设备连接、参数设置等操作

### 管理工作位置

#### 在DeviceDebugView中：
1. 选择电机设备
2. 查看"工作位置"区域
3. 可以运行到点位、读取当前位置

#### 在PositionSettingsView中：
1. 查看所有设备的位置点列表
2. 点击"新增"添加位置点
3. 选择位置点进行编辑
4. 点击"删除"移除位置点
5. 使用"示教"按钮获取当前位置
6. 点击"保存"保存所有更改

### 位置点编辑流程

```
1. 导入配置 → 2. 选择设备 → 3. 新增/编辑位置 → 4. 保存配置
                    ↓                                    ↑
                5. 调试验证 ← 6. 运行到点位 ← 7. 示教位置
```

## 开发指南

### 添加新的设备控件

#### 步骤1：创建XAML文件

```xaml
<!-- YourDeviceDebugControl.xaml -->
<UserControl x:Class="IndustrySystem.MotionDesigner.Controls.DeviceDebug.YourDeviceDebugControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:materialDesign="http://materialdesigninxaml.net/winfx/xaml/themes">
    
    <materialDesign:Card Padding="24" Margin="0,0,0,16" materialDesign:ElevationAssist.Elevation="Dp2">
        <!-- 你的控件内容 -->
    </materialDesign:Card>
</UserControl>
```

#### 步骤2：创建代码behind

```csharp
// YourDeviceDebugControl.xaml.cs
using System.Windows.Controls;

namespace IndustrySystem.MotionDesigner.Controls.DeviceDebug;

public partial class YourDeviceDebugControl : UserControl
{
    public YourDeviceDebugControl()
    {
        InitializeComponent();
    }
}
```

#### 步骤3：在DeviceDebugView.xaml中引用

```xaml
<!-- 在命名空间声明中 -->
xmlns:deviceDebug="clr-namespace:IndustrySystem.MotionDesigner.Controls.DeviceDebug"

<!-- 在设备详情区域添加 -->
<deviceDebug:YourDeviceDebugControl 
    DataContext="{Binding}"
    Visibility="{Binding IsYourDeviceSelected, Converter={StaticResource BoolToVisConverter}}"/>
```

#### 步骤4：在DeviceDebugViewModel中添加属性

```csharp
public bool IsYourDeviceSelected => SelectedDevice?.DeviceType == "YourDeviceType";
```

### 设计规范

#### 布局间距：
- 卡片外边距：`Margin="0,0,0,16"`
- 卡片内边距：`Padding="24"`
- 区块间距：`Margin="0,0,0,16"`
- 控件间距：`Margin="0,0,12,0"` (水平) 或 `Margin="0,0,0,12"` (垂直)

#### 字体大小：
- 标题：`FontSize="20"` `FontWeight="SemiBold"`
- 副标题：`FontSize="16"` `FontWeight="SemiBold"`
- 正文：`FontSize="14"`
- 提示文字：`FontSize="12"` `Opacity="0.7"`

#### 按钮高度：
- 标准按钮：`Height="36"`
- 主要按钮：使用 `MaterialDesignRaisedButton` 样式
- 次要按钮：使用 `MaterialDesignOutlinedButton` 样式

#### 颜色使用：
- 成功/连接：`Background="#4CAF50"` (绿色)
- 危险/停止：`Background="Red"`
- 警告：`Background="Orange"`
- 主题色：使用 `{DynamicResource PrimaryHueMidBrush}`

### 数据绑定模式

所有设备控件使用统一的DataContext（DeviceDebugViewModel），可以直接绑定：

```xaml
<!-- 绑定到ViewModel的属性 -->
<TextBox Text="{Binding MotorTargetPosition}"/>

<!-- 绑定到ViewModel的命令 -->
<Button Command="{Binding MotorMoveCommand}"/>

<!-- 绑定到ViewModel的可见性 -->
<StackPanel Visibility="{Binding IsMotorSelected, Converter={StaticResource BoolToVisConverter}}"/>
```

### 事件发布示例

```csharp
// 在导入配置后发布事件
_eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);

// 在添加位置后发布事件
_eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);

// 在更新位置后发布事件
var eventArgs = new PositionUpdatedEventArgs
{
    DeviceId = position.DeviceId,
    PositionName = position.PositionName,
    Position = position.Position,
    Speed = position.Speed
};
_eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(eventArgs);
```

### 事件订阅示例

```csharp
// 在构造函数中订阅事件
public YourViewModel(IEventAggregator eventAggregator)
{
    _eventAggregator = eventAggregator;
    
    _eventAggregator.GetEvent<DeviceConfigImportedEvent>()
        .Subscribe(OnConfigImported);
    
    _eventAggregator.GetEvent<PositionAddedEvent>()
        .Subscribe(OnPositionAdded);
}

// 事件处理方法
private void OnConfigImported(DeviceConfigDto config)
{
    // 处理配置导入
    CurrentConfig = config;
    LoadDevices();
}

private void OnPositionAdded(PositionPointViewModel position)
{
    // 处理位置添加
    AllPositions.Add(position);
    UpdateStatistics();
}
```

## 最佳实践

### 1. 错误处理
```csharp
try
{
    StatusMessage = "正在执行操作...";
    await SomeAsyncOperation();
    StatusMessage = "操作成功";
}
catch (Exception ex)
{
    _logger.Error(ex, "操作失败");
    StatusMessage = $"失败: {ex.Message}";
}
```

### 2. 用户反馈
- 操作前：显示"正在..."消息
- 操作成功：显示成功消息
- 操作失败：显示错误详情
- 长时间操作：显示进度指示器

### 3. 数据验证
```csharp
if (string.IsNullOrWhiteSpace(PositionName))
{
    StatusMessage = "位置名称不能为空";
    return;
}

if (Position < 0 || Position > MaxPosition)
{
    StatusMessage = "位置超出范围";
    return;
}
```

### 4. 异步操作
```csharp
// 使用 async/await
private async Task ConnectDeviceAsync()
{
    await Task.Run(() => 
    {
        // 耗时操作
    });
}

// 在命令中使用
ConnectCommand = new DelegateCommand(async () => await ConnectDeviceAsync());
```

## 常见问题

### Q: 如何添加新的设备类型？
A: 参考"添加新的设备控件"章节，创建对应的UserControl和ViewModel属性。

### Q: 为什么我的控件不显示？
A: 检查以下几点：
1. ViewModel中的 `IsXXXSelected` 属性是否正确
2. `Visibility` 绑定是否正确
3. DataContext是否正确传递

### Q: 如何自定义控件样式？
A: 遵循Material Design规范，使用主题资源：
```xaml
Background="{DynamicResource MaterialDesignPaper}"
Foreground="{DynamicResource PrimaryHueMidBrush}"
```

### Q: 数据同步不工作？
A: 确保：
1. EventAggregator正确注入
2. 事件正确发布和订阅
3. 事件处理方法正确实现

## 参考资料

- [Material Design in XAML](http://materialdesigninxaml.net/)
- [Prism Framework](https://prismlibrary.com/)
- [MVVM Pattern](https://docs.microsoft.com/en-us/xamarin/xamarin-forms/enterprise-application-patterns/mvvm)

## 更新日志

### v1.0.0 (2024)
- ✅ 重构DeviceDebugView布局
- ✅ 创建模块化设备控件
- ✅ 实现位置点新增/删除功能
- ✅ 添加事件系统支持数据同步
- ✅ 优化UI设计和用户体验

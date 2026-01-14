# 重构后的设备调试系统使用手册

## 🎯 快速开始

重构已完成！现在的设备调试系统更加模块化和易于维护。

## 📂 文件结构

```
ViewModels/
├── DeviceDebugViewModel.cs          # 主协调者
└── DeviceDebug/                     # 子 ViewModel 目录
    ├── MotorDebugViewModel.cs       # 电机调试
    ├── SyringePumpDebugViewModel.cs # 注射泵
    ├── PeristalticPumpDebugViewModel.cs # 蠕动泵
    ├── DiyPumpDebugViewModel.cs     # 自定义泵
    ├── IODeviceDebugViewModel.cs    # IO 模块
    ├── TCUDebugViewModel.cs         # TCU 温控
    ├── RobotDebugViewModel.cs       # 机器人
    ├── ScannerDebugViewModel.cs     # 扫码枪
    ├── CentrifugalDebugViewModel.cs # 离心机
    ├── WeightSensorDebugViewModel.cs# 称重传感器
    └── ChillerDebugViewModel.cs     # 冷水机
```

## 🔧 如何使用

### 1. 访问子 ViewModel

在主 ViewModel 中，所有子 ViewModel 都已实例化并可直接访问：

```csharp
public class DeviceDebugViewModel : BindableBase
{
    // 直接访问子 ViewModel
    public MotorDebugViewModel MotorDebugVM { get; }
    public SyringePumpDebugViewModel SyringePumpDebugVM { get; }
    // ... 其他子 ViewModel
}
```

### 2. 设备自动更新

当选择设备时，对应的子 ViewModel 会自动更新：

```csharp
// 当用户选择电机时
SelectedDevice = motorDeviceItem;
// 自动执行：MotorDebugVM.SelectedMotor = motor;
```

### 3. XAML 中使用

每个调试控件已绑定到对应的子 ViewModel：

```xaml
<!-- 电机调试控件 -->
<deviceDebug:MotorDebugControl 
    DataContext="{Binding MotorDebugVM}"
    Visibility="{Binding DataContext.IsAnyMotorSelected, 
                RelativeSource={RelativeSource AncestorType=UserControl}, 
                Converter={StaticResource BoolToVisConverter}}"/>
```

## 🎨 添加新设备类型

### 步骤 1：创建 ViewModel

```csharp
public class NewDeviceDebugViewModel : BindableBase
{
    private readonly IHardwareController _hardwareController;
    private NewDeviceDto? _selectedDevice;
    
    public NewDeviceDto? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                OnDeviceChanged();
            }
        }
    }
    
    public NewDeviceDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        // 初始化命令
    }
    
    private void OnDeviceChanged()
    {
        // 设备改变时的处理
    }
}
```

### 步骤 2：在主 ViewModel 中添加

```csharp
// 1. 添加属性
public NewDeviceDebugViewModel NewDeviceDebugVM { get; }

// 2. 在构造函数中实例化
NewDeviceDebugVM = new NewDeviceDebugViewModel(hardwareController);

// 3. 在 UpdateDeviceDetails 中添加 case
case NewDeviceDto newDevice:
    NewDeviceDebugVM.SelectedDevice = newDevice;
    break;
```

### 步骤 3：在 XAML 中添加控件

```xaml
<deviceDebug:NewDeviceControl 
    DataContext="{Binding NewDeviceDebugVM}"
    Visibility="{Binding DataContext.IsNewDeviceSelected, 
                RelativeSource={RelativeSource AncestorType=UserControl}, 
                Converter={StaticResource BoolToVisConverter}}"/>
```

## 💡 最佳实践

### 1. 属性命名
使用设备类型作为前缀：
```csharp
// ✅ 正确
public double MotorPosition { get; set; }
public string SyringeStatus { get; set; }

// ❌ 错误
public double Position { get; set; }
public string Status { get; set; }
```

### 2. 命令命名
使用设备类型 + 动作：
```csharp
// ✅ 正确
public ICommand MotorMoveCommand { get; }
public ICommand SyringeInitCommand { get; }

// ❌ 错误
public ICommand MoveCommand { get; }
public ICommand InitCommand { get; }
```

### 3. 状态管理
在设备切换时重置状态：
```csharp
private void OnDeviceChanged()
{
    if (SelectedDevice != null)
    {
        // 重置状态
        IsConnected = false;
        Status = string.Empty;
        // 应用设备默认参数
        Speed = SelectedDevice.DefaultSpeed;
    }
}
```

### 4. 错误处理
始终使用 try-catch 并记录日志：
```csharp
private async Task ConnectAsync()
{
    if (SelectedDevice == null) return;
    
    try
    {
        Status = "正在连接...";
        await _hardwareController.ConnectAsync(SelectedDevice.DeviceId);
        IsConnected = true;
        Status = "已连接";
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "连接失败");
        Status = $"连接失败: {ex.Message}";
    }
}
```

## 🧪 测试示例

### 单元测试示例

```csharp
[Test]
public async Task MotorMove_ShouldUpdatePosition()
{
    // Arrange
    var mockController = new Mock<IHardwareController>();
    var viewModel = new MotorDebugViewModel(mockController.Object);
    var motor = new MotorDto { DeviceId = "motor1", Name = "测试电机" };
    viewModel.SelectedMotor = motor;
    viewModel.MotorTargetPosition = 100;
    
    // Act
    await viewModel.MotorMoveCommand.Execute();
    
    // Assert
    mockController.Verify(x => x.MoveMotorAsync(
        "motor1", 100, It.IsAny<double>(), It.IsAny<bool>(), true), 
        Times.Once);
}
```

## 📊 常用命令模式

### 简单命令
```csharp
public ICommand ConnectCommand { get; }

// 构造函数中
ConnectCommand = new DelegateCommand(async () => await ConnectAsync());

private async Task ConnectAsync()
{
    // 实现逻辑
}
```

### 带参数的命令
```csharp
public ICommand QuickMoveCommand { get; }

// 构造函数中
QuickMoveCommand = new DelegateCommand<string>(async position => await QuickMoveAsync(position));

private async Task QuickMoveAsync(string? position)
{
    if (string.IsNullOrEmpty(position)) return;
    if (double.TryParse(position, out var pos))
    {
        // 实现逻辑
    }
}
```

## 🔍 调试技巧

### 1. 检查 DataContext
在 ViewModel 中添加日志：
```csharp
public XxxDebugViewModel(IHardwareController hardwareController)
{
    _logger.Info("XxxDebugViewModel 已创建");
    _hardwareController = hardwareController;
}
```

### 2. 验证绑定
使用 Snoop 或 Visual Studio Live Visual Tree 检查绑定。

### 3. 监控属性变化
在关键属性的 setter 中添加日志：
```csharp
public XxxDto? SelectedDevice
{
    get => _selectedDevice;
    set
    {
        _logger.Debug($"SelectedDevice 正在更改为: {value?.Name}");
        if (SetProperty(ref _selectedDevice, value))
        {
            OnDeviceChanged();
        }
    }
}
```

## 📖 相关文档

- **DEVICE_DEBUG_VIEWMODEL_REFACTOR.md** - 详细架构设计
- **DEVICE_DEBUG_VIEWMODEL_USAGE_GUIDE.md** - 完整使用指南
- **DEVICE_DEBUG_VIEWMODEL_COMPLETION_SUMMARY.md** - 完成总结
- **FINAL_REFACTOR_COMPLETION_REPORT.md** - 最终完成报告

## 🆘 常见问题

### Q: 如何访问其他子 ViewModel？
A: 所有子 ViewModel 都是主 ViewModel 的公共属性，可以直接访问：
```csharp
var motorVM = mainViewModel.MotorDebugVM;
```

### Q: 控件不显示怎么办？
A: 检查：
1. Visibility 绑定路径是否正确
2. RelativeSource 是否指向 UserControl
3. 类型判断属性是否返回正确的值

### Q: 如何在子 ViewModel 间通信？
A: 推荐使用 Prism 的 EventAggregator：
```csharp
_eventAggregator.GetEvent<DeviceStatusChangedEvent>().Publish(status);
```

### Q: 如何清理不需要的代码？
A: 主 ViewModel 中以下代码已不再需要（但暂时保留以确保兼容性）：
- 设备特定的属性（如 MotorPosition, SyringeStatus）
- 设备特定的命令（如 MotorMoveCommand, SyringeInitCommand）
- 设备特定的方法实现

## ✅ 检查清单

在添加新设备或修改现有设备时，请检查：

- [ ] 创建了独立的 ViewModel 文件
- [ ] 实现了 SelectedDevice 属性
- [ ] 实现了 OnDeviceChanged 方法
- [ ] 添加了所有必要的命令
- [ ] 在主 ViewModel 中添加了属性
- [ ] 在构造函数中实例化
- [ ] 在 UpdateDeviceDetails 中添加 case
- [ ] 在 XAML 中添加了控件
- [ ] 更新了 DataContext 绑定
- [ ] 测试了设备切换功能

## 🎉 总结

重构后的系统具有：
- ✅ 清晰的职责划分
- ✅ 更好的可维护性
- ✅ 更容易的测试
- ✅ 更简单的扩展

**祝开发愉快！** 🚀

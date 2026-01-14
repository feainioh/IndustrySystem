# 设备调试 ViewModel 使用指南

## 快速开始

已成功创建以下独立的设备调试 ViewModel：

### ✅ 已完成的 ViewModel

1. **MotorDebugViewModel** - 电机调试（CAN 和 EtherCAT）
2. **SyringePumpDebugViewModel** - 注射泵调试
3. **PeristalticPumpDebugViewModel** - 蠕动泵调试
4. **IODeviceDebugViewModel** - IO 模块调试
5. **TCUDebugViewModel** - TCU 温控调试
6. **RobotDebugViewModel** - 机器人调试
7. **ScannerDebugViewModel** - 扫码枪调试

### 📍 文件位置

所有新 ViewModel 位于：
```
src/Presentation/IndustrySystem.MotionDesigner/ViewModels/DeviceDebug/
```

## 下一步骤

### 1. 集成到主 ViewModel

修改 `DeviceDebugViewModel.cs`：

```csharp
public class DeviceDebugViewModel : BindableBase
{
    // 添加子 ViewModel 属性
    public MotorDebugViewModel MotorDebugVM { get; }
    public SyringePumpDebugViewModel SyringePumpDebugVM { get; }
    public PeristalticPumpDebugViewModel PeristalticPumpDebugVM { get; }
    public IODeviceDebugViewModel IODeviceDebugVM { get; }
    public TCUDebugViewModel TCUDebugVM { get; }
    public RobotDebugViewModel RobotDebugVM { get; }
    public ScannerDebugViewModel ScannerDebugVM { get; }
    
    public DeviceDebugViewModel(
        IDeviceConfigService configService, 
        IHardwareController hardwareController)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        
        // 实例化子 ViewModel
        MotorDebugVM = new MotorDebugViewModel(hardwareController);
        SyringePumpDebugVM = new SyringePumpDebugViewModel(hardwareController);
        PeristalticPumpDebugVM = new PeristalticPumpDebugViewModel(hardwareController);
        IODeviceDebugVM = new IODeviceDebugViewModel(hardwareController);
        TCUDebugVM = new TCUDebugViewModel(hardwareController);
        RobotDebugVM = new RobotDebugViewModel(hardwareController);
        ScannerDebugVM = new ScannerDebugViewModel(hardwareController);
        
        // ... 其他初始化代码
    }
}
```

### 2. 修改设备选择逻辑

在 `UpdateDeviceDetails` 方法中更新设备：

```csharp
private void UpdateDeviceDetails()
{
    if (SelectedDevice == null) return;
    
    switch (SelectedDevice.OriginalDevice)
    {
        case MotorDto motor:
            MotorDebugVM.SelectedMotor = motor;
            MotorDebugVM.SelectedEtherCATMotor = null;
            break;
            
        case EtherCATMotorDto ecatMotor:
            MotorDebugVM.SelectedMotor = null;
            MotorDebugVM.SelectedEtherCATMotor = ecatMotor;
            break;
            
        case SyringePumpDto syringePump:
            SyringePumpDebugVM.SelectedPump = syringePump;
            break;
            
        case PeristalticPumpDto peristalticPump:
            PeristalticPumpDebugVM.SelectedPump = peristalticPump;
            break;
            
        case EcatIODeviceDto ioDevice:
            IODeviceDebugVM.SelectedDevice = ioDevice;
            break;
            
        case TcuDeviceDto tcu:
            TCUDebugVM.SelectedTcu = tcu;
            break;
            
        case JakaRobotDto robot:
            RobotDebugVM.SelectedRobot = robot;
            break;
            
        case ScannerDto scanner:
            ScannerDebugVM.SelectedScanner = scanner;
            break;
    }
}
```

### 3. 更新 XAML 绑定

#### 方式A：在控件 Code-Behind 中设置

**MotorDebugControl.xaml.cs:**
```csharp
public partial class MotorDebugControl : UserControl
{
    public MotorDebugControl()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        
        if (DataContext is DeviceDebugViewModel mainVM)
        {
            // 设置子 ViewModel 为控件的 DataContext
            this.DataContext = mainVM.MotorDebugVM;
        }
    }
}
```

#### 方式B：在 XAML 中直接绑定

**DeviceDebugView.xaml:**
```xaml
<!-- 电机调试控件 -->
<local:MotorDebugControl 
    DataContext="{Binding MotorDebugVM}"
    Visibility="{Binding DataContext.IsAnyMotorSelected, 
                RelativeSource={RelativeSource AncestorType=UserControl}, 
                Converter={StaticResource BooleanToVisibilityConverter}}"/>

<!-- 注射泵调试控件 -->
<local:SyringePumpDebugControl 
    DataContext="{Binding SyringePumpDebugVM}"
    Visibility="{Binding DataContext.IsSyringePumpSelected, 
                RelativeSource={RelativeSource AncestorType=UserControl}, 
                Converter={StaticResource BooleanToVisibilityConverter}}"/>
                
<!-- 其他控件... -->
```

### 4. 清理主 ViewModel

可以安全删除以下内容：

#### 删除的属性（示例）：
```csharp
// 电机相关 - 已移至 MotorDebugViewModel
- MotorPosition
- MotorTargetPosition
- MotorSpeed
- MotorRelative
// ... 等等

// 注射泵相关 - 已移至 SyringePumpDebugViewModel
- SyringeAbsPosition
- SyringeRelStep
// ... 等等
```

#### 删除的命令（示例）：
```csharp
// 电机命令 - 已移至 MotorDebugViewModel
- MotorMoveCommand
- MotorHomeCommand
- MotorStopCommand
// ... 等等

// 注射泵命令 - 已移至 SyringePumpDebugViewModel
- SyringeInitCommand
- SyringeResetCommand
// ... 等等
```

#### 保留的属性：
```csharp
// 这些属性用于 UI 可见性判断，保留在主 ViewModel
public bool IsMotorSelected { get; }
public bool IsEtherCATMotorSelected { get; }
public bool IsAnyMotorSelected { get; }
public bool IsSyringePumpSelected { get; }
public bool IsPeristalticPumpSelected { get; }
// ... 等等
```

## 待创建的 ViewModel

以下设备仍需创建独立的 ViewModel：

- [ ] **DiyPumpDebugViewModel** - 自定义泵调试
- [ ] **CentrifugalDebugViewModel** - 离心机调试
- [ ] **WeightSensorDebugViewModel** - 称重传感器调试
- [ ] **ChillerDebugViewModel** - 冷水机调试

可以参考已创建的 ViewModel 模板进行创建。

## ViewModel 模板

创建新设备 ViewModel 的模板：

```csharp
using System;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class XxxDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    // 设备属性
    private XxxDto? _selectedDevice;
    public XxxDto? SelectedDevice
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
    
    // 状态属性
    private bool _isConnected;
    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }
    
    private string _status = string.Empty;
    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
    
    // 命令
    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    
    public XxxDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        // 初始化命令
        ConnectCommand = new DelegateCommand(async () => await ConnectAsync());
        DisconnectCommand = new DelegateCommand(async () => await DisconnectAsync());
    }
    
    private void OnDeviceChanged()
    {
        // 设备改变时的处理
        if (SelectedDevice != null)
        {
            // 重置状态，应用默认参数等
        }
    }
    
    private async Task ConnectAsync()
    {
        if (SelectedDevice == null) return;
        
        try
        {
            // 连接逻辑
            await Task.Delay(100); // 模拟
            IsConnected = true;
            Status = $"{SelectedDevice.Name} 已连接";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "连接失败");
            Status = $"连接失败: {ex.Message}";
        }
    }
    
    private async Task DisconnectAsync()
    {
        if (SelectedDevice == null) return;
        
        try
        {
            // 断开连接逻辑
            await Task.Delay(50); // 模拟
            IsConnected = false;
            Status = $"{SelectedDevice.Name} 已断开";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "断开失败");
            Status = $"断开失败: {ex.Message}";
        }
    }
}
```

## 测试建议

### 1. 单元测试
为每个 ViewModel 创建单元测试：

```csharp
[Test]
public async Task MotorMove_ShouldUpdatePosition()
{
    // Arrange
    var mockController = new Mock<IHardwareController>();
    var viewModel = new MotorDebugViewModel(mockController.Object);
    viewModel.SelectedMotor = new MotorDto { DeviceId = "motor1", Name = "测试电机" };
    viewModel.MotorTargetPosition = 100;
    
    // Act
    await viewModel.MotorMoveCommand.Execute();
    
    // Assert
    mockController.Verify(x => x.MoveMotorAsync(
        "motor1", 100, It.IsAny<double>(), It.IsAny<bool>(), true), Times.Once);
}
```

### 2. 集成测试
测试设备切换和状态同步：

```csharp
[Test]
public void DeviceSwitch_ShouldUpdateSubViewModel()
{
    // Arrange
    var mainVM = new DeviceDebugViewModel(configService, hardwareController);
    var motor = new MotorDto { DeviceId = "motor1", Name = "电机1" };
    
    // Act
    mainVM.SelectedDevice = new DeviceItemViewModel { OriginalDevice = motor };
    
    // Assert
    Assert.AreEqual(motor, mainVM.MotorDebugVM.SelectedMotor);
}
```

## 调试提示

### 数据绑定问题
如果控件不显示数据：

1. 检查 DataContext 是否正确设置
2. 使用 Snoop 或其他工具检查绑定错误
3. 确认 ViewModel 属性正确触发 PropertyChanged

### 命令不执行
如果按钮点击无反应：

1. 检查命令是否正确绑定
2. 确认命令的 CanExecute 逻辑
3. 检查是否有异常被吞掉

### 性能问题
如果界面卡顿：

1. 确保耗时操作使用异步
2. 检查是否有不必要的属性更新
3. 考虑使用防抖或节流

## 常见问题

### Q: 为什么要拆分 ViewModel？
A: 
- 单一职责原则
- 更好的可维护性
- 更容易测试
- 降低耦合度

### Q: 子 ViewModel 如何通信？
A: 
- 通过主 ViewModel 协调
- 使用事件聚合器（推荐）
- 使用消息总线

### Q: 如何处理共享状态？
A: 
- 状态提升到主 ViewModel
- 使用共享服务
- 使用状态管理模式

## 参考资料

- [MVVM 模式最佳实践](https://docs.microsoft.com/zh-cn/dotnet/architecture/modern-web-apps-azure/develop-asp-net-core-mvc-apps#mvvm-pattern)
- [Prism 框架文档](https://prismlibrary.com/docs/)
- [WPF 数据绑定](https://docs.microsoft.com/zh-cn/dotnet/desktop/wpf/data/)

## 总结

通过将设备调试逻辑抽象到独立的 ViewModel 中：

✅ 代码组织更清晰  
✅ 职责划分更明确  
✅ 可测试性更好  
✅ 可维护性提升  
✅ 易于扩展新设备  

现在可以逐步迁移和测试每个设备的调试功能！

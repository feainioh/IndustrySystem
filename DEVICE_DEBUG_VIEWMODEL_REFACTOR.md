# 设备调试 ViewModel 重构方案

## 概述

将 `DeviceDebugViewModel` 中的硬件设备调试逻辑抽象到独立的 ViewModel 中，遵循单一职责原则，提高代码可维护性和可测试性。

## 架构设计

### 主 ViewModel
**DeviceDebugViewModel** - 主协调者
- 管理设备列表和选择
- 处理配置导入/导出
- 协调各个子 ViewModel

### 子 ViewModel（按设备类型）

1. **MotorDebugViewModel** - 电机调试
   - CAN 电机
   - EtherCAT 电机
   - 共享属性和命令

2. **SyringePumpDebugViewModel** - 注射泵调试

3. **PeristalticPumpDebugViewModel** - 蠕动泵调试

4. **DiyPumpDebugViewModel** - 自定义泵调试

5. **IODeviceDebugViewModel** - IO 模块调试

6. **TCUDebugViewModel** - TCU 温控调试

7. **RobotDebugViewModel** - 机器人调试

8. **ScannerDebugViewModel** - 扫码枪调试

9. **CentrifugalDebugViewModel** - 离心机调试

10. **WeightSensorDebugViewModel** - 称重传感器调试

11. **ChillerDebugViewModel** - 冷水机调试

## 目录结构

```
ViewModels/
├── DeviceDebugViewModel.cs (主 ViewModel)
└── DeviceDebug/
    ├── MotorDebugViewModel.cs
    ├── SyringePumpDebugViewModel.cs
    ├── PeristalticPumpDebugViewModel.cs
    ├── DiyPumpDebugViewModel.cs
    ├── IODeviceDebugViewModel.cs
    ├── TCUDebugViewModel.cs
    ├── RobotDebugViewModel.cs
    ├── ScannerDebugViewModel.cs
    ├── CentrifugalDebugViewModel.cs
    ├── WeightSensorDebugViewModel.cs
    └── ChillerDebugViewModel.cs
```

## ViewModel 职责划分

### 主 ViewModel (DeviceDebugViewModel)

**职责：**
- 导入/导出配置
- 管理设备列表（AllDevices, FilteredDevices, DeviceCategories）
- 设备搜索和过滤
- 设备选择和切换
- 创建和管理子 ViewModel 实例

**属性：**
```csharp
public DeviceConfigDto? CurrentConfig { get; set; }
public string SelectedDeviceType { get; set; }
public DeviceItemViewModel? SelectedDevice { get; set; }
public string StatusMessage { get; set; }
public string SearchText { get; set; }

// 设备列表
public ObservableCollection<DeviceCategoryViewModel> DeviceCategories { get; }
public ObservableCollection<DeviceItemViewModel> AllDevices { get; }
public ObservableCollection<DeviceItemViewModel> FilteredDevices { get; }

// 子 ViewModel
public MotorDebugViewModel MotorDebugVM { get; }
public SyringePumpDebugViewModel SyringePumpDebugVM { get; }
// ... 其他子 ViewModel
```

**命令：**
```csharp
public ICommand ImportConfigCommand { get; }
public ICommand RefreshDeviceStatusCommand { get; }
```

### 子 ViewModel 通用模式

每个子 ViewModel 遵循以下模式：

**构造函数注入依赖：**
```csharp
public class XxxDebugViewModel : BindableBase
{
    private readonly IHardwareController _hardwareController;
    
    public XxxDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        // 初始化命令
    }
}
```

**设备属性和设置：**
```csharp
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

private void OnDeviceChanged()
{
    // 重置状态
    // 应用设备默认参数
}
```

**调试状态和操作：**
```csharp
// 连接状态
public bool IsConnected { get; set; }
// 运行状态
public bool IsRunning { get; set; }
// 状态消息
public string Status { get; set; }

// 操作命令
public ICommand ConnectCommand { get; }
public ICommand DisconnectCommand { get; }
public ICommand StartCommand { get; }
public ICommand StopCommand { get; }
```

## 主 ViewModel 修改

### 1. 添加子 ViewModel 属性

```csharp
public class DeviceDebugViewModel : BindableBase
{
    // 子 ViewModel 实例
    public MotorDebugViewModel MotorDebugVM { get; }
    public SyringePumpDebugViewModel SyringePumpDebugVM { get; }
    public PeristalticPumpDebugViewModel PeristalticPumpDebugVM { get; }
    public IODeviceDebugViewModel IODeviceDebugVM { get; }
    public TCUDebugViewModel TCUDebugVM { get; }
    public RobotDebugViewModel RobotDebugVM { get; }
    public ScannerDebugViewModel ScannerDebugVM { get; }
    // ... 其他子 ViewModel
    
    public DeviceDebugViewModel(
        IDeviceConfigService configService, 
        IHardwareController hardwareController)
    {
        _configService = configService;
        _hardwareController = hardwareController;
        
        // 创建子 ViewModel
        MotorDebugVM = new MotorDebugViewModel(hardwareController);
        SyringePumpDebugVM = new SyringePumpDebugViewModel(hardwareController);
        PeristalticPumpDebugVM = new PeristalticPumpDebugViewModel(hardwareController);
        IODeviceDebugVM = new IODeviceDebugViewModel(hardwareController);
        TCUDebugVM = new TCUDebugViewModel(hardwareController);
        RobotDebugVM = new RobotDebugViewModel(hardwareController);
        ScannerDebugVM = new ScannerDebugViewModel(hardwareController);
        // ... 其他子 ViewModel
        
        // 初始化命令
        ImportConfigCommand = new DelegateCommand(async () => await ImportConfigAsync());
        RefreshDeviceStatusCommand = new DelegateCommand(async () => await RefreshDeviceStatusAsync());
    }
}
```

### 2. 修改 UpdateDeviceDetails 方法

```csharp
private void UpdateDeviceDetails()
{
    if (SelectedDevice == null) return;
    
    // 根据设备类型更新对应的子 ViewModel
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
            
        // ... 其他设备类型
    }
}
```

### 3. 移除原有设备调试代码

从主 ViewModel 中移除以下内容：
- 设备特定的属性（保留设备类型判断属性）
- 设备特定的命令
- 设备特定的方法实现

保留：
```csharp
// 设备类型判断属性（用于 UI 可见性绑定）
public bool IsMotorSelected => SelectedDevice?.OriginalDevice is MotorDto;
public bool IsEtherCATMotorSelected => SelectedDevice?.OriginalDevice is EtherCATMotorDto;
public bool IsAnyMotorSelected => IsMotorSelected || IsEtherCATMotorSelected;
// ... 其他类型判断
```

## XAML 绑定修改

### 控件 DataContext 设置

每个调试控件的 code-behind 应设置其 DataContext：

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
            // 将子 ViewModel 设置为控件的 DataContext
            this.DataContext = mainVM.MotorDebugVM;
        }
    }
}
```

或者在 DeviceDebugView.xaml 中直接绑定：

```xaml
<local:MotorDebugControl 
    DataContext="{Binding MotorDebugVM}"
    Visibility="{Binding DataContext.IsAnyMotorSelected, 
                RelativeSource={RelativeSource AncestorType=UserControl}, 
                Converter={StaticResource BooleanToVisibilityConverter}}"/>
```

## 优势

### 1. 单一职责
- 每个 ViewModel 只负责一种设备的调试
- 主 ViewModel 只负责协调和导航

### 2. 更好的可维护性
- 代码组织清晰
- 易于定位和修改特定设备的逻辑

### 3. 更好的可测试性
- 可以独立测试每个设备的 ViewModel
- 减少测试的复杂度

### 4. 更好的可扩展性
- 添加新设备类型只需创建新的 ViewModel
- 不影响现有代码

### 5. 降低耦合
- 设备间的调试逻辑完全隔离
- 便于代码重用

## 迁移步骤

### 第一阶段：创建子 ViewModel（已完成）
- [x] MotorDebugViewModel
- [x] SyringePumpDebugViewModel
- [x] PeristalticPumpDebugViewModel
- [x] IODeviceDebugViewModel
- [x] TCUDebugViewModel
- [x] RobotDebugViewModel
- [x] ScannerDebugViewModel
- [ ] DiyPumpDebugViewModel
- [ ] CentrifugalDebugViewModel
- [ ] WeightSensorDebugViewModel
- [ ] ChillerDebugViewModel

### 第二阶段：修改主 ViewModel
1. 添加子 ViewModel 属性
2. 在构造函数中实例化子 ViewModel
3. 修改 UpdateDeviceDetails 方法
4. 移除设备特定的代码

### 第三阶段：修改 XAML 绑定
1. 更新控件的 DataContext 绑定
2. 调整 Visibility 绑定路径
3. 测试每个设备的调试功能

### 第四阶段：测试和优化
1. 单元测试每个子 ViewModel
2. 集成测试设备切换
3. 性能优化

## 注意事项

1. **依赖注入：** 确保 `IHardwareController` 正确注入到各个子 ViewModel

2. **数据绑定：** 注意 RelativeSource 和 DataContext 的正确设置

3. **状态同步：** 设备切换时确保状态正确重置

4. **内存管理：** 子 ViewModel 生命周期与主 ViewModel 一致，注意资源释放

5. **向后兼容：** 保留主 ViewModel 中的类型判断属性以支持现有 UI

## 最佳实践

1. **命名规范：** 子 ViewModel 使用 `XxxDebugViewModel` 模式

2. **属性命名：** 使用设备类型前缀，如 `MotorPosition`, `SyringeStatus`

3. **命令命名：** 使用设备类型前缀 + 动作，如 `MotorMoveCommand`, `TcuStartCommand`

4. **状态消息：** 统一使用 `Status` 或 `XxxStatus` 属性

5. **错误处理：** 在每个命令中使用 try-catch 并记录日志

6. **异步操作：** 所有硬件操作使用异步方法

## 后续扩展

1. **添加事件总线：** 用于设备间的通信

2. **添加状态机：** 管理复杂的设备状态转换

3. **添加历史记录：** 记录设备操作历史

4. **添加配置保存：** 保存设备调试参数

5. **添加批量操作：** 支持多设备同时操作

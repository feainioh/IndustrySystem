# 设备调试与位置设置双向同步功能 - 使用指南

## ✅ 实现的功能

### 1. 双向配置同步
- ✅ **DeviceDebugView → PositionSettingsView**  
  在调试界面导入配置后，位置设置界面自动同步更新
  
- ✅ **PositionSettingsView → DeviceDebugView**  
  在位置设置界面修改/添加/删除位置后，调试界面自动同步更新

### 2. 位置管理功能
- ✅ **添加新位置**  
  在 PositionSettingsView 中可以添加新的工作位置点
  
- ✅ **删除位置**  
  支持删除已有的位置点
  
- ✅ **修改位置**  
  修改位置参数后自动标记为已修改，保存时同步到所有界面

## 📋 功能详情

### 一、双向同步实现

#### 1. 配置导入同步
**触发场景：** 在 DeviceDebugView 中导入配置文件

**同步流程：**
```
用户在 DeviceDebugView 点击"导入配置"
    ↓
DeviceDebugViewModel.ImportConfigAsync()
    ↓
发布 DeviceConfigImportedEvent 事件
    ↓
PositionSettingsViewModel 接收事件
    ↓
自动加载所有设备的位置点到列表
```

**代码位置：**
- 发布事件：`DeviceDebugViewModel.cs` 第 1063 行
- 订阅事件：`PositionSettingsViewModel.cs` 第 115 行
- 处理事件：`PositionSettingsViewModel.OnConfigImported()` 第 646 行

#### 2. 位置修改同步
**触发场景：** 在 PositionSettingsView 中修改位置并保存

**同步流程：**
```
用户修改位置参数（位置值/速度）
    ↓
标记为 IsModified = true
    ↓
用户点击"保存配置"
    ↓
PositionSettingsViewModel.SaveConfigAsync()
    ↓
发布 PositionUpdatedEvent 事件（每个修改的位置）
    ↓
DeviceDebugViewModel 接收事件
    ↓
更新对应设备的工作位置
```

**代码位置：**
- 发布事件：`PositionSettingsViewModel.SaveConfigAsync()` 第 286、308、330 行
- 订阅事件：`DeviceDebugViewModel.cs` 第 812 行
- 处理事件：`DeviceDebugViewModel.OnPositionUpdated()` 第 2858 行

#### 3. 位置添加同步
**触发场景：** 在 PositionSettingsView 中添加新位置

**同步流程：**
```
用户点击"添加位置"按钮
    ↓
PositionSettingsViewModel.AddPosition()
    ↓
创建新的 PositionPointViewModel
    ↓
添加到 AllPositions 列表
    ↓
同时添加到 CurrentConfig 对应设备
    ↓
发布 PositionAddedEvent 事件
    ↓
DeviceDebugViewModel 接收事件
    ↓
添加到对应设备的 WorkPositions
```

**代码位置：**
- 发布事件：`PositionSettingsViewModel.AddPosition()` 第 513 行
- 订阅事件：`DeviceDebugViewModel.cs` 第 813 行
- 处理事件：`DeviceDebugViewModel.OnPositionAdded()` 第 2906 行

#### 4. 位置删除同步
**触发场景：** 在 PositionSettingsView 中删除位置

**同步流程：**
```
用户选择位置点，点击"删除"
    ↓
显示确认对话框
    ↓
用户确认
    ↓
PositionSettingsViewModel.DeletePosition()
    ↓
从 CurrentConfig 中删除
    ↓
从 AllPositions 列表删除
    ↓
发布 PositionDeletedEvent 事件
    ↓
DeviceDebugViewModel 接收事件
    ↓
从对应设备的 WorkPositions 中删除
```

**代码位置：**
- 发布事件：`PositionSettingsViewModel.DeletePosition()` 第 598 行
- 订阅事件：`DeviceDebugViewModel.cs` 第 814 行
- 处理事件：`DeviceDebugViewModel.OnPositionDeleted()` 第 2957 行

### 二、支持的设备类型

同步功能支持以下设备类型：

1. **CAN 电机** (`MotorDto`)
   - 设备列表：`CurrentConfig.Motors`
   - 工作位置：`motor.WorkPositions`

2. **EtherCAT 电机** (`EtherCATMotorDto`)
   - 设备列表：`CurrentConfig.EtherCATMotors`
   - 工作位置：`motor.WorkPositions`

3. **离心机** (`CentrifugalDeviceDto`)
   - 设备列表：`CurrentConfig.CentrifugalDevices`
   - 工作位置：`centrifugal.WorkPositions`

### 三、事件系统架构

#### 事件定义
文件：`src/Presentation/IndustrySystem.MotionDesigner/Events/DeviceConfigEvents.cs`

```csharp
// 配置导入事件
public class DeviceConfigImportedEvent : PubSubEvent<DeviceConfigDto>

// 位置更新事件
public class PositionUpdatedEvent : PubSubEvent<PositionUpdatedEventArgs>

// 位置添加事件
public class PositionAddedEvent : PubSubEvent<PositionPointViewModel>

// 位置删除事件
public class PositionDeletedEvent : PubSubEvent<PositionPointViewModel>
```

#### 事件参数

**PositionUpdatedEventArgs：**
```csharp
{
    string DeviceId;        // 设备 ID
    string PositionName;    // 位置名称
    double Position;        // 位置值
    double Speed;           // 速度值
}
```

**PositionPointViewModel：**
```csharp
{
    string DeviceId;        // 设备 ID
    string DeviceName;      // 设备名称
    string DeviceType;      // 设备类型
    string PositionName;    // 位置名称
    double Position;        // 位置值
    double Speed;           // 速度值
    bool IsModified;        // 是否已修改
}
```

## 🎯 使用场景

### 场景 1：配置导入同步

**步骤：**
1. 在 DeviceDebugView 中点击"导入配置"
2. 选择配置文件 (*.json)
3. 配置自动加载到调试界面
4. 同时自动同步到 PositionSettingsView
5. 切换到位置设置界面，查看所有设备的位置点

**预期结果：**
- 调试界面显示所有设备
- 位置设置界面显示所有设备的工作位置
- 统计信息自动更新

### 场景 2：在位置设置界面添加新位置

**步骤：**
1. 切换到 PositionSettingsView
2. 点击"添加位置"按钮
3. 系统自动创建一个新位置点（使用第一个设备）
4. 修改位置名称、位置值、速度
5. 标记为 IsModified = true
6. 点击"保存配置"
7. 切换回 DeviceDebugView

**预期结果：**
- 新位置已添加到列表
- 调试界面对应设备的工作位置列表中出现新位置
- 可以在调试界面使用该新位置

### 场景 3：修改位置参数并同步

**步骤：**
1. 在 PositionSettingsView 选择一个位置点
2. 修改位置值或速度
3. 位置自动标记为已修改
4. 点击"保存配置"
5. 切换到 DeviceDebugView
6. 选择对应的设备

**预期结果：**
- 修改已保存
- 调试界面显示更新后的位置参数
- 使用该位置进行运动时使用新参数

### 场景 4：删除位置并同步

**步骤：**
1. 在 PositionSettingsView 选择要删除的位置
2. 点击"删除位置"按钮
3. 确认删除
4. 切换到 DeviceDebugView

**预期结果：**
- 位置从列表中移除
- 调试界面对应设备的工作位置列表中不再显示该位置
- 统计信息更新

## ⚙️ 技术实现细节

### 依赖注入

两个 ViewModel 都需要注入 `IEventAggregator`：

```csharp
// DeviceDebugViewModel 构造函数
public DeviceDebugViewModel(
    IDeviceConfigService configService, 
    IHardwareController hardwareController, 
    IEventAggregator eventAggregator)

// PositionSettingsViewModel 构造函数
public PositionSettingsViewModel(
    IDeviceConfigService configService, 
    IHardwareController hardwareController, 
    IEventAggregator eventAggregator)
```

### 事件订阅

**DeviceDebugViewModel：**
```csharp
// 订阅位置更新事件
_eventAggregator.GetEvent<PositionUpdatedEvent>()
    .Subscribe(OnPositionUpdated, ThreadOption.UIThread);

// 订阅位置添加事件
_eventAggregator.GetEvent<PositionAddedEvent>()
    .Subscribe(OnPositionAdded, ThreadOption.UIThread);

// 订阅位置删除事件
_eventAggregator.GetEvent<PositionDeletedEvent>()
    .Subscribe(OnPositionDeleted, ThreadOption.UIThread);
```

**PositionSettingsViewModel：**
```csharp
// 订阅配置导入事件
_eventAggregator.GetEvent<DeviceConfigImportedEvent>()
    .Subscribe(OnConfigImported, ThreadOption.UIThread);
```

### 事件发布

**配置导入：**
```csharp
_eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
```

**位置更新：**
```csharp
_eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
{
    DeviceId = pos.DeviceId,
    PositionName = pos.PositionName,
    Position = pos.Position,
    Speed = pos.Speed
});
```

**位置添加：**
```csharp
_eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);
```

**位置删除：**
```csharp
_eventAggregator.GetEvent<PositionDeletedEvent>().Publish(posToDelete);
```

## 🔍 调试技巧

### 1. 检查事件是否触发
在事件处理方法中添加断点：
- `DeviceDebugViewModel.OnPositionUpdated()`
- `DeviceDebugViewModel.OnPositionAdded()`
- `DeviceDebugViewModel.OnPositionDeleted()`
- `PositionSettingsViewModel.OnConfigImported()`

### 2. 查看状态消息
两个界面都有状态栏显示操作结果：
- `DeviceDebugViewModel.StatusMessage`
- `PositionSettingsViewModel.StatusMessage`

### 3. 验证配置同步
检查 `CurrentConfig` 对象是否在两个 ViewModel 中保持一致：
```csharp
// 两个 ViewModel 应该引用同一个配置对象
DeviceDebugViewModel.CurrentConfig == PositionSettingsViewModel.CurrentConfig
```

## ⚠️ 注意事项

### 1. 线程安全
所有事件都在 UI 线程上处理：
```csharp
.Subscribe(handler, ThreadOption.UIThread)
```

### 2. 命名冲突解决
使用别名避免 `PositionPointViewModel` 的命名冲突：
```csharp
using PositionPointViewModel = IndustrySystem.MotionDesigner.Services.PositionPointViewModel;
```

### 3. 保存顺序
修改位置后必须保存才能同步到其他界面：
1. 修改位置参数
2. 位置标记为 IsModified
3. 点击"保存配置"
4. 事件触发，同步到其他界面

### 4. 配置一致性
两个界面共享同一个 `DeviceConfigDto` 实例，确保数据一致性。

## 📝 扩展建议

### 1. 添加更多设备类型支持
如需支持新的设备类型（如机器人位置），只需：
1. 在对应的事件处理方法中添加 case
2. 处理该设备类型的位置同步

示例：
```csharp
// 在 OnPositionUpdated 中
var robot = CurrentConfig.JakaRobots.FirstOrDefault(r => r.DeviceId == args.DeviceId);
if (robot != null)
{
    var pos = robot.WorkPositions.FirstOrDefault(p => p.Name == args.PositionName);
    if (pos != null)
    {
        pos.Position = args.Position;
        pos.Speed = args.Speed;
    }
}
```

### 2. 实时位置示教同步
在调试界面示教位置时，也可以发布事件同步到位置设置界面：

```csharp
// 在电机示教完成后
_eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
{
    DeviceId = motor.DeviceId,
    PositionName = workPosition.Name,
    Position = currentPosition,
    Speed = workPosition.Speed
});
```

### 3. 批量操作支持
可以添加批量导入/导出位置的功能，一次性同步多个位置。

## 🎓 总结

✅ **已实现功能：**
- 配置导入双向同步
- 位置修改双向同步
- 位置添加双向同步
- 位置删除双向同步
- 支持 CAN电机、EtherCAT电机、离心机

✅ **技术特点：**
- 使用 Prism EventAggregator 解耦
- UI 线程安全
- 实时同步
- 代码结构清晰

✅ **用户体验：**
- 无需手动刷新
- 自动同步更新
- 状态提示清晰
- 操作简单直观

**现在你可以在两个界面之间无缝切换，所有位置数据都会自动保持同步！** 🎉

---

**文档创建时间：** 2024
**功能实现人员：** GitHub Copilot
**项目名称：** IndustrySystem.MotionDesigner

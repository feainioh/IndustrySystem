# 跨视图配置同步 - 实现代码

## DeviceDebugViewModel 添加创建配置功能

在 DeviceDebugViewModel.cs 的命令定义部分添加：

```csharp
public ICommand CreateConfigCommand { get; }
```

在构造函数中初始化：

```csharp
CreateConfigCommand = new DelegateCommand(CreateConfig);
```

添加创建配置方法：

```csharp
/// <summary>
/// Create new hardware configuration
/// </summary>
private void CreateConfig()
{
    try
    {
        var config = new DeviceConfigDto
        {
            Name = "New Configuration",
            Description = $"Created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            Motors = new List<MotorDto>(),
            EtherCATMotors = new List<EtherCATMotorDto>(),
            SyringePumps = new List<SyringePumpDto>(),
            PeristalticPumps = new List<PeristalticPumpDto>(),
            DiyPumps = new List<DiyPumpDto>(),
            CentrifugalDevices = new List<CentrifugalDeviceDto>(),
            JakaRobots = new List<JakaRobotDto>(),
            TcuDevices = new List<TcuDeviceDto>(),
            ChillerDevices = new List<ChillerDto>(),
            WeighingSensors = new List<WeighingSensorDto>(),
            TwoChannelValves = new List<TwoChannelValveDto>(),
            ThreeChannelValves = new List<ThreeChannelValveDto>(),
            EcatIODevices = new List<EcatIODeviceDto>(),
            CustomModbusDevices = new List<CustomModbusDeviceDto>(),
            ScannerDevices = new List<ScannerDto>()
        };
        
        CurrentConfig = config;
        
        // Clear device lists
        AllDevices.Clear();
        DeviceCategories.Clear();
        
        // Publish events
        _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Publish(config);
        _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
        {
            Config = config,
            FilePath = string.Empty,
            Source = "DeviceDebugView-Create"
        });
        
        StatusMessage = "新配置已创建，可以开始添加设备";
        _logger.Info("New configuration created");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Failed to create configuration");
        StatusMessage = $"创建配置失败: {ex.Message}";
    }
}
```

更新 ImportConfig 方法发布事件：

```csharp
private void ImportConfig()
{
    var dialog = new OpenFileDialog
    {
        Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
        Title = "Import Hardware Configuration"
    };
    
    if (dialog.ShowDialog() == true)
    {
        try
        {
            var config = _configService.LoadConfig(dialog.FileName);
            CurrentConfig = config;
            
            LoadDevices(config);
            
            // Publish events
            _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
            _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
            {
                Config = config,
                FilePath = dialog.FileName,
                Source = "DeviceDebugView-Import"
            });
            
            StatusMessage = $"配置已加载: {config.Name}";
            _logger.Info($"Configuration loaded: {config.Name}");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load configuration");
            StatusMessage = $"加载配置失败: {ex.Message}";
        }
    }
}
```

添加位置同步事件订阅（如果还没有）：

```csharp
// 在构造函数中添加
_eventAggregator.GetEvent<PositionUpdatedEvent>().Subscribe(OnPositionUpdated);
_eventAggregator.GetEvent<PositionAddedEvent>().Subscribe(OnPositionAdded);
_eventAggregator.GetEvent<PositionDeletedEvent>().Subscribe(OnPositionDeleted);
```

添加事件处理方法：

```csharp
/// <summary>
/// Handle position updated event
/// </summary>
private void OnPositionUpdated(PositionUpdatedEventArgs args)
{
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        _logger.Info($"Received position updated: {args.DeviceId}/{args.PositionName}");
        
        // Update the position in current config
        UpdatePositionInConfig(args.DeviceId, args.PositionName, args.Position, args.Speed);
        
        // Refresh device positions if the device is currently selected
        if (SelectedDevice?.DeviceId == args.DeviceId)
        {
            RefreshDevicePositions();
        }
    });
}

/// <summary>
/// Handle position added event
/// </summary>
private void OnPositionAdded(PositionPointViewModel position)
{
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        _logger.Info($"Received position added: {position.DeviceName}/{position.PositionName}");
        
        // Add position to config
        AddPositionToConfig(position);
        
        // Refresh device positions if the device is currently selected
        if (SelectedDevice?.DeviceId == position.DeviceId)
        {
            RefreshDevicePositions();
        }
    });
}

/// <summary>
/// Handle position deleted event
/// </summary>
private void OnPositionDeleted(PositionPointViewModel position)
{
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        _logger.Info($"Received position deleted: {position.DeviceName}/{position.PositionName}");
        
        // Remove position from config
        RemovePositionFromConfig(position.DeviceId, position.PositionName);
        
        // Refresh device positions if the device is currently selected
        if (SelectedDevice?.DeviceId == position.DeviceId)
        {
            RefreshDevicePositions();
        }
    });
}

/// <summary>
/// Update position in configuration
/// </summary>
private void UpdatePositionInConfig(string deviceId, string positionName, double position, double speed)
{
    if (CurrentConfig == null) return;
    
    // Update in motors
    var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == deviceId);
    if (motor != null)
    {
        var pos = motor.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            pos.Position = position;
            pos.Speed = speed;
        }
        return;
    }
    
    // Update in EtherCAT motors
    var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == deviceId);
    if (ecatMotor != null)
    {
        var pos = ecatMotor.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            pos.Position = position;
            pos.Speed = speed;
        }
        return;
    }
    
    // Update in centrifugal devices
    var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == deviceId);
    if (cent != null)
    {
        var pos = cent.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            pos.Position = position;
            pos.Speed = speed;
        }
    }
}

/// <summary>
/// Add position to configuration
/// </summary>
private void AddPositionToConfig(PositionPointViewModel position)
{
    if (CurrentConfig == null) return;
    
    var workPos = new WorkPositionDto
    {
        Name = position.PositionName,
        Position = position.Position,
        Speed = position.Speed
    };
    
    // Add to motors
    var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
    if (motor != null)
    {
        motor.WorkPositions.Add(workPos);
        return;
    }
    
    // Add to EtherCAT motors
    var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == position.DeviceId);
    if (ecatMotor != null)
    {
        ecatMotor.WorkPositions.Add(workPos);
        return;
    }
    
    // Add to centrifugal devices
    var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == position.DeviceId);
    if (cent != null)
    {
        cent.WorkPositions.Add(workPos);
    }
}

/// <summary>
/// Remove position from configuration
/// </summary>
private void RemovePositionFromConfig(string deviceId, string positionName)
{
    if (CurrentConfig == null) return;
    
    // Remove from motors
    var motor = CurrentConfig.Motors.FirstOrDefault(m => m.DeviceId == deviceId);
    if (motor != null)
    {
        var pos = motor.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            motor.WorkPositions.Remove(pos);
        }
        return;
    }
    
    // Remove from EtherCAT motors
    var ecatMotor = CurrentConfig.EtherCATMotors.FirstOrDefault(m => m.DeviceId == deviceId);
    if (ecatMotor != null)
    {
        var pos = ecatMotor.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            ecatMotor.WorkPositions.Remove(pos);
        }
        return;
    }
    
    // Remove from centrifugal devices
    var cent = CurrentConfig.CentrifugalDevices.FirstOrDefault(c => c.DeviceId == deviceId);
    if (cent != null)
    {
        var pos = cent.WorkPositions.FirstOrDefault(p => p.Name == positionName);
        if (pos != null)
        {
            cent.WorkPositions.Remove(pos);
        }
    }
}

/// <summary>
/// Refresh device positions
/// </summary>
private void RefreshDevicePositions()
{
    // Trigger property changed for motor work positions
    RaisePropertyChanged(nameof(MotorWorkPositions));
    
    // Refresh motor debug VM if exists
    if (MotorDebugVM != null)
    {
        MotorDebugVM.RefreshPositions();
    }
}
```

## PositionSettingsViewModel 确保事件发布

确保所有位置操作都发布事件。在 PositionSettingsViewModel.cs 中：

```csharp
// 确保 AddPosition 方法发布事件（已有）
_eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);

// 确保 DeletePosition 方法发布事件（已有）
_eventAggregator.GetEvent<PositionDeletedEvent>().Publish(SelectedPosition);

// 添加配置加载事件订阅
_eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Subscribe(OnConfigLoaded);

// 添加事件处理方法
private void OnConfigLoaded(ConfigLoadedEventArgs args)
{
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        _logger.Info($"Received config loaded event from {args.Source}: {args.Config.Name}");
        
        // Reload positions from new configuration
        CurrentConfig = args.Config;
        LoadPositionsFromConfig(args.Config);
        
        StatusMessage = $"已加载配置: {args.Config.Name}";
    });
}

private void LoadPositionsFromConfig(DeviceConfigDto config)
{
    AllPositions.Clear();
    
    // Load from motors
    foreach (var motor in config.Motors)
    {
        foreach (var pos in motor.WorkPositions)
        {
            AllPositions.Add(new PositionPointViewModel
            {
                DeviceId = motor.DeviceId,
                DeviceName = motor.Name,
                DeviceType = "CAN电机",
                PositionName = pos.Name,
                Position = pos.Position,
                Speed = pos.Speed,
                OriginalPosition = pos.Position,
                OriginalSpeed = pos.Speed,
                IsModified = false
            });
        }
    }
    
    // Load from EtherCAT motors
    foreach (var motor in config.EtherCATMotors)
    {
        foreach (var pos in motor.WorkPositions)
        {
            AllPositions.Add(new PositionPointViewModel
            {
                DeviceId = motor.DeviceId,
                DeviceName = motor.Name,
                DeviceType = "EtherCAT电机",
                PositionName = pos.Name,
                Position = pos.Position,
                Speed = pos.Speed,
                OriginalPosition = pos.Position,
                OriginalSpeed = pos.Speed,
                IsModified = false
            });
        }
    }
    
    // Load from centrifugal devices
    foreach (var device in config.CentrifugalDevices)
    {
        foreach (var pos in device.WorkPositions)
        {
            AllPositions.Add(new PositionPointViewModel
            {
                DeviceId = device.DeviceId,
                DeviceName = device.Name,
                DeviceType = "离心机",
                PositionName = pos.Name,
                Position = pos.Position,
                Speed = pos.Speed,
                OriginalPosition = pos.Position,
                OriginalSpeed = pos.Speed,
                IsModified = false
            });
        }
    }
    
    FilterPositions();
    UpdateStatistics();
}
```

## App.xaml.cs 依赖注入配置

确保在 App.xaml.cs 中正确配置依赖注入：

```csharp
protected override void RegisterTypes(IContainerRegistry containerRegistry)
{
    // ... 其他注册
    
    // Register IDeviceConfigService if not already registered
    containerRegistry.RegisterSingleton<IDeviceConfigService, DeviceConfigService>();
    
    // ViewModels
    containerRegistry.Register<DesignerViewModel>();
    containerRegistry.Register<DeviceDebugViewModel>();
    containerRegistry.Register<PositionSettingsViewModel>();
    
    // ... 其他注册
}
```

## 测试步骤

### 1. 测试配置导入同步

**步骤：**
1. 打开应用
2. 切换到 DesignerView
3. 点击"Import Config"按钮
4. 选择配置文件
5. **验证：**
   - DeviceDebugView 的设备列表自动更新
   - PositionSettingsView 的位置列表自动更新
   - 状态栏显示配置名称

### 2. 测试新建配置同步

**步骤：**
1. 切换到 DeviceDebugView
2. 点击"新建配置"按钮
3. **验证：**
   - DesignerView 清空或初始化
   - PositionSettingsView 清空位置列表
   - 状态消息显示"新配置已创建"

### 3. 测试位置修改同步

**步骤：**
1. 在 PositionSettingsView 中选择一个位置
2. 修改位置值或速度
3. **验证：**
   - DeviceDebugView 中该设备的位置参数更新
   - 如果设备被选中，位置下拉列表立即刷新
   - DesignerView 中使用该位置的节点参数更新（如果有）

### 4. 测试位置添加同步

**步骤：**
1. 在 PositionSettingsView 点击"添加位置"
2. 填写信息并添加
3. **验证：**
   - DeviceDebugView 的位置列表新增该位置
   - DesignerView 的位置选项新增该位置
   - 配置对象中对应设备的位置列表已更新

### 5. 测试位置删除同步

**步骤：**
1. 在 PositionSettingsView 选择位置并删除
2. **验证：**
   - DeviceDebugView 位置列表移除该位置
   - DesignerView 中使用该位置的节点显示警告（如果有）
   - 配置对象中已删除该位置

## 调试技巧

### 启用详细日志

在 NLog.config 中设置日志级别为 Debug：

```xml
<logger name="*" minlevel="Debug" writeTo="file" />
```

### 检查事件发布

在每个事件发布处添加日志：

```csharp
_logger.Debug($"Publishing DeviceConfigImportedEvent: {config.Name}");
_eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
```

### 检查事件订阅

在每个事件处理方法开始添加日志：

```csharp
private void OnConfigImported(DeviceConfigDto config)
{
    _logger.Debug($"Handling DeviceConfigImportedEvent: {config.Name}");
    // ... 处理逻辑
}
```

### 验证线程

确认事件处理在 UI 线程：

```csharp
private void OnConfigImported(DeviceConfigDto config)
{
    _logger.Debug($"Thread ID: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
    System.Windows.Application.Current.Dispatcher.Invoke(() =>
    {
        // UI 更新
    });
}
```

## 常见问题

### 问题 1：事件未触发

**原因：** EventAggregator 未正确注入

**解决：** 检查 App.xaml.cs 中的依赖注入配置

### 问题 2：UI 未更新

**原因：** 事件处理在后台线程

**解决：** 使用 Dispatcher.Invoke 包装 UI 更新代码

### 问题 3：配置对象不同步

**原因：** 多个 ViewModel 持有不同的配置实例

**解决：** 使用单例模式或通过事件传递最新配置

### 问题 4：内存泄漏

**原因：** 事件订阅未取消

**解决：** 在 ViewModel 实现 IDisposable 并取消订阅：

```csharp
public void Dispose()
{
    _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Unsubscribe(OnConfigImported);
    // ... 取消其他订阅
}
```

## 性能优化建议

### 1. 批量更新

避免频繁的单个位置更新，使用批量更新：

```csharp
public class PositionsBatchUpdatedEvent : PubSubEvent<List<PositionPointViewModel>> { }
```

### 2. 防抖动

对频繁的更新操作使用防抖动：

```csharp
private System.Timers.Timer _updateTimer;

private void OnPositionChanged()
{
    _updateTimer?.Stop();
    _updateTimer = new System.Timers.Timer(500);
    _updateTimer.Elapsed += (s, e) =>
    {
        _updateTimer.Stop();
        PublishUpdate();
    };
    _updateTimer.Start();
}
```

### 3. 异步处理

对于耗时操作使用异步事件处理：

```csharp
_eventAggregator.GetEvent<DeviceConfigLoadedEvent>()
    .Subscribe(OnConfigLoaded, ThreadOption.BackgroundThread);
```

---

**实现完成后，系统将实现完整的跨视图数据同步！**

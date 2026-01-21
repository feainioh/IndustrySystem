# 跨视图配置同步实现方案

## 📋 需求分析

### 1. DesignerView 增加硬件配置文件导入
- 添加"导入配置"按钮
- 导入后同步到其他视图

### 2. DeviceDebugView 增加新增配置文件功能
- 添加"新建配置"按钮
- 新建后同步到其他视图

### 3. 配置文件同步（DeviceDebugView → 其他视图）
- 加载配置时同步
- 新增配置时同步

### 4. 位置信息同步（PositionSettingsView → 其他视图）
- 修改位置时同步
- 新增位置时同步
- 删除位置时同步

## 🎯 实现方案

### 架构设计

```
┌─────────────────┐      ┌──────────────────┐      ┌─────────────────────┐
│ DesignerView    │      │ DeviceDebugView  │      │ PositionSettings    │
│   ViewModel     │      │   ViewModel      │      │   ViewModel         │
└────────┬────────┘      └────────┬─────────┘      └──────────┬──────────┘
         │                        │                             │
         │                        │                             │
         ▼                        ▼                             ▼
    ┌────────────────────────────────────────────────────────────────┐
    │                    Event Aggregator (Prism)                    │
    │  ┌──────────────────────────────────────────────────────────┐ │
    │  │  • DeviceConfigImportedEvent                             │ │
    │  │  • DeviceConfigCreatedEvent                              │ │
    │  │  • DeviceConfigLoadedEvent                               │ │
    │  │  • PositionUpdatedEvent                                  │ │
    │  │  • PositionAddedEvent                                    │ │
    │  │  • PositionDeletedEvent                                  │ │
    │  │  • DeviceAddedEvent                                      │ │
    │  └──────────────────────────────────────────────────────────┘ │
    └────────────────────────────────────────────────────────────────┘
```

### 事件流程

#### 1. 配置导入流程（DesignerView）

```
用户点击"导入配置"
    ↓
DesignerViewModel.ImportConfigCommand
    ↓
选择配置文件
    ↓
加载配置到内存
    ↓
发布 DeviceConfigImportedEvent
    ↓
DeviceDebugViewModel 接收 → 更新设备列表
PositionSettingsViewModel 接收 → 更新位置列表
```

#### 2. 配置新建流程（DeviceDebugView）

```
用户点击"新建配置"
    ↓
DeviceDebugViewModel.CreateConfigCommand
    ↓
创建空配置
    ↓
发布 DeviceConfigCreatedEvent
    ↓
DesignerViewModel 接收 → 初始化设计器
PositionSettingsViewModel 接收 → 清空位置列表
```

#### 3. 位置修改流程（PositionSettingsView）

```
用户修改位置参数
    ↓
PositionSettingsViewModel
    ↓
更新位置数据
    ↓
发布 PositionUpdatedEvent
    ↓
DeviceDebugViewModel 接收 → 更新对应设备的位置下拉列表
DesignerViewModel 接收 → 更新动作节点中的位置选项
```

#### 4. 位置添加流程（PositionSettingsView）

```
用户添加新位置
    ↓
PositionSettingsViewModel.AddPosition
    ↓
添加到配置
    ↓
发布 PositionAddedEvent
    ↓
DeviceDebugViewModel 接收 → 刷新位置下拉列表
DesignerViewModel 接收 → 刷新位置选项
```

## 📝 详细实现

### 1. 修改 DesignerViewModel

```csharp
using Prism.Events;
using IndustrySystem.MotionDesigner.Events;
using IndustrySystem.MotionDesigner.Services;

public class DesignerViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IDeviceConfigService _configService;
    private DeviceConfigDto? _currentConfig;
    
    public ICommand ImportConfigCommand { get; }
    
    public DesignerViewModel(
        IMotionProgramAppService programService,
        IMotionProgramExecutor executor,
        IEventAggregator eventAggregator,
        IDeviceConfigService configService)
    {
        _programService = programService;
        _executor = executor;
        _eventAggregator = eventAggregator;
        _configService = configService;
        
        ImportConfigCommand = new DelegateCommand(ImportConfig);
        
        // 订阅事件
        _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Subscribe(OnConfigImported);
        _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Subscribe(OnConfigCreated);
        _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Subscribe(OnConfigLoaded);
        _eventAggregator.GetEvent<PositionUpdatedEvent>().Subscribe(OnPositionUpdated);
        _eventAggregator.GetEvent<PositionAddedEvent>().Subscribe(OnPositionAdded);
        _eventAggregator.GetEvent<PositionDeletedEvent>().Subscribe(OnPositionDeleted);
        _eventAggregator.GetEvent<DeviceAddedEvent>().Subscribe(OnDeviceAdded);
    }
    
    private void ImportConfig()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            Title = "Import Hardware Configuration"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var config = _configService.LoadConfig(dialog.FileName);
                _currentConfig = config;
                
                // 发布事件
                _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
                _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
                {
                    Config = config,
                    FilePath = dialog.FileName,
                    Source = "Import"
                });
                
                MessageBox.Show("Configuration imported successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to import configuration: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    private void OnConfigImported(DeviceConfigDto config)
    {
        _currentConfig = config;
        // 更新设计器中的设备列表
    }
    
    private void OnConfigCreated(DeviceConfigDto config)
    {
        _currentConfig = config;
        // 初始化设计器
    }
    
    private void OnConfigLoaded(ConfigLoadedEventArgs args)
    {
        _currentConfig = args.Config;
        // 处理配置加载
    }
    
    private void OnPositionUpdated(PositionUpdatedEventArgs args)
    {
        // 更新节点中的位置信息
        UpdateNodePositions(args.DeviceId, args.PositionName);
    }
    
    private void OnPositionAdded(PositionPointViewModel position)
    {
        // 刷新位置选项
        RefreshPositionOptions(position.DeviceId);
    }
    
    private void OnPositionDeleted(PositionPointViewModel position)
    {
        // 移除位置选项
        RemovePositionOption(position.DeviceId, position.PositionName);
    }
    
    private void OnDeviceAdded(DeviceAddedEventArgs args)
    {
        // 刷新设备列表
        RefreshDeviceList();
    }
}
```

### 2. 修改 DeviceDebugViewModel

```csharp
public class DeviceDebugViewModel : BindableBase
{
    private readonly IEventAggregator _eventAggregator;
    private readonly IDeviceConfigService _configService;
    
    public ICommand CreateConfigCommand { get; }
    public ICommand ImportConfigCommand { get; }
    
    public DeviceDebugViewModel(
        IEventAggregator eventAggregator,
        IDeviceConfigService configService)
    {
        _eventAggregator = eventAggregator;
        _configService = configService;
        
        CreateConfigCommand = new DelegateCommand(CreateConfig);
        ImportConfigCommand = new DelegateCommand(ImportConfig);
        
        // 订阅事件
        _eventAggregator.GetEvent<PositionUpdatedEvent>().Subscribe(OnPositionUpdated);
        _eventAggregator.GetEvent<PositionAddedEvent>().Subscribe(OnPositionAdded);
        _eventAggregator.GetEvent<PositionDeletedEvent>().Subscribe(OnPositionDeleted);
    }
    
    private void CreateConfig()
    {
        try
        {
            var config = new DeviceConfigDto
            {
                Name = "New Configuration",
                Description = "Created at " + DateTime.Now,
                Motors = new List<MotorDto>(),
                EtherCATMotors = new List<EtherCATMotorDto>(),
                // ... 其他设备列表
            };
            
            CurrentConfig = config;
            
            // 发布事件
            _eventAggregator.GetEvent<DeviceConfigCreatedEvent>().Publish(config);
            _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
            {
                Config = config,
                FilePath = string.Empty,
                Source = "Create"
            });
            
            StatusMessage = "New configuration created successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to create configuration: {ex.Message}";
        }
    }
    
    private void ImportConfig()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            Title = "Import Hardware Configuration"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var config = _configService.LoadConfig(dialog.FileName);
                CurrentConfig = config;
                
                LoadDevices(config);
                
                // 发布事件
                _eventAggregator.GetEvent<DeviceConfigImportedEvent>().Publish(config);
                _eventAggregator.GetEvent<DeviceConfigLoadedEvent>().Publish(new ConfigLoadedEventArgs
                {
                    Config = config,
                    FilePath = dialog.FileName,
                    Source = "Import"
                });
                
                StatusMessage = $"Configuration loaded: {config.Name}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Failed to load configuration: {ex.Message}";
            }
        }
    }
    
    private void OnPositionUpdated(PositionUpdatedEventArgs args)
    {
        // 更新对应设备的位置下拉列表
        var device = AllDevices.FirstOrDefault(d => d.DeviceId == args.DeviceId);
        if (device != null)
        {
            RefreshDevicePositions(device);
        }
    }
    
    private void OnPositionAdded(PositionPointViewModel position)
    {
        // 刷新设备的位置列表
        RefreshDevicePositions(position.DeviceId);
    }
    
    private void OnPositionDeleted(PositionPointViewModel position)
    {
        // 移除设备的位置选项
        RemoveDevicePosition(position.DeviceId, position.PositionName);
    }
}
```

### 3. 修改 PositionSettingsViewModel

```csharp
public class PositionSettingsViewModel : BindableBase
{
    // 已有事件发布代码，确保在修改/添加/删除位置时都发布事件
    
    private void AddPosition()
    {
        // ... 现有代码
        
        // 发布位置添加事件
        _eventAggregator.GetEvent<PositionAddedEvent>().Publish(newPosition);
    }
    
    private void DeletePosition()
    {
        // ... 现有代码
        
        // 发布位置删除事件
        _eventAggregator.GetEvent<PositionDeletedEvent>().Publish(SelectedPosition);
    }
    
    // 位置修改时自动发布（通过 PropertyChanged）
    private void OnPositionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is PositionPointViewModel position)
        {
            if (e.PropertyName == nameof(position.Position) || 
                e.PropertyName == nameof(position.Speed))
            {
                _eventAggregator.GetEvent<PositionUpdatedEvent>().Publish(new PositionUpdatedEventArgs
                {
                    DeviceId = position.DeviceId,
                    PositionName = position.PositionName,
                    Position = position.Position,
                    Speed = position.Speed
                });
            }
        }
    }
    
    // 订阅配置加载事件
    private void OnConfigLoaded(ConfigLoadedEventArgs args)
    {
        // 重新加载位置列表
        LoadPositions(args.Config);
    }
}
```

## 🎨 UI 更新

### DesignerView.xaml

```xaml
<!-- 顶部工具栏 -->
<StackPanel Orientation="Horizontal" Grid.Column="0">
    <!-- 新增：导入配置按钮 -->
    <Button Command="{Binding ImportConfigCommand}" 
           Style="{StaticResource MaterialDesignRaisedButton}"
           Margin="0,0,8,0"
           ToolTip="Import Hardware Configuration">
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="FileImport" Margin="0,0,4,0"/>
            <TextBlock Text="Import Config"/>
        </StackPanel>
    </Button>
    
    <Button Command="{Binding NewProgramCommand}" 
           Style="{StaticResource MaterialDesignOutlinedButton}">
        <!-- 现有按钮 -->
    </Button>
</StackPanel>
```

### DeviceDebugView.xaml

```xaml
<!-- 顶部工具栏 -->
<StackPanel Orientation="Horizontal">
    <!-- 新增：新建配置按钮 -->
    <Button Command="{Binding CreateConfigCommand}" 
           Style="{StaticResource MaterialDesignRaisedButton}"
           Margin="0,0,8,0"
           Background="#4CAF50">
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="Plus" Margin="0,0,4,0"/>
            <TextBlock Text="新建配置"/>
        </StackPanel>
    </Button>
    
    <Button Command="{Binding ImportConfigCommand}" 
           Style="{StaticResource MaterialDesignRaisedButton}"
           Margin="0,0,8,0">
        <StackPanel Orientation="Horizontal">
            <materialDesign:PackIcon Kind="FileImport" Margin="0,0,4,0"/>
            <TextBlock Text="导入配置"/>
        </StackPanel>
    </Button>
</StackPanel>
```

## ✅ 实现检查清单

### Phase 1: 事件系统
- [x] 定义新事件（DeviceConfigCreatedEvent, DeviceConfigLoadedEvent等）
- [ ] 测试事件发布和订阅

### Phase 2: DesignerViewModel
- [ ] 添加 IEventAggregator 依赖注入
- [ ] 添加 IDeviceConfigService 依赖注入
- [ ] 实现 ImportConfigCommand
- [ ] 实现事件订阅处理方法
- [ ] 更新构造函数

### Phase 3: DeviceDebugViewModel
- [ ] 添加 CreateConfigCommand
- [ ] 更新 ImportConfigCommand（如果需要）
- [ ] 实现事件订阅处理方法
- [ ] 测试配置创建和加载

### Phase 4: PositionSettingsViewModel
- [ ] 确保所有位置操作都发布事件
- [ ] 添加配置加载事件订阅
- [ ] 测试位置同步

### Phase 5: UI 更新
- [ ] DesignerView 添加导入按钮
- [ ] DeviceDebugView 添加新建/导入按钮
- [ ] 测试 UI 交互

### Phase 6: 依赖注入配置
- [ ] 在 App.xaml.cs 注册 IDeviceConfigService
- [ ] 更新 ViewModel 构造函数注入
- [ ] 测试依赖注入

### Phase 7: 集成测试
- [ ] 测试配置导入同步
- [ ] 测试配置新建同步
- [ ] 测试位置修改同步
- [ ] 测试位置添加同步
- [ ] 测试位置删除同步

## 🔍 测试场景

### 场景 1: DesignerView 导入配置
1. 打开 DesignerView
2. 点击"Import Config"
3. 选择配置文件
4. **验证：** DeviceDebugView 的设备列表自动更新
5. **验证：** PositionSettingsView 的位置列表自动更新

### 场景 2: DeviceDebugView 新建配置
1. 打开 DeviceDebugView
2. 点击"新建配置"
3. **验证：** DesignerView 初始化为空
4. **验证：** PositionSettingsView 清空位置列表

### 场景 3: PositionSettingsView 修改位置
1. 在 PositionSettingsView 选择一个位置
2. 修改位置值或速度
3. **验证：** DeviceDebugView 中对应设备的位置下拉列表更新
4. **验证：** DesignerView 中使用该位置的节点参数更新

### 场景 4: PositionSettingsView 添加位置
1. 在 PositionSettingsView 点击"添加位置"
2. 填写位置信息并添加
3. **验证：** DeviceDebugView 的位置下拉列表新增该位置
4. **验证：** DesignerView 可以选择新位置

### 场景 5: PositionSettingsView 删除位置
1. 在 PositionSettingsView 选择位置并删除
2. **验证：** DeviceDebugView 位置下拉列表移除该位置
3. **验证：** DesignerView 中使用该位置的节点显示警告

## 📊 数据流图

```
┌─────────────────────────────────────────────────────────────────┐
│                         用户操作                                  │
└────┬────────────────────────────┬──────────────────────┬─────────┘
     │                            │                      │
     ▼                            ▼                      ▼
┌─────────────┐          ┌─────────────────┐     ┌──────────────┐
│DesignerView │          │DeviceDebugView  │     │PositionView  │
│   导入配置   │          │新建/导入配置     │     │修改/增删位置  │
└──────┬──────┘          └────────┬────────┘     └──────┬───────┘
       │                          │                     │
       │ Import                   │ Create/Import       │ Update/Add/Delete
       ▼                          ▼                     ▼
┌──────────────────────────────────────────────────────────────────┐
│                      Event Aggregator                             │
│  ┌────────────────────────────────────────────────────────────┐  │
│  │ Publish Events                                             │  │
│  │  • DeviceConfigImportedEvent                               │  │
│  │  • DeviceConfigCreatedEvent                                │  │
│  │  • DeviceConfigLoadedEvent                                 │  │
│  │  • PositionUpdatedEvent / AddedEvent / DeletedEvent        │  │
│  └────────────────────────────────────────────────────────────┘  │
└──────┬───────────────────────────┬────────────────────────┬──────┘
       │                           │                        │
       │ Subscribe                 │ Subscribe              │ Subscribe
       ▼                           ▼                        ▼
┌─────────────┐          ┌─────────────────┐     ┌──────────────┐
│DesignerVM   │          │DeviceDebugVM    │     │PositionVM    │
│  更新设备    │          │  更新设备列表    │     │  重新加载    │
│  更新位置    │          │  更新位置列表    │     │              │
└─────────────┘          └─────────────────┘     └──────────────┘
```

## 🎯 性能优化

### 1. 事件节流
```csharp
// 使用 ThreadOption.BackgroundThread 避免阻塞 UI
_eventAggregator.GetEvent<PositionUpdatedEvent>()
    .Subscribe(OnPositionUpdated, ThreadOption.BackgroundThread);
```

### 2. 批量更新
```csharp
// 批量添加位置时，最后才发布事件
private void AddPositionsBatch(List<PositionPointViewModel> positions)
{
    foreach (var pos in positions)
    {
        AllPositions.Add(pos);
    }
    
    // 只发布一次批量更新事件
    _eventAggregator.GetEvent<PositionsBatchAddedEvent>().Publish(positions);
}
```

### 3. 防抖动
```csharp
private Timer _updateTimer;

private void OnPositionChanged()
{
    _updateTimer?.Stop();
    _updateTimer = new Timer(500); // 500ms 延迟
    _updateTimer.Elapsed += (s, e) =>
    {
        _updateTimer.Stop();
        PublishPositionUpdate();
    };
    _updateTimer.Start();
}
```

## 📝 注意事项

### 1. 线程安全
- 事件处理可能在后台线程
- 更新 UI 需要使用 `Application.Current.Dispatcher.Invoke`

### 2. 内存泄漏
- 确保 ViewModel 销毁时取消事件订阅
- 使用 `keepSubscriberReferenceAlive: false`

### 3. 循环依赖
- 避免事件处理中再次发布相同事件
- 使用标志位防止递归调用

### 4. 错误处理
- 事件处理中的异常不会传播
- 需要在每个处理方法中捕获异常

---

**实现顺序：** 事件系统 → DesignerViewModel → DeviceDebugViewModel → UI 更新 → 集成测试

**预计工作量：** 4-6 小时

**风险点：** 依赖注入配置、事件订阅生命周期管理

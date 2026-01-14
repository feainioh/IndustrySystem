# 设备调试功能增强总结

## 概述
为 `DeviceDebugViewModel` 添加了三种设备的完整调试功能：离心机、自定义泵和TCU温控。

## 1. 离心机 (CentrifugalDevice) 新增功能

### 新增属性
- `CentrifugalSpeed` - 离心转速 (RPM)
- `CentrifugalTime` - 离心时间 (秒)
- `CentrifugalRotorPosition` - 转子位置 (1-N)
- `CentrifugalConnected` - 连接状态
- `CentrifugalRunning` - 运行状态
- `CentrifugalStatus` - 状态信息
- `IsCentrifugalSelected` - 是否选中离心机

### 新增命令
- `CentrifugalConnectCommand` - 连接离心机
- `CentrifugalSetSpeedCommand` - 设置转速
- `CentrifugalSetTimeCommand` - 设置离心时间
- `CentrifugalSetRotorPositionCommand` - 设置转子位置
- `CentrifugalStartCommand` - 开始离心
- `CentrifugalStopCommand` - 停止离心

### 实现方法
```csharp
- CentrifugalConnectAsync() - 连接设备
- CentrifugalSetSpeedAsync() - 设置转速
- CentrifugalSetTimeAsync() - 设置时间
- CentrifugalSetRotorPositionAsync() - 设置转子位置
- CentrifugalStartAsync() - 开始离心（包含转速、时间、位置信息）
- CentrifugalStopAsync() - 停止离心
```

### 初始化逻辑
- 从配置中读取默认转速 (`DefaultParameters.DefaultSpeed` 或 `Parameters.MaxSpeed`)
- 默认时间：60秒
- 默认转子位置：1
- 初始状态：未连接、未运行

## 2. 自定义泵 (DiyPump) 新增功能

### 新增属性
- `DiyPumpConnected` - 连接状态
- `DiyPumpServoEnabled` - 使能状态
- `DiyPumpChannel` - 当前通道 (1-4)
- `DiyPumpStatus` - 状态信息
- `DiyPumpChannelOptions` - 通道选项 (ReadOnlyCollection<int>: [1,2,3,4])
- `IsDiyPumpSelected` - 是否选中自定义泵

### 新增命令
- `DiyPumpConnectCommand` - 连接泵
- `DiyPumpServoOnCommand` - 上使能
- `DiyPumpServoOffCommand` - 下使能
- `DiyPumpClearAlarmCommand` - 清除报警
- `DiyPumpResetCommand` - 归零
- `DiyPumpSwitchChannelCommand` - 切换通道

### 实现方法
```csharp
- DiyPumpConnectAsync() - 连接设备
- DiyPumpServoAsync(bool enable) - 控制使能（上/下）
- DiyPumpClearAlarmAsync() - 清除报警
- DiyPumpResetAsync() - 归零
- DiyPumpSwitchChannelAsync() - 切换到指定通道
```

### 初始化逻辑
- 默认通道：1
- 初始状态：未连接、未使能

## 3. TCU温控 增强功能

### 新增属性
- `TcuConnected` - 连接状态
- `TcuCirculationEnabled` - 循环开关状态
- *(已有)* `TcuTargetTemperature` - 目标温度
- *(已有)* `TcuCurrentTemperature` - 当前温度
- *(已有)* `TcuIsRunning` - 运行状态

### 新增/增强命令
- `TcuConnectCommand` - 连接TCU
- `TcuStartControlCommand` - 开始控温
- `TcuSetCirculationCommand` - 设置循环开关
- *(已有)* `TcuSetTemperatureCommand` - 设置温度
- *(已有)* `TcuStartCommand` - 启动循环
- *(已有)* `TcuStopCommand` - 停止循环

### 实现方法
```csharp
- TcuConnectAsync() - 连接设备（更新设置 TcuConnected 状态）
- TcuStartControlAsync() - 开始控温（显示目标温度和循环状态）
- TcuSetCirculationAsync() - 设置循环开关
```

### 初始化逻辑
- 默认目标温度：25°C
- 当前温度：0°C
- 初始状态：未连接、循环关闭

## 更新的方法

### UpdateDeviceDetails()
为三种设备类型添加了初始化逻辑：
- `CentrifugalDeviceDto` - 初始化离心机参数
- `DiyPumpDto` - 初始化自定义泵参数
- `TcuDeviceDto` - 增强TCU初始化（添加连接和循环状态）

### SelectedDevice Setter
添加了新设备类型的属性变更通知：
- `IsCentrifugalSelected`
- `IsDiyPumpSelected`

## UI 绑定建议

### 离心机视图
```xml
<StackPanel Visibility="{Binding IsCentrifugalSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <!-- 连接状态 -->
    <TextBlock Text="{Binding CentrifugalStatus}"/>
    
    <!-- 连接按钮 -->
    <Button Command="{Binding CentrifugalConnectCommand}" Content="连接"/>
    
    <!-- 转速设置 -->
    <NumericUpDown Value="{Binding CentrifugalSpeed}"/>
    <Button Command="{Binding CentrifugalSetSpeedCommand}" Content="设置转速"/>
    
    <!-- 时间设置 -->
    <NumericUpDown Value="{Binding CentrifugalTime}"/>
    <Button Command="{Binding CentrifugalSetTimeCommand}" Content="设置时间"/>
    
    <!-- 转子位置 -->
    <NumericUpDown Value="{Binding CentrifugalRotorPosition}" Minimum="1"/>
    <Button Command="{Binding CentrifugalSetRotorPositionCommand}" Content="设置位置"/>
    
    <!-- 控制按钮 -->
    <Button Command="{Binding CentrifugalStartCommand}" Content="开始离心"/>
    <Button Command="{Binding CentrifugalStopCommand}" Content="停止"/>
</StackPanel>
```

### 自定义泵视图
```xml
<StackPanel Visibility="{Binding IsDiyPumpSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <!-- 连接和状态 -->
    <TextBlock Text="{Binding DiyPumpStatus}"/>
    <Button Command="{Binding DiyPumpConnectCommand}" Content="连接"/>
    
    <!-- 使能控制 -->
    <Button Command="{Binding DiyPumpServoOnCommand}" Content="上使能"/>
    <Button Command="{Binding DiyPumpServoOffCommand}" Content="下使能"/>
    
    <!-- 通道切换 -->
    <ComboBox ItemsSource="{Binding DiyPumpChannelOptions}" 
              SelectedItem="{Binding DiyPumpChannel}"/>
    <Button Command="{Binding DiyPumpSwitchChannelCommand}" Content="切换通道"/>
    
    <!-- 功能按钮 -->
    <Button Command="{Binding DiyPumpClearAlarmCommand}" Content="清除报警"/>
    <Button Command="{Binding DiyPumpResetCommand}" Content="归零"/>
</StackPanel>
```

### TCU增强视图
```xml
<StackPanel Visibility="{Binding IsTcuSelected, Converter={StaticResource BoolToVisibilityConverter}}">
    <!-- 连接 -->
    <Button Command="{Binding TcuConnectCommand}" Content="连接"/>
    
    <!-- 温度设置 -->
    <NumericUpDown Value="{Binding TcuTargetTemperature}"/>
    <Button Command="{Binding TcuSetTemperatureCommand}" Content="设置温度"/>
    
    <!-- 循环控制 -->
    <CheckBox IsChecked="{Binding TcuCirculationEnabled}" Content="开启循环"/>
    <Button Command="{Binding TcuSetCirculationCommand}" Content="应用循环设置"/>
    
    <!-- 控温控制 -->
    <Button Command="{Binding TcuStartControlCommand}" Content="开始控温"/>
    <Button Command="{Binding TcuStopCommand}" Content="停止"/>
    
    <!-- 当前状态 -->
    <TextBlock Text="{Binding TcuCurrentTemperature, StringFormat='当前温度: {0}°C'}"/>
</StackPanel>
```

## 测试建议

### 离心机测试
1. 选择离心机设备
2. 点击"连接"按钮，验证连接状态
3. 设置转速、时间、转子位置
4. 点击"开始离心"，验证参数显示正确
5. 点击"停止"，验证停止功能

### 自定义泵测试
1. 选择自定义泵设备
2. 点击"连接"按钮
3. 测试上使能/下使能
4. 切换不同通道 (1-4)
5. 测试清除报警和归零功能

### TCU测试
1. 选择TCU设备
2. 点击"连接"按钮
3. 设置目标温度
4. 切换循环开关状态
5. 点击"开始控温"，验证显示包含温度和循环状态
6. 测试停止功能

## 注意事项

1. **模拟实现**：所有方法目前都是模拟实现（使用 `Task.Delay`），实际硬件控制需要调用 `_hardwareController` 的相应方法。

2. **错误处理**：所有方法都包含 try-catch 错误处理，记录日志并更新状态消息。

3. **状态同步**：
   - 离心机：通过 `CentrifugalStatus` 显示详细状态
   - 自定义泵：通过 `DiyPumpStatus` 显示操作结果
   - TCU：通过 `StatusMessage` 和连接状态显示

4. **参数验证**：在执行操作前检查设备是否已选中（`SelectedDevice` 和具体设备类型）。

5. **初始化**：所有新属性在 `UpdateDeviceDetails()` 方法中根据设备类型正确初始化。

## 后续工作

如需实际硬件控制，需要：
1. 在 `IHardwareController` 接口中定义相应方法
2. 实现硬件通信逻辑
3. 替换模拟的 `Task.Delay` 调用为实际的硬件控制方法
4. 添加实时状态读取和更新机制

## 文件修改
- `src/Presentation/IndustrySystem.MotionDesigner/ViewModels/DeviceDebugViewModel.cs` - 全部修改

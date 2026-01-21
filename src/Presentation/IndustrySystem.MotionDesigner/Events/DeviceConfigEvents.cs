using System;
using Prism.Events;
using IndustrySystem.MotionDesigner.Services;

namespace IndustrySystem.MotionDesigner.Events;

/// <summary>
/// 设备配置已导入事件
/// </summary>
public class DeviceConfigImportedEvent : PubSubEvent<DeviceConfigDto>
{
}

/// <summary>
/// 设备配置已保存事件
/// </summary>
public class DeviceConfigSavedEvent : PubSubEvent<DeviceConfigDto>
{
}

/// <summary>
/// 位置点已更新事件
/// </summary>
public class PositionUpdatedEvent : PubSubEvent<PositionUpdatedEventArgs>
{
}

public class PositionUpdatedEventArgs
{
    public string DeviceId { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public double Position { get; set; }
    public double Speed { get; set; }
}

/// <summary>
/// 位置点已添加事件
/// </summary>
public class PositionAddedEvent : PubSubEvent<PositionPointViewModel>
{
}

/// <summary>
/// 位置点已删除事件
/// </summary>
public class PositionDeletedEvent : PubSubEvent<PositionPointViewModel>
{
}

/// <summary>
/// 配置文件已创建事件（新建配置）
/// </summary>
public class DeviceConfigCreatedEvent : PubSubEvent<DeviceConfigDto>
{
}

/// <summary>
/// 配置文件已加载事件（从文件加载）
/// </summary>
public class DeviceConfigLoadedEvent : PubSubEvent<ConfigLoadedEventArgs>
{
}

public class ConfigLoadedEventArgs
{
    public DeviceConfigDto Config { get; set; } = null!;
    public string FilePath { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // "Import", "Create", "Load"
}

/// <summary>
/// 请求同步配置事件（用于主动请求最新配置）
/// </summary>
public class ConfigSyncRequestEvent : PubSubEvent<string>
{
}

/// <summary>
/// 设备已添加事件
/// </summary>
public class DeviceAddedEvent : PubSubEvent<DeviceAddedEventArgs>
{
}

public class DeviceAddedEventArgs
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
}



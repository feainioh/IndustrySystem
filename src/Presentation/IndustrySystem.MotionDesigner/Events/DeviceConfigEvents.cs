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


namespace IndustrySystem.MotionDesigner.Services;

/// <summary>
/// 设备配置服务接口
/// </summary>
public interface IDeviceConfigService
{
    /// <summary>
    /// 从文件导入设备配置
    /// </summary>
    Task<DeviceConfigDto> ImportFromFileAsync(string filePath);

    /// <summary>
    /// 从 JSON 字符串导入设备配置
    /// </summary>
    Task<DeviceConfigDto> ImportFromJsonAsync(string json);

    /// <summary>
    /// 获取所有设备列表
    /// </summary>
    DeviceConfigDto? GetCurrentConfig();
}

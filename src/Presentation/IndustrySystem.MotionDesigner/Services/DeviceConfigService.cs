using System.IO;
using System.Text.Json;
using NLog;

namespace IndustrySystem.MotionDesigner.Services;

/// <summary>
/// 设备配置服务实现
/// </summary>
public class DeviceConfigService : IDeviceConfigService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private DeviceConfigDto? _currentConfig;

    public async Task<DeviceConfigDto> ImportFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"文件不存在: {filePath}", filePath);
        }

        _logger.Info("正在导入设备配置文件: {FilePath}", filePath);

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return await ImportFromJsonAsync(json);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导入设备配置文件失败: {FilePath}", filePath);
            throw new InvalidOperationException($"导入设备配置文件失败: {ex.Message}", ex);
        }
    }

    public Task<DeviceConfigDto> ImportFromJsonAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON 字符串不能为空", nameof(json));
        }

        _logger.Debug("正在解析设备配置 JSON");

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var config = JsonSerializer.Deserialize<DeviceConfigDto>(json, options);
            
            if (config == null)
            {
                throw new InvalidOperationException("JSON 反序列化结果为 null");
            }

            _currentConfig = config;
            
            _logger.Info("成功导入设备配置，包含 {MotorCount} 个电机, {PumpCount} 个泵, {RobotCount} 个机器人",
                config.Motors.Count + config.EtherCATMotors.Count,
                config.SyringePumps.Count + config.PeristalticPumps.Count + config.DiyPumps.Count,
                config.JakaRobots.Count);
            
            return Task.FromResult(config);
        }
        catch (JsonException ex)
        {
            _logger.Error(ex, "JSON 反序列化失败");
            throw new InvalidOperationException($"JSON 格式错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "解析设备配置失败");
            throw;
        }
    }

    public DeviceConfigDto? GetCurrentConfig()
    {
        return _currentConfig;
    }
}

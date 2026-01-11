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
    private string? _currentFilePath;

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
            var config = await ImportFromJsonAsync(json);
            _currentFilePath = filePath;
            return config;
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

    public async Task SaveConfigAsync(DeviceConfigDto config)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            throw new InvalidOperationException("没有可保存的配置文件路径，请先导入配置或使用导出功能");
        }

        _logger.Info("正在保存设备配置到: {FilePath}", _currentFilePath);

        try
        {
            await ExportToFileAsync(config, _currentFilePath);
            _currentConfig = config;
            _logger.Info("设备配置保存成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "保存设备配置失败");
            throw new InvalidOperationException($"保存设备配置失败: {ex.Message}", ex);
        }
    }

    public async Task ExportToFileAsync(DeviceConfigDto config, string filePath)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("文件路径不能为空", nameof(filePath));
        }

        _logger.Info("正在导出设备配置到: {FilePath}", filePath);

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(config, options);
            await File.WriteAllTextAsync(filePath, json);
            
            _logger.Info("设备配置导出成功");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "导出设备配置失败");
            throw new InvalidOperationException($"导出设备配置失败: {ex.Message}", ex);
        }
    }
}

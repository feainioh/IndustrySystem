using System.Text.Json.Serialization;

namespace IndustrySystem.MotionDesigner.Services;

/// <summary>
/// 设备配置根对象
/// </summary>
public class DeviceConfigDto
{
    [JsonPropertyName("deviceInfo")]
    public DeviceInfoDto? DeviceInfo { get; set; }

    [JsonPropertyName("canDevices")]
    public List<CanDeviceDto> CanDevices { get; set; } = new();

    [JsonPropertyName("etherCATDevices")]
    public List<EtherCATDeviceDto> EtherCATDevices { get; set; } = new();

    [JsonPropertyName("motors")]
    public List<MotorDto> Motors { get; set; } = new();

    [JsonPropertyName("etherCATMotors")]
    public List<EtherCATMotorDto> EtherCATMotors { get; set; } = new();

    [JsonPropertyName("syringePumps")]
    public List<SyringePumpDto> SyringePumps { get; set; } = new();

    [JsonPropertyName("peristalticPumps")]
    public List<PeristalticPumpDto> PeristalticPumps { get; set; } = new();

    [JsonPropertyName("diyPumps")]
    public List<DiyPumpDto> DiyPumps { get; set; } = new();

    [JsonPropertyName("jakaRobots")]
    public List<JakaRobotDto> JakaRobots { get; set; } = new();

    [JsonPropertyName("tcuDevices")]
    public List<TcuDeviceDto> TcuDevices { get; set; } = new();

    [JsonPropertyName("centrifugalDevices")]
    public List<CentrifugalDeviceDto> CentrifugalDevices { get; set; } = new();

    [JsonPropertyName("scanners")]
    public List<ScannerDto> Scanners { get; set; } = new();

    [JsonPropertyName("weighingSensors")]
    public List<WeighingSensorDto> WeighingSensors { get; set; } = new();

    [JsonPropertyName("twoChannelValves")]
    public List<TwoChannelValveDto> TwoChannelValves { get; set; } = new();

    [JsonPropertyName("threeChannelValves")]
    public List<ThreeChannelValveDto> ThreeChannelValves { get; set; } = new();

    [JsonPropertyName("ecatIODevices")]
    public List<EcatIODeviceDto> EcatIODevices { get; set; } = new();
}

public class DeviceInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = string.Empty;
}

public class CanDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class EtherCATDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class MotorDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("axisId")]
    public int AxisId { get; set; }

    [JsonPropertyName("workPositions")]
    public List<WorkPositionDto> WorkPositions { get; set; } = new();
}

public class EtherCATMotorDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("axisNo")]
    public int AxisNo { get; set; }

    [JsonPropertyName("workPositions")]
    public List<WorkPositionDto> WorkPositions { get; set; } = new();
}

public class WorkPositionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("position")]
    public double Position { get; set; }

    [JsonPropertyName("speed")]
    public double Speed { get; set; }
}

public class SyringePumpDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class PeristalticPumpDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class DiyPumpDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class JakaRobotDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TcuDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class CentrifugalDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ScannerDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class WeighingSensorDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TwoChannelValveDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ThreeChannelValveDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class EcatIODeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ioChannels")]
    public List<IoChannelDto> IoChannels { get; set; } = new();
}

public class IoChannelDto
{
    [JsonPropertyName("channelNumber")]
    public int ChannelNumber { get; set; }

    [JsonPropertyName("channelName")]
    public string ChannelName { get; set; } = string.Empty;

    [JsonPropertyName("ioType")]
    public string IoType { get; set; } = string.Empty;
}

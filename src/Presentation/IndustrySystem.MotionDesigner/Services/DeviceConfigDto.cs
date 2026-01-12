using System.Collections.ObjectModel;
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

    [JsonPropertyName("chillerDevices")]
    public List<ChillerDeviceDto> ChillerDevices { get; set; } = new();

    [JsonPropertyName("customModbusDevices")]
    public List<CustomModbusDeviceDto> CustomModbusDevices { get; set; } = new();
}

public class DeviceInfoDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("serialNumber")]
    public string SerialNumber { get; set; } = string.Empty;

    [JsonPropertyName("firmwareVersion")]
    public string FirmwareVersion { get; set; } = string.Empty;

    [JsonPropertyName("manufactureDate")]
    public DateTime? ManufactureDate { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class CanDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("deviceType")]
    public int DeviceType { get; set; }

    [JsonPropertyName("deviceIndex")]
    public int DeviceIndex { get; set; }

    [JsonPropertyName("channels")]
    public List<CanChannelDto> Channels { get; set; } = new();

    [JsonPropertyName("autoReconnect")]
    public bool AutoReconnect { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class CanChannelDto
{
    [JsonPropertyName("channelIndex")]
    public int ChannelIndex { get; set; }

    [JsonPropertyName("baudRate")]
    public string BaudRate { get; set; } = string.Empty;

    [JsonPropertyName("canType")]
    public int CanType { get; set; }

    [JsonPropertyName("mode")]
    public int Mode { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class EtherCATDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("cardId")]
    public int CardId { get; set; }

    [JsonPropertyName("eniFilePath")]
    public string EniFilePath { get; set; } = string.Empty;

    [JsonPropertyName("axisParamFilePath")]
    public string AxisParamFilePath { get; set; } = string.Empty;

    [JsonPropertyName("autoReconnect")]
    public bool AutoReconnect { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class MotorParametersDto
{
    [JsonPropertyName("pulsesPerRevolution")]
    public int PulsesPerRevolution { get; set; }

    [JsonPropertyName("gearRatio")]
    public double GearRatio { get; set; }

    [JsonPropertyName("displacementPerRevolution")]
    public double DisplacementPerRevolution { get; set; }

    [JsonPropertyName("jogSpeed")]
    public double JogSpeed { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
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

    [JsonPropertyName("communicationId")]
    public int CommunicationId { get; set; }

    [JsonPropertyName("deviceIndex")]
    public int DeviceIndex { get; set; }

    [JsonPropertyName("channelIndex")]
    public int ChannelIndex { get; set; }

    [JsonPropertyName("parameters")]
    public MotorParametersDto? Parameters { get; set; }

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

    [JsonPropertyName("etherCATDeviceId")]
    public string EtherCATDeviceId { get; set; } = string.Empty;

    [JsonPropertyName("parameters")]
    public MotorParametersDto? Parameters { get; set; }

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

    [JsonPropertyName("communicationId")]
    public int CommunicationId { get; set; }

    [JsonPropertyName("deviceIndex")]
    public int DeviceIndex { get; set; }

    [JsonPropertyName("channelIndex")]
    public int ChannelIndex { get; set; }

    [JsonPropertyName("canFrameId")]
    public int CanFrameId { get; set; }

    [JsonPropertyName("syringeVolume")]
    public double SyringeVolume { get; set; }

    [JsonPropertyName("liquidOffset")]
    public double LiquidOffset { get; set; }
}

public class PeristalticPumpParametersDto
{
    [JsonPropertyName("pulsesPerRevolution")]
    public int PulsesPerRevolution { get; set; }

    [JsonPropertyName("gearRatio")]
    public double GearRatio { get; set; }

    [JsonPropertyName("displacementPerRevolution")]
    public double DisplacementPerRevolution { get; set; }

    [JsonPropertyName("deviceID")]
    public string DeviceID { get; set; } = string.Empty;

    [JsonPropertyName("communicationId")]
    public string CommunicationId { get; set; } = string.Empty;

    [JsonPropertyName("defaultRPM")]
    public double DefaultRPM { get; set; }

    [JsonPropertyName("defaultFlowRate")]
    public double DefaultFlowRate { get; set; }

    [JsonPropertyName("pulseConversionFactor")]
    public double PulseConversionFactor { get; set; }

    [JsonPropertyName("accelerationTime")]
    public double AccelerationTime { get; set; }

    [JsonPropertyName("decelerationTime")]
    public double DecelerationTime { get; set; }

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; }

    [JsonPropertyName("commandDelayMs")]
    public int CommandDelayMs { get; set; }
}

public class PeristalticPumpDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("communicationId")]
    public int CommunicationId { get; set; }

    [JsonPropertyName("deviceIndex")]
    public int DeviceIndex { get; set; }

    [JsonPropertyName("channelIndex")]
    public int ChannelIndex { get; set; }

    [JsonPropertyName("canFrameId")]
    public int CanFrameId { get; set; }

    [JsonPropertyName("productModel")]
    public string ProductModel { get; set; } = string.Empty;

    [JsonPropertyName("pumpHeadModel")]
    public string PumpHeadModel { get; set; } = string.Empty;

    [JsonPropertyName("tubeSpec")]
    public string TubeSpec { get; set; } = string.Empty;

    [JsonPropertyName("rotorCount")]
    public int RotorCount { get; set; }

    [JsonPropertyName("maxRPM")]
    public double MaxRPM { get; set; }

    [JsonPropertyName("maxFlowRate")]
    public double MaxFlowRate { get; set; }

    [JsonPropertyName("liquidOffset")]
    public double LiquidOffset { get; set; }

    [JsonPropertyName("pumpAccuracy")]
    public double PumpAccuracy { get; set; }

    [JsonPropertyName("parameters")]
    public PeristalticPumpParametersDto? Parameters { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class DiyPumpDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("pulsePerRevolution")]
    public int PulsePerRevolution { get; set; }

    [JsonPropertyName("gearRatio")]
    public double GearRatio { get; set; }

    [JsonPropertyName("displacementPerRevolution")]
    public double DisplacementPerRevolution { get; set; }

    [JsonPropertyName("offsetPostion")]
    public double OffsetPosition { get; set; }

    [JsonPropertyName("communicationId")]
    public int CommunicationId { get; set; }

    [JsonPropertyName("maxRPM")]
    public double MaxRPM { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class RobotParametersDto
{
    [JsonPropertyName("slaveId")]
    public int SlaveId { get; set; }

    [JsonPropertyName("deviceStatusStartAddress")]
    public int DeviceStatusStartAddress { get; set; }

    [JsonPropertyName("taskFeedbackStartAddress")]
    public int TaskFeedbackStartAddress { get; set; }

    [JsonPropertyName("controlCommandStartAddress")]
    public int ControlCommandStartAddress { get; set; }

    [JsonPropertyName("taskSendStartAddress")]
    public int TaskSendStartAddress { get; set; }

    [JsonPropertyName("taskStartAddress")]
    public int TaskStartAddress { get; set; }

    [JsonPropertyName("continueExecutionSignalAddress")]
    public int ContinueExecutionSignalAddress { get; set; }
}

public class JakaRobotDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("robotType")]
    public string RobotType { get; set; } = string.Empty;

    [JsonPropertyName("robotModel")]
    public string RobotModel { get; set; } = string.Empty;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("autoReconnect")]
    public bool AutoReconnect { get; set; }

    [JsonPropertyName("connectionTimeout")]
    public int ConnectionTimeout { get; set; }

    [JsonPropertyName("receiveTimeout")]
    public int ReceiveTimeout { get; set; }

    [JsonPropertyName("sendTimeout")]
    public int SendTimeout { get; set; }

    [JsonPropertyName("reconnectInterval")]
    public int ReconnectInterval { get; set; }

    [JsonPropertyName("robotParameters")]
    public RobotParametersDto? RobotParameters { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class TcuDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("portName")]
    public string PortName { get; set; } = string.Empty;

    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; }

    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    [JsonPropertyName("stopBits")]
    public string StopBits { get; set; } = "One";

    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    [JsonPropertyName("slaveId")]
    public int SlaveId { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class CentrifugalParametersDto
{
    [JsonPropertyName("maxSpeed")]
    public int MaxSpeed { get; set; }

    [JsonPropertyName("minSpeed")]
    public int MinSpeed { get; set; }

    [JsonPropertyName("maxTime")]
    public int MaxTime { get; set; }

    [JsonPropertyName("minTime")]
    public int MinTime { get; set; }

    [JsonPropertyName("accelerationTime")]
    public int AccelerationTime { get; set; }

    [JsonPropertyName("decelerationTime")]
    public int DecelerationTime { get; set; }

    [JsonPropertyName("capacityPositions")]
    public int CapacityPositions { get; set; }

    [JsonPropertyName("maxCapacity")]
    public double MaxCapacity { get; set; }

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;
}

public class CentrifugalDefaultParametersDto
{
    [JsonPropertyName("defaultSpeed")]
    public int DefaultSpeed { get; set; }

    [JsonPropertyName("defaultTime")]
    public int DefaultTime { get; set; }

    [JsonPropertyName("defaultPosition1")]
    public int DefaultPosition1 { get; set; }

    [JsonPropertyName("defaultPosition2")]
    public int DefaultPosition2 { get; set; }

    [JsonPropertyName("safetyTimeout")]
    public int SafetyTimeout { get; set; }
}

public class CentrifugalDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("portName")]
    public string PortName { get; set; } = string.Empty;

    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; }

    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    [JsonPropertyName("stopBits")]
    public string StopBits { get; set; } = "One";

    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    [JsonPropertyName("slaveId")]
    public int SlaveId { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("parameters")]
    public CentrifugalParametersDto? Parameters { get; set; }

    [JsonPropertyName("workPositions")]
    public List<WorkPositionDto> WorkPositions { get; set; } = new();

    [JsonPropertyName("defaultParameters")]
    public CentrifugalDefaultParametersDto? DefaultParameters { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class ScannerDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("triggerCommand")]
    public string TriggerCommand { get; set; } = string.Empty;

    [JsonPropertyName("connectionTimeout")]
    public int ConnectionTimeout { get; set; }

    [JsonPropertyName("readTimeout")]
    public int ReadTimeout { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class WeighingSensorDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("portName")]
    public string PortName { get; set; } = string.Empty;

    [JsonPropertyName("slaveAddress")]
    public int SlaveAddress { get; set; }

    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; }

    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    [JsonPropertyName("stopBits")]
    public string StopBits { get; set; } = "One";

    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    [JsonPropertyName("decimalPlaces")]
    public int DecimalPlaces { get; set; }

    [JsonPropertyName("maxWeight")]
    public double MaxWeight { get; set; }

    [JsonPropertyName("precision")]
    public double Precision { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class TwoChannelValveDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ioChannel")]
    public int IoChannel { get; set; }

    [JsonPropertyName("defaultState")]
    public int DefaultState { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class ThreeChannelValveDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("ioChannel")]
    public int IoChannel { get; set; }

    [JsonPropertyName("defaultState")]
    public int DefaultState { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class EcatIODeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("etherCATDeviceId")]
    public string EtherCATDeviceId { get; set; } = string.Empty;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("ioChannels")]
    public List<IoChannelDto> IoChannels { get; set; } = new();

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

public class IoChannelDto
{
    [JsonPropertyName("channelNumber")]
    public int ChannelNumber { get; set; }

    [JsonPropertyName("channelName")]
    public string ChannelName { get; set; } = string.Empty;

    [JsonPropertyName("ioType")]
    public string IoType { get; set; } = string.Empty;

    [JsonPropertyName("defaultValue")]
    public double DefaultValue { get; set; }

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 冷水机设备配置
/// </summary>
public class ChillerDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("communicationType")]
    public string CommunicationType { get; set; } = "RS485"; // RS485 or TCP

    [JsonPropertyName("portName")]
    public string PortName { get; set; } = string.Empty;

    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; } = 9600;

    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    [JsonPropertyName("stopBits")]
    public string StopBits { get; set; } = "One";

    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    [JsonPropertyName("slaveId")]
    public int SlaveId { get; set; } = 1;

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = "Modbus RTU";

    [JsonPropertyName("minTemperature")]
    public double MinTemperature { get; set; }

    [JsonPropertyName("maxTemperature")]
    public double MaxTemperature { get; set; }

    [JsonPropertyName("temperaturePrecision")]
    public double TemperaturePrecision { get; set; } = 0.1;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

/// <summary>
/// 自定义Modbus设备配置
/// </summary>
public class CustomModbusDeviceDto
{
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = string.Empty;

    [JsonPropertyName("communicationType")]
    public string CommunicationType { get; set; } = "RS485"; // RS485, TCP

    [JsonPropertyName("portName")]
    public string PortName { get; set; } = string.Empty;

    [JsonPropertyName("ipAddress")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("tcpPort")]
    public int TcpPort { get; set; } = 502;

    [JsonPropertyName("baudRate")]
    public int BaudRate { get; set; } = 9600;

    [JsonPropertyName("parity")]
    public string Parity { get; set; } = "None";

    [JsonPropertyName("stopBits")]
    public string StopBits { get; set; } = "One";

    [JsonPropertyName("dataBits")]
    public int DataBits { get; set; } = 8;

    [JsonPropertyName("slaveId")]
    public int SlaveId { get; set; } = 1;

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = "Modbus RTU"; // Modbus RTU, Modbus TCP, Modbus ASCII

    [JsonPropertyName("registers")]
    public List<ModbusRegisterDto> Registers { get; set; } = new();

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("createdTime")]
    public DateTime? CreatedTime { get; set; }

    [JsonPropertyName("modifiedTime")]
    public DateTime? ModifiedTime { get; set; }
}

/// <summary>
/// Modbus寄存器配置
/// </summary>
public class ModbusRegisterDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public int Address { get; set; }

    [JsonPropertyName("registerType")]
    public string RegisterType { get; set; } = "Holding"; // Coil, DiscreteInput, Input, Holding

    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "Int16"; // Bool, Int16, UInt16, Int32, UInt32, Float, Double

    [JsonPropertyName("registerCount")]
    public int RegisterCount { get; set; } = 1;

    [JsonPropertyName("scale")]
    public double Scale { get; set; } = 1.0;

    [JsonPropertyName("offset")]
    public double Offset { get; set; } = 0;

    [JsonPropertyName("unit")]
    public string Unit { get; set; } = string.Empty;

    [JsonPropertyName("readWrite")]
    public string ReadWrite { get; set; } = "RW"; // R, W, RW

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 用于UI显示的设备包装类
/// </summary>
public class DeviceItemViewModel
{
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public object? OriginalDevice { get; set; }
    public string IconKind { get; set; } = "Cog";
}

/// <summary>
/// 设备分类视图模型 - 用于 TreeView 分组显示
/// </summary>
public class DeviceCategoryViewModel
{
    public string CategoryName { get; set; } = string.Empty;
    public string IconKind { get; set; } = "Folder";
    public bool IsExpanded { get; set; } = true;
    public ObservableCollection<DeviceItemViewModel> Devices { get; set; } = new();
    public int DeviceCount => Devices.Count;
}

/// <summary>
/// 位置点位视图模型
/// </summary>
public class PositionPointViewModel
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public double Position { get; set; }
    public double Speed { get; set; }
    public bool IsModified { get; set; }
}

using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.Dialogs;

public class AddDeviceDialogViewModel : BindableBase
{
    private int _selectedDeviceTypeIndex;
    private string _deviceId = string.Empty;
    private string _deviceName = string.Empty;
    private string _description = string.Empty;
    private bool? _dialogResult;

    // CAN 参数
    private string _canNodeId = string.Empty;
    private bool _showCanParameters;

    // 串口参数
    private int _selectedPortIndex;
    private string _baudRate = "9600";
    private bool _showSerialPortParameters;

    // 网络参数
    private string _ipAddress = "192.168.1.100";
    private string _port = "502";
    private bool _showNetworkParameters;

    // EtherCAT 参数
    private string _slaveId = string.Empty;
    private bool _showEtherCatParameters;

    public ObservableCollection<string> DeviceTypes { get; } = new()
    {
        "CAN 电机",
        "EtherCAT 电机",
        "注射泵",
        "蠕动泵",
        "自定义泵",
        "离心机",
        "TCU 温控",
        "冷水机",
        "称重传感器",
        "扫码枪",
        "Jaka 机器人",
        "IO 设备"
    };

    public ObservableCollection<string> SerialPorts { get; } = new()
    {
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8"
    };

    public int SelectedDeviceTypeIndex
    {
        get => _selectedDeviceTypeIndex;
        set
        {
            if (SetProperty(ref _selectedDeviceTypeIndex, value))
            {
                UpdateParameterVisibility();
            }
        }
    }

    public string DeviceId
    {
        get => _deviceId;
        set => SetProperty(ref _deviceId, value);
    }

    public string DeviceName
    {
        get => _deviceName;
        set => SetProperty(ref _deviceName, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string CanNodeId
    {
        get => _canNodeId;
        set => SetProperty(ref _canNodeId, value);
    }

    public bool ShowCanParameters
    {
        get => _showCanParameters;
        set => SetProperty(ref _showCanParameters, value);
    }

    public int SelectedPortIndex
    {
        get => _selectedPortIndex;
        set => SetProperty(ref _selectedPortIndex, value);
    }

    public string BaudRate
    {
        get => _baudRate;
        set => SetProperty(ref _baudRate, value);
    }

    public bool ShowSerialPortParameters
    {
        get => _showSerialPortParameters;
        set => SetProperty(ref _showSerialPortParameters, value);
    }

    public string IpAddress
    {
        get => _ipAddress;
        set => SetProperty(ref _ipAddress, value);
    }

    public string Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public bool ShowNetworkParameters
    {
        get => _showNetworkParameters;
        set => SetProperty(ref _showNetworkParameters, value);
    }

    public string SlaveId
    {
        get => _slaveId;
        set => SetProperty(ref _slaveId, value);
    }

    public bool ShowEtherCatParameters
    {
        get => _showEtherCatParameters;
        set => SetProperty(ref _showEtherCatParameters, value);
    }

    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    // 输出结果
    public string ResultDeviceType { get; private set; } = string.Empty;
    public string ResultDeviceId { get; private set; } = string.Empty;
    public string ResultDeviceName { get; private set; } = string.Empty;
    public string ResultDescription { get; private set; } = string.Empty;
    public int? ResultCanNodeId { get; private set; }
    public string? ResultPortName { get; private set; }
    public int? ResultBaudRate { get; private set; }
    public string? ResultIpAddress { get; private set; }
    public int? ResultPort { get; private set; }
    public int? ResultSlaveId { get; private set; }

    public ICommand AddCommand { get; }
    public ICommand CancelCommand { get; }

    public AddDeviceDialogViewModel()
    {
        AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd)
            .ObservesProperty(() => DeviceId)
            .ObservesProperty(() => DeviceName);
        CancelCommand = new DelegateCommand(ExecuteCancel);

        // 默认选中第一个设备类型
        SelectedDeviceTypeIndex = 0;
    }

    private void UpdateParameterVisibility()
    {
        // 隐藏所有参数面板
        ShowCanParameters = false;
        ShowSerialPortParameters = false;
        ShowNetworkParameters = false;
        ShowEtherCatParameters = false;

        switch (SelectedDeviceTypeIndex)
        {
            case 0: // CAN 电机
                ShowCanParameters = true;
                break;
            case 1: // EtherCAT 电机
                ShowEtherCatParameters = true;
                break;
            case 2: // 注射泵
            case 3: // 蠕动泵
            case 4: // 自定义泵
            case 5: // 离心机
            case 6: // TCU 温控
            case 7: // 冷水机
            case 8: // 称重传感器
                ShowSerialPortParameters = true;
                break;
            case 9: // 扫码枪
            case 10: // Jaka 机器人
                ShowNetworkParameters = true;
                break;
            case 11: // IO 设备
                ShowEtherCatParameters = true;
                break;
        }
    }

    private bool CanExecuteAdd()
    {
        return !string.IsNullOrWhiteSpace(DeviceId) && !string.IsNullOrWhiteSpace(DeviceName);
    }

    private void ExecuteAdd()
    {
        // 获取设备类型
        ResultDeviceType = DeviceTypes[SelectedDeviceTypeIndex];
        ResultDeviceId = DeviceId.Trim();
        ResultDeviceName = DeviceName.Trim();
        ResultDescription = Description.Trim();

        // 获取连接参数
        switch (SelectedDeviceTypeIndex)
        {
            case 0: // CAN 电机
                if (int.TryParse(CanNodeId, out var nodeId))
                {
                    ResultCanNodeId = nodeId;
                }
                break;
            case 1: // EtherCAT 电机
            case 11: // IO 设备
                if (int.TryParse(SlaveId, out var slaveId))
                {
                    ResultSlaveId = slaveId;
                }
                break;
            case 2: // 注射泵
            case 3: // 蠕动泵
            case 4: // 自定义泵
            case 5: // 离心机
            case 6: // TCU 温控
            case 7: // 冷水机
            case 8: // 称重传感器
                if (SelectedPortIndex >= 0 && SelectedPortIndex < SerialPorts.Count)
                {
                    ResultPortName = SerialPorts[SelectedPortIndex];
                }
                if (int.TryParse(BaudRate, out var baudRate))
                {
                    ResultBaudRate = baudRate;
                }
                break;
            case 9: // 扫码枪
            case 10: // Jaka 机器人
                ResultIpAddress = IpAddress.Trim();
                if (int.TryParse(Port, out var port))
                {
                    ResultPort = port;
                }
                break;
        }

        DialogResult = true;
    }

    private void ExecuteCancel()
    {
        DialogResult = false;
    }
}

using System.Windows;
using IndustrySystem.MotionDesigner.ViewModels.Dialogs;

namespace IndustrySystem.MotionDesigner.Dialogs;

public partial class AddDeviceDialog : Window
{
    private readonly AddDeviceDialogViewModel _viewModel;

    public string DeviceType => _viewModel.ResultDeviceType;
    public string DeviceId => _viewModel.ResultDeviceId;
    public string DeviceName => _viewModel.ResultDeviceName;
    public string Description => _viewModel.ResultDescription;
    
    // 连接参数
    public int? CanNodeId => _viewModel.ResultCanNodeId;
    public string? PortName => _viewModel.ResultPortName;
    public int? BaudRate => _viewModel.ResultBaudRate;
    public string? IpAddress => _viewModel.ResultIpAddress;
    public int? Port => _viewModel.ResultPort;
    public int? SlaveId => _viewModel.ResultSlaveId;
    
    public AddDeviceDialog()
    {
        InitializeComponent();
        _viewModel = new AddDeviceDialogViewModel();
        DataContext = _viewModel;
    }
}


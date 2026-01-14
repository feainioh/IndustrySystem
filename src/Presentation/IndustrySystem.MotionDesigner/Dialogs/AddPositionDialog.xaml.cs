using System.Windows;
using IndustrySystem.MotionDesigner.Services;
using IndustrySystem.MotionDesigner.ViewModels.Dialogs;

namespace IndustrySystem.MotionDesigner.Dialogs;

public partial class AddPositionDialog : Window
{
    private readonly AddPositionDialogViewModel _viewModel;

    public string? SelectedDeviceId => _viewModel.ResultDeviceId;
    public string? SelectedDeviceName => _viewModel.ResultDeviceName;
    public string? SelectedDeviceType => _viewModel.ResultDeviceType;
    public string PositionName => _viewModel.ResultPositionName;
    public double PositionValue => _viewModel.ResultPositionValue;
    public double Speed => _viewModel.ResultSpeed;
    public bool ContinueAdding => _viewModel.ContinueAdding;
    
    public AddPositionDialog(DeviceConfigDto config)
    {
        InitializeComponent();
        _viewModel = new AddPositionDialogViewModel(config);
        DataContext = _viewModel;
    }
}


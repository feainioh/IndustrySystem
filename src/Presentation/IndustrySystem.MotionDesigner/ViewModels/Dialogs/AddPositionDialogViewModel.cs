using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using IndustrySystem.MotionDesigner.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.Dialogs;

public class AddPositionDialogViewModel : BindableBase
{
    private DeviceItem? _selectedDevice;
    private string _positionName = string.Empty;
    private string _positionValue = "0";
    private string _speed = "100";
    private bool _continueAdding;
    private bool? _dialogResult;

    public ObservableCollection<DeviceItem> AvailableDevices { get; } = new();

    public DeviceItem? SelectedDevice
    {
        get => _selectedDevice;
        set => SetProperty(ref _selectedDevice, value);
    }

    public string PositionName
    {
        get => _positionName;
        set => SetProperty(ref _positionName, value);
    }

    public string PositionValue
    {
        get => _positionValue;
        set => SetProperty(ref _positionValue, value);
    }

    public string Speed
    {
        get => _speed;
        set => SetProperty(ref _speed, value);
    }

    public bool ContinueAdding
    {
        get => _continueAdding;
        set => SetProperty(ref _continueAdding, value);
    }

    public bool? DialogResult
    {
        get => _dialogResult;
        set => SetProperty(ref _dialogResult, value);
    }

    // 输出结果
    public string? ResultDeviceId { get; private set; }
    public string? ResultDeviceName { get; private set; }
    public string? ResultDeviceType { get; private set; }
    public string ResultPositionName { get; private set; } = string.Empty;
    public double ResultPositionValue { get; private set; }
    public double ResultSpeed { get; private set; }

    public ICommand AddCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand PresetCommand { get; }

    public AddPositionDialogViewModel(DeviceConfigDto config)
    {
        LoadAvailableDevices(config);

        AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedDevice)
            .ObservesProperty(() => PositionName).ObservesProperty(() => PositionValue).ObservesProperty(() => Speed);
        CancelCommand = new DelegateCommand(ExecuteCancel);
        PresetCommand = new DelegateCommand<string>(ExecutePreset);
    }

    private void LoadAvailableDevices(DeviceConfigDto config)
    {
        AvailableDevices.Clear();

        // 添加 CAN 电机
        foreach (var motor in config.Motors)
        {
            AvailableDevices.Add(new DeviceItem
            {
                DeviceId = motor.DeviceId,
                DeviceName = motor.Name,
                DeviceType = "CAN电机"
            });
        }

        // 添加 EtherCAT 电机
        foreach (var motor in config.EtherCATMotors)
        {
            AvailableDevices.Add(new DeviceItem
            {
                DeviceId = motor.DeviceId,
                DeviceName = motor.Name,
                DeviceType = "EtherCAT电机"
            });
        }

        // 添加离心机
        foreach (var device in config.CentrifugalDevices)
        {
            AvailableDevices.Add(new DeviceItem
            {
                DeviceId = device.DeviceId,
                DeviceName = device.Name,
                DeviceType = "离心机"
            });
        }

        // 添加机器人
        foreach (var robot in config.JakaRobots)
        {
            AvailableDevices.Add(new DeviceItem
            {
                DeviceId = robot.DeviceId,
                DeviceName = robot.Name,
                DeviceType = "机器人"
            });
        }

        // 默认选中第一个设备
        if (AvailableDevices.Count > 0)
        {
            SelectedDevice = AvailableDevices[0];
        }
    }

    private bool CanExecuteAdd()
    {
        return SelectedDevice != null &&
               !string.IsNullOrWhiteSpace(PositionName) &&
               !string.IsNullOrWhiteSpace(PositionValue) &&
               !string.IsNullOrWhiteSpace(Speed);
    }

    private void ExecuteAdd()
    {
        // 验证位置值
        if (!double.TryParse(PositionValue, out var positionValue))
        {
            // 在实际应用中应该使用更好的错误提示方式
            System.Windows.MessageBox.Show("位置值必须是有效的数字", "验证错误", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // 验证速度
        if (!double.TryParse(Speed, out var speed) || speed <= 0)
        {
            System.Windows.MessageBox.Show("速度必须是大于 0 的数字", "验证错误", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // 保存结果
        ResultDeviceId = SelectedDevice!.DeviceId;
        ResultDeviceName = SelectedDevice.DeviceName;
        ResultDeviceType = SelectedDevice.DeviceType;
        ResultPositionName = PositionName.Trim();
        ResultPositionValue = positionValue;
        ResultSpeed = speed;

        DialogResult = true;

        // 如果继续添加，清空表单
        if (ContinueAdding)
        {
            PositionName = string.Empty;
            PositionValue = "0";
            Speed = "100";
        }
    }

    private void ExecuteCancel()
    {
        DialogResult = false;
    }

    private void ExecutePreset(string? presetName)
    {
        if (string.IsNullOrEmpty(presetName)) return;

        PositionName = presetName;

        switch (presetName)
        {
            case "HOME_POS":
                PositionValue = "0";
                Speed = "50";
                break;
            case "SAMPLE_POS":
                PositionValue = "100";
                Speed = "200";
                break;
            case "WASH_POS":
                PositionValue = "200";
                Speed = "150";
                break;
            case "WAIT_POS":
                PositionValue = "0";
                Speed = "100";
                break;
            case "PARK_POS":
                PositionValue = "300";
                Speed = "100";
                break;
            case "MAINTENANCE_POS":
                PositionValue = "500";
                Speed = "50";
                break;
        }
    }

    public class DeviceItem
    {
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string DisplayName => $"{DeviceName} ({DeviceType})";
    }
}

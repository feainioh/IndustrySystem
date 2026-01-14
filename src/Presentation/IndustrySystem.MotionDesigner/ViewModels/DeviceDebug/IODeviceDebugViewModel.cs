using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class IODeviceDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private EcatIODeviceDto? _selectedDevice;
    private bool _ioConnected = true;
    private string _ioStatus = string.Empty;
    
    public ObservableCollection<IoChannelControlItem> IoChannels { get; } = new();
    
    public EcatIODeviceDto? SelectedDevice
    {
        get => _selectedDevice;
        set
        {
            if (SetProperty(ref _selectedDevice, value))
            {
                OnDeviceChanged();
            }
        }
    }
    
    public bool IoConnected
    {
        get => _ioConnected;
        set => SetProperty(ref _ioConnected, value);
    }
    
    public string IoStatus
    {
        get => _ioStatus;
        set => SetProperty(ref _ioStatus, value);
    }
    
    public IEnumerable<IoChannelControlItem> IoDiChannels => IoChannels.Where(c => c.IoType.Equals("DI", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoDoChannels => IoChannels.Where(c => c.IoType.Equals("DO", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoAiChannels => IoChannels.Where(c => c.IoType.Equals("AI", StringComparison.OrdinalIgnoreCase));
    public IEnumerable<IoChannelControlItem> IoAoChannels => IoChannels.Where(c => c.IoType.Equals("AO", StringComparison.OrdinalIgnoreCase));
    
    public ICommand IoToggleOutputCommand { get; }
    public ICommand IoSetOutputCommand { get; }
    
    public IODeviceDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        IoToggleOutputCommand = new DelegateCommand<IoChannelControlItem>(async item => await IoToggleOutputAsync(item));
        IoSetOutputCommand = new DelegateCommand<IoChannelControlItem>(async item => await IoSetOutputAsync(item));
    }
    
    private void OnDeviceChanged()
    {
        IoChannels.Clear();
        
        if (SelectedDevice != null)
        {
            foreach (var ch in SelectedDevice.IoChannels.OrderBy(c => c.ChannelNumber))
            {
                IoChannels.Add(new IoChannelControlItem(ch));
            }
        }
        
        RaisePropertyChanged(nameof(IoDiChannels));
        RaisePropertyChanged(nameof(IoDoChannels));
        RaisePropertyChanged(nameof(IoAiChannels));
        RaisePropertyChanged(nameof(IoAoChannels));
    }
    
    private async Task IoToggleOutputAsync(IoChannelControlItem? item)
    {
        if (item == null || SelectedDevice == null || !item.IsOutput) return;
        
        try
        {
            item.Toggle();
            await _hardwareController.SetIoOutputAsync(SelectedDevice.DeviceId, item.ChannelNumber, item.Value > 0.5);
            IoStatus = $"IO {SelectedDevice.Name} 通道 {item.ChannelNumber} 已设置为 {item.Value}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 IO 输出失败");
            IoStatus = $"设置失败: {ex.Message}";
        }
    }
    
    private async Task IoSetOutputAsync(IoChannelControlItem? item)
    {
        if (item == null || SelectedDevice == null || !item.IsOutput) return;
        
        try
        {
            await _hardwareController.SetIoOutputAsync(SelectedDevice.DeviceId, item.ChannelNumber, item.Value > 0.5);
            IoStatus = $"IO {SelectedDevice.Name} 通道 {item.ChannelNumber} 已设置为 {item.Value}";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "设置 IO 输出失败");
            IoStatus = $"设置失败: {ex.Message}";
        }
    }
}

public class IoChannelControlItem : BindableBase
{
    public IoChannelControlItem(IoChannelDto channel)
    {
        Channel = channel;
        Value = channel.DefaultValue;
    }

    public IoChannelDto Channel { get; }
    public int ChannelNumber => Channel.ChannelNumber;
    public string ChannelName => Channel.ChannelName;
    public string IoType => Channel.IoType;
    public bool IsOutput => IoType.Equals("DO", StringComparison.OrdinalIgnoreCase) || IoType.Equals("AO", StringComparison.OrdinalIgnoreCase);

    private double _value;
    public double Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                RaisePropertyChanged(nameof(BoolValue));
            }
        }
    }

    public bool BoolValue
    {
        get => Value > 0.5;
        set
        {
            Value = value ? 1 : 0;
            RaisePropertyChanged(nameof(Value));
        }
    }

    public void Toggle()
    {
        if (IsOutput)
        {
            BoolValue = !BoolValue;
        }
    }
}

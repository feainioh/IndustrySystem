using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands; // added

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class HardwareDebugViewModel : INotifyPropertyChanged
{
    private readonly ICommunicationAppService _comm;
    private bool _isConnected;
    public bool IsConnected { get => _isConnected; private set { _isConnected = value; OnPropertyChanged(); } }
    public string Transport { get; set; } = "ModbusTCP"; // or CAN/EtherCAT/HTTP/TCP
    public string Endpoint { get; set; } = "127.0.0.1:502";
    private string _lastRead = string.Empty;
    public string LastRead { get => _lastRead; private set { _lastRead = value; OnPropertyChanged(); } }
    public ushort ReadStartAddress { get; set; } = 0;
    public ushort ReadCount { get; set; } = 2;
    public ushort WriteAddress { get; set; } = 0;
    public string WriteValues { get; set; } = "123"; // 逗号分隔，或单值
    private string _status = string.Empty;
    public string Status { get => _status; private set { _status = value; OnPropertyChanged(); } }

    public ICommand ConnectCommand { get; }
    public ICommand DisconnectCommand { get; }
    public ICommand ReadCommand { get; }
    public ICommand WriteCommand { get; }

    public HardwareDebugViewModel(ICommunicationAppService comm)
    {
        _comm = comm;
        ConnectCommand = new AsyncDelegateCommand(ConnectAsync);
        DisconnectCommand = new AsyncDelegateCommand(DisconnectAsync);
        ReadCommand = new AsyncDelegateCommand(ReadAsync);
        WriteCommand = new AsyncDelegateCommand(WriteAsync);
    }

    private async Task ConnectAsync()
    {
        if (Transport == "ModbusTCP")
        {
            var parts = Endpoint.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 502;
            try
            {
                await _comm.ConnectModbusAsync(host, port);
                IsConnected = true;
                Status = $"已连接 {host}:{port}";
            }
            catch (System.Exception ex)
            {
                Status = $"连接失败: {ex.Message}";
            }
        }
    }

    private async Task DisconnectAsync()
    {
        if (Transport == "ModbusTCP")
        {
            await _comm.DisconnectModbusAsync();
            IsConnected = false;
            Status = "已断开";
        }
    }

    private async Task ReadAsync()
    {
        if (Transport == "ModbusTCP")
        {
            try
            {
                var data = await _comm.ReadHoldingRegistersAsync(ReadStartAddress, ReadCount);
                LastRead = string.Join(",", data);
                Status = $"读取成功: {ReadStartAddress} 共 {ReadCount}";
            }
            catch (System.Exception ex)
            {
                Status = $"读取失败: {ex.Message}";
            }
        }
    }
    private async Task WriteAsync()
    {
        if (Transport != "ModbusTCP") return;
        try
        {
            // 解析写入值：支持单值或多值
            var parts = (WriteValues ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) { Status = "写入值为空"; return; }
            if (parts.Length == 1)
            {
                if (ushort.TryParse(parts[0], out var v))
                {
                    await _comm.WriteSingleRegisterAsync(WriteAddress, v);
                    Status = $"写入单寄存器成功: [{WriteAddress}]={v}";
                }
                else { Status = "写入值格式错误"; }
            }
            else
            {
                var list = new System.Collections.Generic.List<ushort>(parts.Length);
                foreach (var p in parts)
                {
                    if (!ushort.TryParse(p, out var val)) { Status = $"值格式错误: {p}"; return; }
                    list.Add(val);
                }
                await _comm.WriteMultipleRegistersAsync(WriteAddress, list.ToArray());
                Status = $"写入多寄存器成功: 起始[{WriteAddress}], 数量={list.Count}";
            }
        }
        catch (System.Exception ex)
        {
            Status = $"写入失败: {ex.Message}";
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

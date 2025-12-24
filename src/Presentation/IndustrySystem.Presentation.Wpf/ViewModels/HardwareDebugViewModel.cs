using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using Prism.Commands;
using Prism.Mvvm;
using NLog;
using System;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public class HardwareDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly ICommunicationAppService _comm;
    private bool _isConnected;
    public bool IsConnected { get => _isConnected; private set => SetProperty(ref _isConnected, value); }
    public string Transport { get; set; } = "ModbusTCP";
    public string Endpoint { get; set; } = "127.0.0.1:502";
    private string _lastRead = string.Empty;
    public string LastRead { get => _lastRead; private set => SetProperty(ref _lastRead, value); }
    public ushort ReadStartAddress { get; set; } = 0;
    public ushort ReadCount { get; set; } = 2;
    public ushort WriteAddress { get; set; } = 0;
    public string WriteValues { get; set; } = "123";
    private string _status = string.Empty;
    public string Status { get => _status; private set => SetProperty(ref _status, value); }

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
        _logger.Info(Resources.Strings.Log_HardwareDebugViewModel_Initialized);
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
                _logger.Info(string.Format(Resources.Strings.Log_HardwareDebug_Connecting, host, port));
                await _comm.ConnectModbusAsync(host, port);
                IsConnected = true;
                Status = string.Format(Resources.Strings.Log_HardwareDebug_Connected, host, port);
                _logger.Info(Status);
            }
            catch (Exception ex)
            {
                Status = string.Format(Resources.Strings.Log_HardwareDebug_ConnectFailed, ex.Message);
                _logger.Error(ex, Resources.Strings.Log_HardwareDebug_ConnectFailed);
            }
        }
    }

    private async Task DisconnectAsync()
    {
        if (Transport == "ModbusTCP")
        {
            _logger.Info(Resources.Strings.Log_HardwareDebug_Disconnecting);
            await _comm.DisconnectModbusAsync();
            IsConnected = false;
            Status = Resources.Strings.Log_HardwareDebug_Disconnected;
            _logger.Info(Status);
        }
    }

    private async Task ReadAsync()
    {
        if (Transport == "ModbusTCP")
        {
            try
            {
                _logger.Debug(string.Format(Resources.Strings.Log_HardwareDebug_Reading, ReadStartAddress, ReadCount));
                var data = await _comm.ReadHoldingRegistersAsync(ReadStartAddress, ReadCount);
                LastRead = string.Join(",", data);
                Status = string.Format(Resources.Strings.Log_HardwareDebug_ReadSuccess, ReadStartAddress, ReadCount);
                _logger.Info(Status);
            }
            catch (Exception ex)
            {
                Status = string.Format(Resources.Strings.Log_HardwareDebug_ReadFailed, ex.Message);
                _logger.Error(ex, Resources.Strings.Log_HardwareDebug_ReadFailed);
            }
        }
    }

    private async Task WriteAsync()
    {
        if (Transport != "ModbusTCP") return;
        try
        {
            var parts = (WriteValues ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) { Status = Resources.Strings.Log_HardwareDebug_WriteEmpty; return; }
            if (parts.Length == 1)
            {
                if (ushort.TryParse(parts[0], out var v))
                {
                    await _comm.WriteSingleRegisterAsync(WriteAddress, v);
                    Status = string.Format(Resources.Strings.Log_HardwareDebug_WriteSingleSuccess, WriteAddress, v);
                    _logger.Info(Status);
                }
                else { Status = Resources.Strings.Log_HardwareDebug_WriteFormatError; }
            }
            else
            {
                var list = new System.Collections.Generic.List<ushort>(parts.Length);
                foreach (var p in parts)
                {
                    if (!ushort.TryParse(p, out var val)) { Status = string.Format(Resources.Strings.Log_HardwareDebug_WriteValueFormatError, p); return; }
                    list.Add(val);
                }
                await _comm.WriteMultipleRegistersAsync(WriteAddress, list.ToArray());
                Status = string.Format(Resources.Strings.Log_HardwareDebug_WriteMultiSuccess, WriteAddress, list.Count);
                _logger.Info(Status);
            }
        }
        catch (Exception ex)
        {
            Status = string.Format(Resources.Strings.Log_HardwareDebug_WriteFailed, ex.Message);
            _logger.Error(ex, Resources.Strings.Log_HardwareDebug_WriteFailed);
        }
    }
}

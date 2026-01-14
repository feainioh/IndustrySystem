using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.MotionDesigner.Services;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels.DeviceDebug;

public class ScannerDebugViewModel : BindableBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly IHardwareController _hardwareController;
    
    private ScannerDto? _selectedScanner;
    private bool _scannerConnected;
    private bool _scannerEnabled = true;
    private bool _scannerScanning;
    private bool _scannerContinuousScan;
    private int _scannerInterval = 1000;
    private string _scannerStatus = string.Empty;
    private string _scannerIp = string.Empty;
    private int _scannerPort;
    private string _scannerResult = string.Empty;
    
    private ObservableCollection<string> _scannerHistory = new();
    
    public ScannerDto? SelectedScanner
    {
        get => _selectedScanner;
        set
        {
            if (SetProperty(ref _selectedScanner, value))
            {
                OnScannerChanged();
            }
        }
    }
    
    public bool ScannerConnected
    {
        get => _scannerConnected;
        set => SetProperty(ref _scannerConnected, value);
    }
    
    public bool ScannerEnabled
    {
        get => _scannerEnabled;
        set => SetProperty(ref _scannerEnabled, value);
    }
    
    public bool ScannerScanning
    {
        get => _scannerScanning;
        set => SetProperty(ref _scannerScanning, value);
    }
    
    public bool ScannerContinuousScan
    {
        get => _scannerContinuousScan;
        set => SetProperty(ref _scannerContinuousScan, value);
    }
    
    public int ScannerInterval
    {
        get => _scannerInterval;
        set => SetProperty(ref _scannerInterval, value);
    }
    
    public string ScannerStatus
    {
        get => _scannerStatus;
        set => SetProperty(ref _scannerStatus, value);
    }
    
    public string ScannerIp
    {
        get => _scannerIp;
        set => SetProperty(ref _scannerIp, value);
    }
    
    public int ScannerPort
    {
        get => _scannerPort;
        set => SetProperty(ref _scannerPort, value);
    }
    
    public string ScannerResult
    {
        get => _scannerResult;
        set => SetProperty(ref _scannerResult, value);
    }
    
    public ObservableCollection<string> ScannerHistory
    {
        get => _scannerHistory;
        set => SetProperty(ref _scannerHistory, value);
    }
    
    public ICommand ScannerConnectCommand { get; }
    public ICommand ScannerDisconnectCommand { get; }
    public ICommand ScannerScanCommand { get; }
    public ICommand ScannerClearResultCommand { get; }
    public ICommand ScannerCopyResultCommand { get; }
    public ICommand ScannerStartContinuousCommand { get; }
    public ICommand ScannerStopContinuousCommand { get; }
    public ICommand ScannerClearHistoryCommand { get; }
    
    public ScannerDebugViewModel(IHardwareController hardwareController)
    {
        _hardwareController = hardwareController;
        
        ScannerConnectCommand = new DelegateCommand(async () => await ScannerConnectAsync());
        ScannerDisconnectCommand = new DelegateCommand(async () => await ScannerDisconnectAsync());
        ScannerScanCommand = new DelegateCommand(async () => await ScannerScanAsync());
        ScannerClearResultCommand = new DelegateCommand(() => { ScannerResult = string.Empty; ScannerStatus = "扫描结果已清除"; });
        ScannerCopyResultCommand = new DelegateCommand(() => { if (!string.IsNullOrEmpty(ScannerResult)) { System.Windows.Clipboard.SetText(ScannerResult); ScannerStatus = "已复制到剪贴板"; } });
        ScannerStartContinuousCommand = new DelegateCommand(async () => await ScannerStartContinuousAsync());
        ScannerStopContinuousCommand = new DelegateCommand(() => { ScannerScanning = false; ScannerStatus = "连续扫描已停止"; });
        ScannerClearHistoryCommand = new DelegateCommand(() => { ScannerHistory.Clear(); ScannerStatus = "扫描历史已清除"; });
    }
    
    private void OnScannerChanged()
    {
        if (SelectedScanner != null)
        {
            ScannerIp = SelectedScanner.IpAddress;
            ScannerPort = SelectedScanner.Port;
            ScannerConnected = false;
            ScannerResult = string.Empty;
            ScannerStatus = string.Empty;
        }
    }
    
    private async Task ScannerConnectAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(100);
        ScannerConnected = true;
        ScannerStatus = $"扫码枪 {SelectedScanner.Name} 已连接 ({ScannerIp}:{ScannerPort})";
    }
    
    private async Task ScannerDisconnectAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(50);
        ScannerConnected = false;
        ScannerScanning = false;
        ScannerStatus = $"扫码枪 {SelectedScanner.Name} 已断开";
    }
    
    private async Task ScannerScanAsync()
    {
        if (SelectedScanner == null) return;
        await Task.Delay(100);
        ScannerResult = $"SCAN-{DateTime.Now:HHmmss}";
        ScannerHistory.Add(ScannerResult);
        ScannerStatus = $"扫码枪 {SelectedScanner.Name} 读取: {ScannerResult} ({ScannerIp}:{ScannerPort})";
    }
    
    private async Task ScannerStartContinuousAsync()
    {
        if (SelectedScanner == null || !ScannerConnected) return;
        ScannerScanning = true;
        ScannerStatus = $"开始连续扫描，间隔 {ScannerInterval}ms";
        await Task.Delay(100);
    }
}

using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Ioc;

namespace IndustrySystem.MotionDesigner;

public partial class MainWindow : Window
{
    public MainWindow(IContainerProvider containerProvider)
    {
        InitializeComponent();        
    }

    /// <summary>
    /// 允许拖动窗口
    /// </summary>
    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }
}

/// <summary>
/// Bool 到宽度的转换器
/// </summary>
public class BoolToWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isVisible && isVisible)
        {
            if (parameter is string widthStr && double.TryParse(widthStr, out var width))
            {
                return width;
            }
            return 300.0;
        }
        return 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MainWindowViewModel : BindableBase
{
private readonly IRegionManager _regionManager;
private string _currentProgramName = "Untitled";
private bool _isProjectExplorerVisible = true;
private string _statusMessage = "欢迎使用 Motion Designer";

    
public string CurrentProgramName
{
    get => _currentProgramName;
    set => SetProperty(ref _currentProgramName, value);
}

public bool IsProjectExplorerVisible
{
    get => _isProjectExplorerVisible;
    set => SetProperty(ref _isProjectExplorerVisible, value);
}

public string StatusMessage
{
    get => _statusMessage;
    set => SetProperty(ref _statusMessage, value);
}
    
public DelegateCommand MinimizeCommand { get; }
public DelegateCommand MaximizeCommand { get; }
public DelegateCommand CloseCommand { get; }
public DelegateCommand NavigateToDesignerCommand { get; }
public DelegateCommand NavigateToDeviceDebugCommand { get; }
public DelegateCommand NavigateToPositionSettingsCommand { get; }
public DelegateCommand ToggleProjectExplorerCommand { get; }
    
public MainWindowViewModel(IRegionManager regionManager)
{
    _regionManager = regionManager;
        
    MinimizeCommand = new DelegateCommand(() => 
        System.Windows.Application.Current.MainWindow.WindowState = WindowState.Minimized);
    MaximizeCommand = new DelegateCommand(() =>
    {
        var window = System.Windows.Application.Current.MainWindow;
        window.WindowState = window.WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    });
    CloseCommand = new DelegateCommand(() => 
        System.Windows.Application.Current.Shutdown());
        
    NavigateToDesignerCommand = new DelegateCommand(() =>
    {
        if (_regionManager != null)
            {
                _regionManager.RequestNavigate("MainRegion" , "DesignerView");
            }
        });
        
        NavigateToDeviceDebugCommand = new DelegateCommand(() =>
        {
            if (_regionManager != null)
            {
                _regionManager.RequestNavigate("MainRegion", "DeviceDebugView");
            }
        });
        
        NavigateToPositionSettingsCommand = new DelegateCommand(() =>
        {
            if (_regionManager != null)
            {
                _regionManager.RequestNavigate("MainRegion", "PositionSettingsView");
            }
        });

        ToggleProjectExplorerCommand = new DelegateCommand(() =>
        {
            IsProjectExplorerVisible = !IsProjectExplorerVisible;
        });
    }
}

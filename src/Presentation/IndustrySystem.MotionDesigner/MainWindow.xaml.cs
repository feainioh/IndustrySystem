using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Ioc;
using System.Reflection;

namespace IndustrySystem.MotionDesigner;

public partial class MainWindow : Window
{
    private readonly object? _regionManager;
    
    public MainWindow(IContainerProvider containerProvider)
    {
        InitializeComponent();        
        
    }
}

public class MainWindowViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;
    private string _currentProgramName = "Untitled";
    
    public string CurrentProgramName
    {
        get => _currentProgramName;
        set => SetProperty(ref _currentProgramName, value);
    }
    
    public DelegateCommand MinimizeCommand { get; }
    public DelegateCommand MaximizeCommand { get; }
    public DelegateCommand CloseCommand { get; }
    public DelegateCommand NavigateToDesignerCommand { get; }
    public DelegateCommand NavigateToDeviceDebugCommand { get; }
    
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
            //    var requestNavigateMethod = _regionManager.GetType().GetMethod("RequestNavigate", new[] { typeof(string), typeof(string), typeof(object) });
            //    requestNavigateMethod?.Invoke(_regionManager, new object?[] { "MainRegion", "DesignerView", null });
                _regionManager.RequestNavigate("MainRegion" , "DesignerView");
            }
        });
        
        NavigateToDeviceDebugCommand = new DelegateCommand(() =>
        {
            if (_regionManager != null)
            {
                //var requestNavigateMethod = _regionManager.GetType().GetMethod("RequestNavigate", new[] { typeof(string), typeof(string), typeof(object) });
                //requestNavigateMethod?.Invoke(_regionManager, new object?[] { "MainRegion", "DeviceDebugView", null });
                _regionManager.RequestNavigate("MainRegion", "DeviceDebugView");
            }
        });
    }
}

using System.Windows;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner;

public partial class MainWindow : Window
{
    public MainWindow(IRegionManager regionManager)
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        
        // Navigate to designer view on startup
        regionManager.RegisterViewWithRegion("MainRegion", typeof(Views.DesignerView));
    }
}

public class MainWindowViewModel : BindableBase
{
    private string _currentProgramName = "Untitled";
    
    public string CurrentProgramName
    {
        get => _currentProgramName;
        set => SetProperty(ref _currentProgramName, value);
    }
    
    public DelegateCommand MinimizeCommand { get; }
    public DelegateCommand MaximizeCommand { get; }
    public DelegateCommand CloseCommand { get; }
    
    public MainWindowViewModel()
    {
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
    }
}

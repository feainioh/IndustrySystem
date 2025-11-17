using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class DeviceParamsView : UserControl
    {
        public DeviceParamsView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<DeviceParamsViewModel>();
        }
    }
}

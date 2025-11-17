using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RealtimeDataView : UserControl
    {
        public RealtimeDataView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<RealtimeDataViewModel>();
        }
    }
}

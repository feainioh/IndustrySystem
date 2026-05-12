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
            var vm = ContainerLocator.Current.Resolve<RealtimeDataViewModel>();
            DataContext = vm;
        }
    }
}

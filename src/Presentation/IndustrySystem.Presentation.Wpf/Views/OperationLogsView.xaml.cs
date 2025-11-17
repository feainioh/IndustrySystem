using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class OperationLogsView : UserControl
    {
        public OperationLogsView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<OperationLogsViewModel>();
        }
    }
}

using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentConfigView : UserControl
    {
        public ExperimentConfigView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<ExperimentConfigViewModel>();
        }
    }
}

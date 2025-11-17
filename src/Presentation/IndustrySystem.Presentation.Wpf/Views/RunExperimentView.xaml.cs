using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RunExperimentView : UserControl
    {
        public RunExperimentView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<RunExperimentViewModel>();
        }
    }
}

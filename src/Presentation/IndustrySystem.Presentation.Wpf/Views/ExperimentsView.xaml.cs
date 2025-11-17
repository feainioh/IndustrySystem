using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentsView : UserControl
    {
        public ExperimentsView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<ExperimentsViewModel>();
        }
    }
}

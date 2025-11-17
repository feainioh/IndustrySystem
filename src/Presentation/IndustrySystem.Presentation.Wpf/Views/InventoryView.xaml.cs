using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<InventoryViewModel>();
        }
    }
}

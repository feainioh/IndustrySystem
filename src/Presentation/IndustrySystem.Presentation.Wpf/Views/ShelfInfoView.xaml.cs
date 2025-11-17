using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ShelfInfoView : UserControl
    {
        public ShelfInfoView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<ShelfInfoViewModel>();
        }
    }
}

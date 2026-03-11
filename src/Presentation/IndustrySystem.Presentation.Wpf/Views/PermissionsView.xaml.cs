using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class PermissionsView : UserControl
    {
        public PermissionsView(ViewModels.PermissionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}


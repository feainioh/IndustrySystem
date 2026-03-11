using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RoleManageView : UserControl
    {
        public RoleManageView(RoleManageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}


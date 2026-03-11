using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
 public partial class MaterialInfoView : UserControl
 {
 public MaterialInfoView(MaterialInfoViewModel viewModel)
 {
 InitializeComponent();
 DataContext = viewModel;
 }
 }
}

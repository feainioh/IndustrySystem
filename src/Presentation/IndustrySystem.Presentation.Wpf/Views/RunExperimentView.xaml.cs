using System.Windows.Controls;
using System.Windows.Input;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RunExperimentView : UserControl
    {
        public RunExperimentView()
        {
            InitializeComponent();
        }

        private void StatusBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (DataContext is ViewModels.RunExperimentViewModel vm)
                vm.IsStatusBarExpanded = !vm.IsStatusBarExpanded;
        }
    }
}

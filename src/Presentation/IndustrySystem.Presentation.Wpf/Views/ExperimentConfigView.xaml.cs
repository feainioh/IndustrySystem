using System.Windows;
using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentConfigView : UserControl
    {
        private ExperimentConfigViewModel? ViewModel => DataContext as ExperimentConfigViewModel;

        public ExperimentConfigView()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.NavigateToCurrentParameterEditor();
        }
    }
}

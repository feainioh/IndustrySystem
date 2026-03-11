using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views.Dialogs
{
    public partial class ContainerListDialog : UserControl
    {
        public ContainerListDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("RootDialogHost");
        }
    }
}

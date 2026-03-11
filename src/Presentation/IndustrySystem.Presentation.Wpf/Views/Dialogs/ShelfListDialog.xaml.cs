using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views.Dialogs
{
    public partial class ShelfListDialog : UserControl
    {
        public ShelfListDialog()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogHost.Close("RootDialogHost");
        }
    }
}

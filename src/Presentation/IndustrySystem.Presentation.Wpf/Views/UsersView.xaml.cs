using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.UsersViewModel vm && UserNameBox != null && DisplayNameBox != null)
            {
                _ = vm.AddAsync(UserNameBox.Text, DisplayNameBox.Text);
                UserNameBox.Text = string.Empty;
                DisplayNameBox.Text = string.Empty;
            }
        }
    }
}

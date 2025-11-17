using System;
using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class PermissionsView : UserControl
    {
        public PermissionsView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.PermissionsViewModel vm && NameBox != null && DisplayNameBox != null)
            {
                _ = vm.AddAsync(NameBox.Text, DisplayNameBox.Text);
                NameBox.Text = string.Empty;
                DisplayNameBox.Text = string.Empty;
            }
        }
    }
}

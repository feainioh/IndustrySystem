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
            if (DataContext is ViewModels.PermissionsViewModel vm && 
                NameBox != null && DisplayNameBox != null)
            {
                _ = vm.AddAsync(NameBox.Text, DisplayNameBox.Text);
                NameBox.Clear();
                DisplayNameBox.Clear();
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.PermissionsViewModel vm)
            {
                vm.EditPermission(id);
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is ViewModels.PermissionsViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}

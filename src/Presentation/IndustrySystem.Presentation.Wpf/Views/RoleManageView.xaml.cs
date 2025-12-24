using System;
using System.Windows;
using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RoleManageView : UserControl
    {
        public RoleManageView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is RoleManageViewModel vm && 
                RoleNameBox != null)
            {
                _ = vm.AddAsync(RoleNameBox.Text);
                RoleNameBox.Clear();
                if (DescBox != null) DescBox.Clear();
            }
        }

        private void OnPermissions(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is RoleManageViewModel vm)
            {
                _ = vm.ManagePermissionsAsync(id);
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id)
            {
                MessageBox.Show(
                    $"Edit Role: {id}\n\nThis feature is not yet implemented.", 
                    "Edit Role", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is RoleManageViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}

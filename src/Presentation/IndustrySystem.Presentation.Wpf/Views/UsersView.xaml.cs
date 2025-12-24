using System;
using System.Windows;
using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView(UsersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm && 
                UserNameBox != null && DisplayNameBox != null)
            {
                _ = vm.AddAsync(UserNameBox.Text, DisplayNameBox.Text);
                UserNameBox.Text = string.Empty;
                DisplayNameBox.Text = string.Empty;
            }
        }

        private void OnResetPassword(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is UsersViewModel vm)
            {
                _ = vm.ResetPasswordAsync(id);
            }
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            // TODO: Implement edit dialog
            if (sender is Button btn && btn.Tag is Guid id)
            {
                MessageBox.Show($"{Strings.Btn_Edit} {id}", Strings.Tooltip_EditUser, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is UsersViewModel vm)
            {
                var result = MessageBox.Show(
                    Strings.Msg_ConfirmDeleteUser,
                    Strings.Msg_ConfirmDelete,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _ = vm.DeleteAsync(id);
                }
            }
        }
    }
}

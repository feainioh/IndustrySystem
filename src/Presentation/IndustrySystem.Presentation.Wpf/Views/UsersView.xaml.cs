using System;
using System.Windows;
using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;

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
            if (sender is Button btn && btn.Tag is Guid id)
            {
                _ = OpenUserDialogAsync(id);
            }
        }

        private async System.Threading.Tasks.Task OpenUserDialogAsync(Guid? id)
        {
            var dialogVm = ContainerLocator.Current.Resolve<UserEditDialogViewModel>();
            await dialogVm.LoadAsync(id);
            var dialog = new Dialogs.UserEditDialog { DataContext = dialogVm };

            System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModels.DialogViewModel.DialogResult))
                {
                    DialogHost.Close("RootDialogHost", dialogVm.DialogResult);
                }
            };

            dialogVm.PropertyChanged += handler;
            try
            {
                var result = await DialogHost.Show(dialog, "RootDialogHost");
                if (result is bool saved && saved && DataContext is UsersViewModel vm)
                {
                    await vm.LoadAsync();
                }
            }
            finally
            {
                dialogVm.PropertyChanged -= handler;
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Guid id && 
                DataContext is UsersViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}

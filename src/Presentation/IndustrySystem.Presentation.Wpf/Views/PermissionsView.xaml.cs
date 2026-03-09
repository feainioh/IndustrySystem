using System;
using System.Windows;
using System.Windows.Controls;
using IndustrySystem.Application.Contracts.Dtos;
using IndustrySystem.Presentation.Wpf.Resources;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class PermissionsView : UserControl
    {
        public PermissionsView(ViewModels.PermissionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.PermissionsViewModel vm && 
                NameBox != null && DisplayNameBox != null && GroupNameBox != null)
            {
                _ = vm.AddAsync(NameBox.Text, DisplayNameBox.Text, GroupNameBox.Text);
                NameBox.Clear();
                DisplayNameBox.Clear();
                GroupNameBox.Clear();
            }
        }

        private async void OnEdit(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is FrameworkElement element)
                {
                    if (TryGetGuid(element.Tag, out var id))
                    {
                        await OpenPermissionDialogAsync(id);
                        return;
                    }

                    if (element.DataContext is PermissionDto permission)
                    {
                        await OpenPermissionDialogAsync(permission.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{Strings.Msg_ErrorTitle}: {ex.Message}", Strings.Msg_ErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool TryGetGuid(object? value, out Guid id)
        {
            switch (value)
            {
                case Guid guid:
                    id = guid;
                    return true;
                case string text when Guid.TryParse(text, out var parsed):
                    id = parsed;
                    return true;
                default:
                    id = Guid.Empty;
                    return false;
            }
        }

        private async System.Threading.Tasks.Task OpenPermissionDialogAsync(Guid? id)
        {
            var dialogVm = ContainerLocator.Current.Resolve<PermissionEditDialogViewModel>();
            await dialogVm.LoadAsync(id);
            var dialog = new Dialogs.PermissionEditDialog { DataContext = dialogVm };

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
                if (result is bool saved && saved && DataContext is ViewModels.PermissionsViewModel vm)
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
                DataContext is ViewModels.PermissionsViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}


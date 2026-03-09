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
    public partial class RoleManageView : UserControl
    {
        public RoleManageView(RoleManageViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
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
                _ = OpenRoleDialogAsync(id);
            }
        }

        private async System.Threading.Tasks.Task OpenRoleDialogAsync(Guid? id)
        {
            var dialogVm = ContainerLocator.Current.Resolve<RoleEditDialogViewModel>();
            await dialogVm.LoadAsync(id);
            var dialog = new Dialogs.RoleEditDialog { DataContext = dialogVm };

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
                if (result is bool saved && saved && DataContext is RoleManageViewModel vm)
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
                DataContext is RoleManageViewModel vm)
            {
                _ = vm.DeleteAsync(id);
            }
        }
    }
}


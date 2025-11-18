using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RoleManageView : UserControl
    {
        public RoleManageView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<RoleManageViewModel>();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            _ = OpenRoleDialogAsync(null);
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.RoleDto r)
            {
                _ = OpenRoleDialogAsync(r.Id);
            }
        }

        private async System.Threading.Tasks.Task OpenRoleDialogAsync(Guid? id)
        {
            var vm = ContainerLocator.Current.Resolve<RoleEditDialogViewModel>();
            await vm.LoadAsync(id);
            var dialog = new Dialogs.RoleEditDialog { DataContext = vm };

            System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModels.DialogViewModel.DialogResult))
                {
                    DialogHost.Close("RootDialogHost", vm.DialogResult);
                }
            };
            vm.PropertyChanged += handler;
            try
            {
                var result = await DialogHost.Show(dialog, "RootDialogHost");
                if (DataContext is ViewModels.RoleManageViewModel listVm) await listVm.LoadAsync();
            }
            finally
            {
                vm.PropertyChanged -= handler;
            }
        }
    }
}

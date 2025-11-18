using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

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
            _ = OpenPermissionDialogAsync(null);
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.PermissionDto p)
            {
                _ = OpenPermissionDialogAsync(p.Id);
            }
        }

        private async System.Threading.Tasks.Task OpenPermissionDialogAsync(Guid? id)
        {
            var vm = ContainerLocator.Current.Resolve<PermissionEditDialogViewModel>();
            await vm.LoadAsync(id);
            var dialog = new Dialogs.PermissionEditDialog { DataContext = vm };

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
                if (DataContext is ViewModels.PermissionsViewModel listVm) await listVm.LoadAsync();
            }
            finally
            {
                vm.PropertyChanged -= handler;
            }
        }
    }
}

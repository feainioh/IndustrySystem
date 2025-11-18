using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            // Manual wiring since AutoWireViewModel is false
            DataContext = ContainerLocator.Current.Resolve<UsersViewModel>();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            _ = OpenUserDialogAsync(null);
        }

        private async System.Threading.Tasks.Task OpenUserDialogAsync(Guid? id)
        {
            var vm = ContainerLocator.Current.Resolve<UserEditDialogViewModel>();
            await vm.LoadAsync(id);
            var dialog = new Dialogs.UserEditDialog { DataContext = vm };

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
                if (DataContext is ViewModels.UsersViewModel listVm) await listVm.LoadAsync();
            }
            finally
            {
                vm.PropertyChanged -= handler;
            }
        }
    }
}

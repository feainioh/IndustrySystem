using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentTemplateView : UserControl
    {
        public ExperimentTemplateView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            _ = OpenTemplateDialogAsync(null);
        }

        private async System.Threading.Tasks.Task OpenTemplateDialogAsync(Guid? id)
        {
            var vm = ContainerLocator.Current.Resolve<ExperimentTemplateEditDialogViewModel>();
            await vm.LoadAsync(id);
            var dialog = new Dialogs.ExperimentTemplateEditDialog { DataContext = vm };

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
                // refresh list after dialog close
                if (DataContext is ViewModels.ExperimentTemplateViewModel listVm)
                    await listVm.LoadAsync();
            }
            finally
            {
                vm.PropertyChanged -= handler;
            }
        }
    }
}

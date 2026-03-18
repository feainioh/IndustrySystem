using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentTemplateView : UserControl
    {
        private ExperimentTemplateViewModel ViewModel => (ExperimentTemplateViewModel)DataContext;

        public ExperimentTemplateView(ExperimentTemplateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            _ = OpenTemplateDialogAsync(null);
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.ExperimentTemplateDto t)
            {
                _ = OpenTemplateDialogAsync(t.Id);
            }
        }

        private async System.Threading.Tasks.Task OpenTemplateDialogAsync(Guid? id)
        {
            var vm = ContainerLocator.Current.Resolve<ExperimentTemplateEditDialogViewModel>();
            await vm.LoadAsync(id);
            var dialog = new Dialogs.ExperimentTemplateEditDialog { DataContext = vm };

            System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
            {
                if (e.PropertyName == nameof(DialogViewModel.DialogResult))
                {
                    DialogHost.Close("RootDialogHost", vm.DialogResult);
                }
            };
            vm.PropertyChanged += handler;
            try
            {
                var result = await DialogHost.Show(dialog, "RootDialogHost");
                if (result is true)
                {
                    await ViewModel.LoadAsync();
                }
            }
            finally
            {
                vm.PropertyChanged -= handler;
            }
        }
    }
}

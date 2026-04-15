using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Dialogs;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentGroupsView : UserControl
    {
        private readonly IDialogService _dialogService;

        public ExperimentGroupsView(ExperimentGroupsViewModel viewModel, IDialogService dialogService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _dialogService = dialogService;
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            OpenGroupDialog(null);
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.ExperimentGroupDto g)
            {
                OpenGroupDialog(g.Id);
            }
        }

        private void OpenGroupDialog(Guid? id)
        {
            var parameters = new DialogParameters { { "id", id } };
            _dialogService.ShowDialog(nameof(Dialogs.ExperimentGroupEditDialog), parameters, async result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    if (DataContext is ExperimentGroupsViewModel vm)
                    {
                        await vm.LoadAsync();
                    }
                }
            });
        }
    }
}

using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Dialogs;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentTemplateView : UserControl
    {
        private readonly IDialogService _dialogService;
        private ExperimentTemplateViewModel ViewModel => (ExperimentTemplateViewModel)DataContext;

        public ExperimentTemplateView(ExperimentTemplateViewModel viewModel, IDialogService dialogService)
        {
            InitializeComponent();
            DataContext = viewModel;
            _dialogService = dialogService;
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object sender, RoutedEventArgs e)
        {
            // Region 仅在 View 进入可视树并触发 Loaded 后才由 Prism 创建，
            // 此处补偿 VM 构造阶段因 region 不存在而跳过的首次导航。
            ViewModel.NavigateToCurrentParameterEditor();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            OpenTemplateDialog(null);
        }

        private void OnEdit(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.ExperimentTemplateDto t)
            {
                OpenTemplateDialog(t.Id);
            }
        }

        private void OpenTemplateDialog(Guid? id)
        {
            var parameters = new DialogParameters { { "id", id } };
            _dialogService.ShowDialog(nameof(Dialogs.ExperimentTemplateEditDialog), parameters, async result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    await ViewModel.LoadAsync();
                }
            });
        }
    }
}

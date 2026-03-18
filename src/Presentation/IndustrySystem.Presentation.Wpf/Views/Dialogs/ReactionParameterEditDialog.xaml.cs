using System.Windows.Controls;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

namespace IndustrySystem.Presentation.Wpf.Views.Dialogs;

public partial class ReactionParameterEditDialog : UserControl
{
    private readonly ExperimentParameterEditorViewModel _vm;
    private readonly ExperimentTemplateViewModel? _parentVm;

    public ReactionParameterEditDialog(IExperimentParameterAppService svc, ExperimentTemplateViewModel parentVm)
    {
        InitializeComponent();
        _vm = new ExperimentParameterEditorViewModel(svc, ExperimentType.Reaction);
        DataContext = _vm;
        _parentVm = parentVm;
        _parentVm.PropertyChanged += OnParentPropertyChanged;

        // 如果父VM已经有选中的参数详情，立即加载
        if (_parentVm.CurrentParameterDetail is { } detail && detail.Type == ExperimentType.Reaction)
            _vm.LoadParameterDetail(detail);
    }

    private void OnParentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExperimentTemplateViewModel.CurrentParameterDetail))
        {
            var detail = _parentVm?.CurrentParameterDetail;
            if (detail is not null && detail.Type == ExperimentType.Reaction)
                _vm.LoadParameterDetail(detail);
        }
    }
}

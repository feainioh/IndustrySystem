using System.Windows.Controls;
using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;
using IndustrySystem.Presentation.Wpf.ViewModels;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

namespace IndustrySystem.Presentation.Wpf.Views.Dialogs;

public partial class CentrifugationParameterEditDialog : UserControl
{
    private readonly ExperimentParameterEditorViewModel _vm;
    private readonly ExperimentTemplateViewModel? _parentVm;

    public CentrifugationParameterEditDialog(IExperimentParameterAppService svc, ExperimentTemplateViewModel parentVm)
    {
        InitializeComponent();
        _vm = new ExperimentParameterEditorViewModel(svc, ExperimentType.Centrifugation);
        DataContext = _vm;
        _parentVm = parentVm;
        _parentVm.PropertyChanged += OnParentPropertyChanged;
        if (_parentVm.CurrentParameterDetail is { } detail && detail.Type == ExperimentType.Centrifugation)
            _vm.LoadParameterDetail(detail);
    }

    private void OnParentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExperimentTemplateViewModel.CurrentParameterDetail))
        {
            var detail = _parentVm?.CurrentParameterDetail;
            if (detail is not null && detail.Type == ExperimentType.Centrifugation)
                _vm.LoadParameterDetail(detail);
        }
    }
}

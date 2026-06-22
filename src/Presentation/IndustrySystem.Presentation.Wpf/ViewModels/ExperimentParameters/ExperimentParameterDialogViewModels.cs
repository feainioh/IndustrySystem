using IndustrySystem.Application.Contracts.Services;
using IndustrySystem.Domain.Shared.Enums;

namespace IndustrySystem.Presentation.Wpf.ViewModels.ExperimentParameters;

/// <summary>
/// 各参数编辑页的专属 ViewModel，避免多个 View 共用同一个类型。
/// </summary>
public sealed class ReactionParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public ReactionParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Reaction)
    {
    }
}

public sealed class RotaryEvaporationParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public RotaryEvaporationParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.RotaryEvaporation)
    {
    }
}

public sealed class DetectionParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public DetectionParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Detection)
    {
    }
}

public sealed class FiltrationParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public FiltrationParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Filtration)
    {
    }
}

public sealed class DryingParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public DryingParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Drying)
    {
    }
}

public sealed class QuenchingParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public QuenchingParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Quenching)
    {
    }
}

public sealed class ExtractionParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public ExtractionParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Extraction)
    {
    }
}

public sealed class SamplingParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public SamplingParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Sampling)
    {
    }
}

public sealed class CentrifugationParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public CentrifugationParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.Centrifugation)
    {
    }
}

public sealed class CustomDetectionParameterEditDialogViewModel : ExperimentParameterEditorViewModelBase
{
    public CustomDetectionParameterEditDialogViewModel(IExperimentParameterAppService svc)
        : base(svc, ExperimentType.CustomDetection)
    {
    }
}

using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ExperimentSelectionItem : BindableBase
{
    public Guid Id { get; init; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

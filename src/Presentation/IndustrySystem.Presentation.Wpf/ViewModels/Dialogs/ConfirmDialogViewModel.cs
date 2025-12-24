using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ConfirmDialogViewModel : DialogViewModel
{
    private string _message = string.Empty;
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    public DelegateCommand OkCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public ConfirmDialogViewModel()
    {
        Title = "х╥хо";
        OkCommand = new DelegateCommand(() => DialogResult = true);
        CancelCommand = new DelegateCommand(() => DialogResult = false);
    }
}

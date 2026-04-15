using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ConfirmDialogViewModel : DialogViewModel
{
    private string _message = string.Empty;
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    public DelegateCommand OkCommand { get; }
    public new DelegateCommand CancelCommand { get; }

    public ConfirmDialogViewModel()
    {
        Title = "确认";
        OkCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));
        CancelCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel)));
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue<string>("message", out var msg))
            Message = msg;
    }
}

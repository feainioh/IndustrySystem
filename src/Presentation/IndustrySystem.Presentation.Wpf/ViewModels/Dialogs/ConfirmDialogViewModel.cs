using Prism.Commands;
using Prism.Dialogs;
using IndustrySystem.Presentation.Wpf.Resources;

namespace IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

public class ConfirmDialogViewModel : DialogViewModel
{
    private string _message = string.Empty;
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    public DelegateCommand OkCommand { get; }
    public new DelegateCommand CancelCommand { get; }

    public ConfirmDialogViewModel()
    {
        Title = LocalizationProvider.Instance["Dialog_Confirm_Title"];
        OkCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));
        CancelCommand = new DelegateCommand(() => RequestClose.Invoke(new DialogResult(ButtonResult.Cancel)));
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        if (parameters.TryGetValue<string>("title", out var title) && !string.IsNullOrWhiteSpace(title))
            Title = title;

        if (parameters.TryGetValue<string>("message", out var msg))
            Message = msg;
    }
}

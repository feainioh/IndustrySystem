using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Dialogs;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class DialogViewModel : BaseViewModel, IDialogAware
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    protected DialogViewModel()
    {
        SaveCommand = new AsyncDelegateCommand(OnSaveAsync, CanSave);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #region IDialogAware

    public DialogCloseListener RequestClose { get; set; }

    public virtual bool CanCloseDialog() => true;

    public virtual void OnDialogClosed() { }

    public virtual void OnDialogOpened(IDialogParameters parameters) { }

    #endregion

    protected void RaiseSaveCanExecuteChanged()
    {
        switch (SaveCommand)
        {
            case AsyncDelegateCommand asyncDelegateCommand:
                asyncDelegateCommand.RaiseCanExecuteChanged();
                break;
            case DelegateCommand delegateCommand:
                delegateCommand.RaiseCanExecuteChanged();
                break;
        }
    }

    protected virtual bool CanSave() => true;

    protected virtual Task OnSaveAsync()
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        return Task.CompletedTask;
    }

    protected virtual void OnCancel()
    {
        RequestClose.Invoke(new DialogResult(ButtonResult.Cancel));
    }
}

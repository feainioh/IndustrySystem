using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class DialogViewModel : BindableBase
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private bool? _dialogResult;
    public bool? DialogResult
    {
        get => _dialogResult;
        protected set => SetProperty(ref _dialogResult, value);
    }

    protected DialogViewModel()
    {
        SaveCommand = new AsyncDelegateCommand(OnSaveAsync, CanSave);
        CancelCommand = new DelegateCommand(OnCancel);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    protected virtual bool CanSave() => true;

    protected virtual Task OnSaveAsync()
    {
        DialogResult = true;
        return Task.CompletedTask;
    }

    protected virtual void OnCancel() => DialogResult = false;
}

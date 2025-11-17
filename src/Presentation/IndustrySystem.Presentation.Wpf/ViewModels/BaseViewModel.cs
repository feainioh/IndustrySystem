using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Commands;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class BaseViewModel : BindableBase
{
    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        protected set
        {
            if (SetProperty(ref _isBusy, value))
            {
                (RefreshCommand as AsyncDelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public ICommand RefreshCommand { get; }

    protected BaseViewModel()
    {
        RefreshCommand = new AsyncDelegateCommand(ExecuteRefreshAsync, CanExecuteRefresh);
    }

    protected virtual bool CanExecuteRefresh() => !IsBusy;

    private async Task ExecuteRefreshAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            await OnRefreshAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected virtual Task OnRefreshAsync() => Task.CompletedTask;
}

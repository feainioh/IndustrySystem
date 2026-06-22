using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace IndustrySystem.MotionDesigner.ViewModels;

/// <summary>
/// Base ViewModel for MotionDesigner pages and controls.
/// Provides IsBusy + RefreshCommand infrastructure.
/// Equivalent to IndustrySystem.Presentation.Wpf.ViewModels.BaseViewModel.
///
/// TODO: Long-term — extract shared ViewModel base classes into a
///       IndustrySystem.Presentation.Shared project to avoid duplication.
/// </summary>
public abstract class MotionDesignerBaseViewModel : BindableBase
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

    protected MotionDesignerBaseViewModel()
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

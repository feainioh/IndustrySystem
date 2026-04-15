using System.Threading.Tasks;
using Prism.Navigation; 

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class NagetiveViewModel : BaseViewModel, INavigationAware
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;

    #region INavigationAware

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = OnNavigatedToAsync();
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _ = OnNavigatedFromAsync();
    }

    #endregion
}

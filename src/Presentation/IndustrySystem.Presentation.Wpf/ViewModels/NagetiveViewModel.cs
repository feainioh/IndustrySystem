using System.Threading.Tasks;

namespace IndustrySystem.Presentation.Wpf.ViewModels;

public abstract class NagetiveViewModel : BaseViewModel
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}

namespace IndustrySystem.Presentation.Wpf.Services;

public interface IDialogService
{
 Task<bool?> ShowAsync(System.Windows.Controls.UserControl view, IndustrySystem.Presentation.Wpf.ViewModels.DialogViewModel vm, string hostId = "RootDialogHost");
}

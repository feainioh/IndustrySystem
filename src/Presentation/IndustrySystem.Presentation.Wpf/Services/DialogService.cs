using System.ComponentModel;
using System.Threading.Tasks;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;

namespace IndustrySystem.Presentation.Wpf.Services;

public class DialogService : IDialogService
{
 public async Task<bool?> ShowAsync(System.Windows.Controls.UserControl view, DialogViewModel vm, string hostId = "RootDialogHost")
 {
 view.DataContext = vm;
 PropertyChangedEventHandler handler = (s,e) =>
 {
 if (e.PropertyName == nameof(DialogViewModel.DialogResult))
 {
 DialogHost.Close(hostId, vm.DialogResult);
 }
 };
 vm.PropertyChanged += handler;
 try
 {
 var result = await DialogHost.Show(view, hostId);
 return result as bool?;
 }
 finally
 {
 vm.PropertyChanged -= handler;
 }
 }
}

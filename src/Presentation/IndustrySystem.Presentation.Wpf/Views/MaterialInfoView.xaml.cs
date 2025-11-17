using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
 public partial class MaterialInfoView : UserControl
 {
 public MaterialInfoView()
 {
 InitializeComponent();
 }

 private void OnAdd(object sender, RoutedEventArgs e)
 {
 _ = OpenMaterialDialogAsync(null);
 }

 private void OnEdit(object sender, RoutedEventArgs e)
 {
 _ = OpenMaterialDialogAsync(Guid.Empty);
 }

 private async System.Threading.Tasks.Task OpenMaterialDialogAsync(Guid? id)
 {
 var vm = ContainerLocator.Current.Resolve<MaterialEditDialogViewModel>();
 await vm.LoadAsync(id);
 var dialog = new Dialogs.MaterialEditDialog { DataContext = vm };

 System.ComponentModel.PropertyChangedEventHandler handler = (s, e) =>
 {
 if (e.PropertyName == nameof(ViewModels.DialogViewModel.DialogResult))
 {
 DialogHost.Close("RootDialogHost", vm.DialogResult);
 }
 };
 vm.PropertyChanged += handler;
 try
 {
 var result = await DialogHost.Show(dialog, "RootDialogHost");
 // TODO: refresh when list VM exists
 }
 finally
 {
 vm.PropertyChanged -= handler;
 }
 }
 }
}

using System;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;
using MaterialDesignThemes.Wpf;

namespace IndustrySystem.Presentation.Wpf.Views
{
 public partial class ExperimentGroupsView : UserControl
 {
 public ExperimentGroupsView()
 {
 InitializeComponent();
 }

 private void OnAdd(object sender, RoutedEventArgs e)
 {
 _ = OpenGroupDialogAsync(null);
 }

 private void OnEdit(object sender, RoutedEventArgs e)
 {
 if ((sender as FrameworkElement)?.DataContext is Application.Contracts.Dtos.ExperimentGroupDto g)
 {
 _ = OpenGroupDialogAsync(g.Id);
 }
 }

 private async System.Threading.Tasks.Task OpenGroupDialogAsync(Guid? id)
 {
 var vm = ContainerLocator.Current.Resolve<ExperimentGroupEditDialogViewModel>();
 await vm.LoadAsync(id);
 var dialog = new Dialogs.ExperimentGroupEditDialog { DataContext = vm };

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
 // TODO: refresh list when viewmodel exists
 }
 finally
 {
 vm.PropertyChanged -= handler;
 }
 }
 }
}

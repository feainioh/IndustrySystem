using System.Windows.Controls;
using IndustrySystem.Presentation.Wpf.ViewModels.Dialogs;

namespace IndustrySystem.Presentation.Wpf.Views.Dialogs;

public partial class UserEditDialog : UserControl
{
    public UserEditDialog()
    {
        InitializeComponent();
        PwdBox.PasswordChanged += (_, _) =>
        {
            if (DataContext is UserEditDialogViewModel vm)
                vm.Password = PwdBox.Password;
        };
    }
}

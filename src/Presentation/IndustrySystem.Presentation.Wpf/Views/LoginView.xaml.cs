using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using System.Windows;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<LoginViewModel>();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Wire PasswordBox to ViewModel.Password since Password is not a DependencyProperty
            if (DataContext is LoginViewModel vm)
            {
                if (this.FindName("Pwd") is PasswordBox pb)
                {
                    pb.PasswordChanged += (s, _) => vm.Password = pb.Password;
                }
            }
        }
    }
}

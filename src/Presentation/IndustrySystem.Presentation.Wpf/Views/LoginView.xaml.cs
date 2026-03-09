using System.Windows.Controls;
using Prism.Ioc;
using IndustrySystem.Presentation.Wpf.ViewModels;
using System.Windows;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class LoginView : UserControl
    {
        private bool _passwordHooked;

        public LoginView()
        {
            InitializeComponent();
            DataContext = ContainerLocator.Current.Resolve<LoginViewModel>();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LoginViewModel vm)
            {
                return;
            }

            if (this.FindName("Pwd") is not PasswordBox pb)
            {
                return;
            }

            if (pb.Password != vm.Password)
            {
                pb.Password = vm.Password ?? string.Empty;
            }

            if (_passwordHooked)
            {
                return;
            }

            pb.PasswordChanged += (s, _) => vm.Password = pb.Password;
            _passwordHooked = true;
        }
    }
}

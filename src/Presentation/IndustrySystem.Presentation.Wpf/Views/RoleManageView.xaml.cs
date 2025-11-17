using System;
using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class RoleManageView : UserControl
    {
        public RoleManageView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.RoleManageViewModel vm && NameBox != null)
            {
                _ = vm.AddAsync(NameBox.Text);
                NameBox.Text = string.Empty;
            }
        }
    }
}

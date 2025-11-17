using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace IndustrySystem.Presentation.Wpf.Views
{
    public partial class ExperimentTemplateView : UserControl
    {
        public ExperimentTemplateView()
        {
            InitializeComponent();
        }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ExperimentTemplateViewModel vm && NameBox != null && DescBox != null)
            {
                _ = vm.AddAsync(NameBox.Text, DescBox.Text);
                NameBox.Text = string.Empty;
                DescBox.Text = string.Empty;
            }
        }
    }
}

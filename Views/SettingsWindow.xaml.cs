using System.Windows;
using HMI_ScrewingMonitor.ViewModels;

namespace HMI_ScrewingMonitor.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel();
        }

        public SettingsWindow(SettingsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}
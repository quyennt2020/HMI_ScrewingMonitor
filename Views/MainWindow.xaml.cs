using System;
using System.Windows;
using HMI_ScrewingMonitor.ViewModels;

namespace HMI_ScrewingMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Window events
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional: Auto-maximize for HMI application
            // this.WindowState = WindowState.Maximized;
            
            // Optional: Remove window chrome for full-screen HMI
            // this.WindowStyle = WindowStyle.None;
            // this.ResizeMode = ResizeMode.NoResize;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cleanup resources
            _viewModel?.Dispose();
        }
    }
}

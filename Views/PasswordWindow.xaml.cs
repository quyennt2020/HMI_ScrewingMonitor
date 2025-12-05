using System.Windows;

namespace HMI_ScrewingMonitor.Views
{
    public partial class PasswordWindow : Window
    {
        public string Password { get; private set; }

        public PasswordWindow()
        {
            InitializeComponent();
            PasswordInput.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PasswordInput.Password;
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

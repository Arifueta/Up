using System.Windows;
using System.Windows.Controls;
using UPApp.ViewModels;

namespace UPApp
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            DataContext = new AuthViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AuthViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
} 
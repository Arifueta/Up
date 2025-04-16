using System.Windows;
using UPApp.Models;
using UPApp.ViewModels;

namespace UPApp.Views
{
    public partial class AdminWindow : Window
    {
        public AdminWindow(User user)
        {
            InitializeComponent();
            DataContext = new AdminViewModel(user);
        }
    }
} 
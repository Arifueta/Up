using System.Windows;
using UPApp.Models;
using UPApp.ViewModels;

namespace UPApp.Views
{
    public partial class BugalterWindow : Window
    {
        public BugalterWindow(User user)
        {
            InitializeComponent();
            DataContext = new BugalterViewModel(user);
        }
    }
} 
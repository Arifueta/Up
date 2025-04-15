using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using UPApp.Models;

namespace UPApp.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
       
        private string _connectionString = "Data Source=DESKTOP-LS8VIPM;Database=UPDB;Integrated Security=True;";
        private string _login;
        private string _password;
        private User _currentUser;

        public AuthViewModel()
        {
            LoginCommand = new RelayCommand(LoginFun, CanLogin);
        }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }

        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password);
        }

        private void LoginFun()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT u.*, r.name as role_name FROM users u " +
                                  "JOIN roletype r ON u.type = r.idroletype " +
                                  "WHERE u.login = @login AND u.password = @password";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@login", Login);
                        command.Parameters.AddWithValue("@password", Password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentUser = new User
                                {
                                    Id = !reader.IsDBNull(reader.GetOrdinal("id")) ? reader.GetInt32(reader.GetOrdinal("id")) : 0,
                                    Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader.GetString(reader.GetOrdinal("name")) : string.Empty,
                                    Login = !reader.IsDBNull(reader.GetOrdinal("login")) ? reader.GetString(reader.GetOrdinal("login")) : string.Empty,
                                    Password = !reader.IsDBNull(reader.GetOrdinal("password")) ? reader.GetString(reader.GetOrdinal("password")) : string.Empty,
                                    Ip = !reader.IsDBNull(reader.GetOrdinal("ip")) ? reader.GetString(reader.GetOrdinal("ip")) : string.Empty,
                                    LastEnter = !reader.IsDBNull(reader.GetOrdinal("lastenter")) ? reader.GetDateTime(reader.GetOrdinal("lastenter")) : DateTime.Now,
                                    Type = !reader.IsDBNull(reader.GetOrdinal("type")) ? reader.GetInt32(reader.GetOrdinal("type")) : 0
                                };

                                var mainWindow = new MainWindow(CurrentUser);
                                mainWindow.Show();
                                CloseWindow();
                            }
                            else
                            {
                                MessageBox.Show("Invalid login or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }
    }
} 
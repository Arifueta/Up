using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using UPApp.DataBase;
using UPApp.Models;
using UPApp.Views;

namespace UPApp.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private ObservableCollection<User> _users;
        static Connection connection = new Connection();
        private string _connectionString = connection.stringconnection;

        public AdminViewModel(User user)
        {
            _currentUser = user;
            LogoutCommand = new RelayCommand(Logout);
            AddUserCommand = new RelayCommand(AddUser);
            LoadUsers();
        }

        public User CurrentUser => _currentUser;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand AddUserCommand { get; }

        private void LoadUsers()
        {
            try
            {
                Users = new ObservableCollection<User>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT u.id, u.name, u.login, u.password, u.ip, u.lastenter, u.type, r.name as role_name 
                                   FROM users u 
                                   LEFT JOIN roletype r ON u.type = r.idroletype
                                   ORDER BY u.id";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                try
                                {
                                    var user = new User
                                    {
                                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                                        Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader.GetString(reader.GetOrdinal("name")) : string.Empty,
                                        Login = !reader.IsDBNull(reader.GetOrdinal("login")) ? reader.GetString(reader.GetOrdinal("login")) : string.Empty,
                                        Password = !reader.IsDBNull(reader.GetOrdinal("password")) ? reader.GetString(reader.GetOrdinal("password")) : string.Empty,
                                        Type = !reader.IsDBNull(reader.GetOrdinal("type")) ? reader.GetInt32(reader.GetOrdinal("type")) : 0,
                                        RoleName = !reader.IsDBNull(reader.GetOrdinal("role_name")) ? reader.GetString(reader.GetOrdinal("role_name")) : "Unknown",
                                        LastEnter = !reader.IsDBNull(reader.GetOrdinal("lastenter")) ? reader.GetDateTime(reader.GetOrdinal("lastenter")) : DateTime.Now,
                                        Ip = !reader.IsDBNull(reader.GetOrdinal("ip")) ? reader.GetString(reader.GetOrdinal("ip")) : string.Empty
                                    };
                                    Users.Add(user);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error processing user data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUser()
        {
            // Здесь можно добавить логику для открытия окна добавления нового пользователя
            MessageBox.Show("Функция добавления пользователя будет реализована позже", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Logout()
        {
            var authWindow = new AuthWindow();
            authWindow.Show();
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
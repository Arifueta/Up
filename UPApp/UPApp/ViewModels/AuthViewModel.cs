using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using UPApp.DataBase;
using UPApp.Models;
using UPApp.Views;

namespace UPApp.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        static Connection connection = new Connection();
        private string _connectionString = connection.stringconnection;
        private string _login;
        private string _password;
        private string _captchaInput;
        private User _currentUser;
        private bool _isCaptchaVisible;
        private bool _isLoginEnabled = true;
        private CaptchaControl _captchaControl;
        private int _failedAttempts;
        private DispatcherTimer _lockoutTimer;

        public AuthViewModel()
        {
            LoginCommand = new RelayCommand(LoginFun, CanLogin);
            RegenerateCaptchaCommand = new RelayCommand(RegenerateCaptcha, () => IsCaptchaVisible);
            _captchaControl = new CaptchaControl();
            _lockoutTimer = new DispatcherTimer();
            _lockoutTimer.Interval = TimeSpan.FromSeconds(10);
            _lockoutTimer.Tick += (s, e) => 
            {
                IsLoginEnabled = true;
                _lockoutTimer.Stop();
            };
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

        public string CaptchaInput
        {
            get => _captchaInput;
            set
            {
                _captchaInput = value;
                OnPropertyChanged();
            }
        }

        public bool IsCaptchaVisible
        {
            get => _isCaptchaVisible;
            set
            {
                _isCaptchaVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoginEnabled
        {
            get => _isLoginEnabled;
            set
            {
                _isLoginEnabled = value;
                OnPropertyChanged();
            }
        }

        public CaptchaControl CaptchaControl
        {
            get => _captchaControl;
            set
            {
                _captchaControl = value;
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
        public ICommand RegenerateCaptchaCommand { get; }

        private bool CanLogin()
        {
            if (!IsLoginEnabled) return false;
            
            bool hasRequiredFields = !string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password);
            if (!IsCaptchaVisible) return hasRequiredFields;
            
            return hasRequiredFields && !string.IsNullOrEmpty(CaptchaInput);
        }

        private void RegenerateCaptcha()
        {
            CaptchaControl.GenerateNewCaptcha();
            CaptchaInput = string.Empty;
        }

        private void LoginFun()
        {
            try
            {
                if (!ValidateLogin())
                {
                    HandleFailedLogin();
                    return;
                }

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
                                HandleSuccessfulLogin(reader);
                            }
                            else
                            {
                                HandleFailedLogin();
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

        private bool ValidateLogin()
        {
            if (!IsCaptchaVisible) return true;
            
            return string.Equals(CaptchaInput, CaptchaControl.CaptchaText, StringComparison.OrdinalIgnoreCase);
        }

        private void HandleSuccessfulLogin(SqlDataReader reader)
        {
            CurrentUser = new User
            {
                Id = !reader.IsDBNull(reader.GetOrdinal("id")) ? reader.GetInt32(reader.GetOrdinal("id")) : 0,
                Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader.GetString(reader.GetOrdinal("name")) : string.Empty,
                Login = !reader.IsDBNull(reader.GetOrdinal("login")) ? reader.GetString(reader.GetOrdinal("login")) : string.Empty,
                Password = !reader.IsDBNull(reader.GetOrdinal("password")) ? reader.GetString(reader.GetOrdinal("password")) : string.Empty,
                Ip = !reader.IsDBNull(reader.GetOrdinal("ip")) ? reader.GetString(reader.GetOrdinal("ip")) : string.Empty,
                LastEnter = !reader.IsDBNull(reader.GetOrdinal("lastenter")) ? reader.GetDateTime(reader.GetOrdinal("lastenter")) : DateTime.Now,
                Type = !reader.IsDBNull(reader.GetOrdinal("type")) ? reader.GetInt32(reader.GetOrdinal("type")) : 0,
                RoleName = !reader.IsDBNull(reader.GetOrdinal("role_name")) ? reader.GetString(reader.GetOrdinal("role_name")) : string.Empty
            };

            // Обновляем дату последнего входа
            using (SqlConnection updateConn = new SqlConnection(_connectionString))
            {
                updateConn.Open();
                string updateQuery = "UPDATE users SET lastenter = @lastenter WHERE id = @id";
                using (SqlCommand updateCmd = new SqlCommand(updateQuery, updateConn))
                {
                    updateCmd.Parameters.AddWithValue("@lastenter", DateTime.Now);
                    updateCmd.Parameters.AddWithValue("@id", CurrentUser.Id);
                    updateCmd.ExecuteNonQuery();
                }
            }

            _failedAttempts = 0;
            IsCaptchaVisible = false;

            Window windowToShow;
            switch (CurrentUser.Type)
            {
                case 1: // laborant
                    windowToShow = new MainWindow(CurrentUser);
                    break;
                case 2: // bugalter
                    windowToShow = new BugalterWindow(CurrentUser);
                    break;
                case 3: // admin
                    windowToShow = new AdminWindow(CurrentUser);
                    break;
                default:
                    MessageBox.Show("Unknown user role", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            windowToShow.Show();
            CloseWindow();
        }

        private void HandleFailedLogin()
        {
            _failedAttempts++;
            string message = "Invalid login or password";

            if (_failedAttempts == 1)
            {
                IsCaptchaVisible = true;
                RegenerateCaptcha();
                message += ". Please enter the CAPTCHA for your next attempt.";
            }
            else if (_failedAttempts > 1)
            {
                IsLoginEnabled = false;
                _lockoutTimer.Start();
                RegenerateCaptcha();
                message += ". Your account is locked for 10 seconds.";
            }

            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
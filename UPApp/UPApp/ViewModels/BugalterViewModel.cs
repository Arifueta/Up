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
    public class BugalterViewModel : BaseViewModel
    {
        private readonly User _currentUser;
        private ObservableCollection<ServiceReport> _services;
        static Connection connection = new Connection();
        private string _connectionString = connection.stringconnection;
        private DateTime _startDate = DateTime.Now.AddMonths(-1);
        private DateTime _endDate = DateTime.Now;

        public BugalterViewModel(User user)
        {
            _currentUser = user;
            LogoutCommand = new RelayCommand(Logout);
            LoadServicesCommand = new RelayCommand(LoadServices);
            LoadServices();
        }

        public User CurrentUser => _currentUser;

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ServiceReport> Services
        {
            get => _services;
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand LoadServicesCommand { get; }

        private void LoadServices()
        {
            try
            {
                Services = new ObservableCollection<ServiceReport>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            p.fullname AS PatientName,
                            s.Service AS ServiceName,
                            s.Price AS ServicePrice,
                            bs.finished AS ServiceDate,
                            bs.result AS ServiceResult,
                            bs.status AS ServiceStatus,
                            b.barcode AS Barcode
                        FROM bloodservices bs
                        JOIN blood b ON bs.blood = b.id
                        JOIN patients p ON b.patient = p.id
                        JOIN services s ON bs.service = s.Code
                        WHERE bs.finished BETWEEN @startDate AND @endDate
                        ORDER BY bs.finished DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@startDate", StartDate);
                        cmd.Parameters.AddWithValue("@endDate", EndDate);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Services.Add(new ServiceReport
                                {
                                    PatientName = !reader.IsDBNull(reader.GetOrdinal("PatientName")) ? reader.GetString(reader.GetOrdinal("PatientName")) : "Unknown",
                                    ServiceName = !reader.IsDBNull(reader.GetOrdinal("ServiceName")) ? reader.GetString(reader.GetOrdinal("ServiceName")) : "Unknown",
                                    ServicePrice = !reader.IsDBNull(reader.GetOrdinal("ServicePrice")) ? reader.GetDouble(reader.GetOrdinal("ServicePrice")) : 0,
                                    ServiceDate = !reader.IsDBNull(reader.GetOrdinal("ServiceDate")) ? reader.GetDateTime(reader.GetOrdinal("ServiceDate")) : DateTime.MinValue,
                                    ServiceResult = !reader.IsDBNull(reader.GetOrdinal("ServiceResult")) ? reader.GetDouble(reader.GetOrdinal("ServiceResult")) : 0,
                                    ServiceStatus = !reader.IsDBNull(reader.GetOrdinal("ServiceStatus")) ? reader.GetString(reader.GetOrdinal("ServiceStatus")) : "Unknown",
                                    Barcode = !reader.IsDBNull(reader.GetOrdinal("Barcode")) ? reader.GetDouble(reader.GetOrdinal("Barcode")).ToString() : "Unknown"
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

    public class ServiceReport
    {
        public string PatientName { get; set; }
        public string ServiceName { get; set; }
        public double ServicePrice { get; set; }
        public DateTime ServiceDate { get; set; }
        public double ServiceResult { get; set; }
        public string ServiceStatus { get; set; }
        public string Barcode { get; set; }
    }
} 
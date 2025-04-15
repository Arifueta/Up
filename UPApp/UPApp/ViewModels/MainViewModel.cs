using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using UPApp.Models;

namespace UPApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _connectionString = "Data Source=DESKTOP-LS8VIPM;Database=UPDB;Integrated Security=True;";
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Blood> _bloodSamples;
        private ObservableCollection<Service> _services;
        private ObservableCollection<BloodService> _bloodServices;
        private Patient _selectedPatient;
        private Blood _selectedBloodSample;
        private Service _selectedService;
        private BloodService _selectedBloodService;
        private User _currentUser;
        private string _searchText;

        public MainViewModel(User currentUser)
        {
            CurrentUser = currentUser;
            LoadData();
            AddBloodServiceCommand = new RelayCommand(AddBloodService, CanAddBloodService);
            UpdateBloodServiceCommand = new RelayCommand(UpdateBloodService, CanUpdateBloodService);
            DeleteBloodServiceCommand = new RelayCommand(DeleteBloodService, CanDeleteBloodService);
            AddBloodCommand = new RelayCommand(AddBlood, CanAddBlood);
            LogoutCommand = new RelayCommand(Logout);
        }

        public ICommand AddBloodServiceCommand { get; }
        public ICommand UpdateBloodServiceCommand { get; }
        public ICommand DeleteBloodServiceCommand { get; }
        public ICommand AddBloodCommand { get; }
        public ICommand LogoutCommand { get; }

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public ObservableCollection<Blood> BloodSamples
        {
            get => _bloodSamples;
            set => SetProperty(ref _bloodSamples, value);
        }

        public ObservableCollection<Service> Services
        {
            get => _services;
            set => SetProperty(ref _services, value);
        }

        public ObservableCollection<BloodService> BloodServices
        {
            get => _bloodServices;
            set => SetProperty(ref _bloodServices, value);
        }

        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    LoadBloodSamples();
                }
            }
        }

        public Blood SelectedBloodSample
        {
            get => _selectedBloodSample;
            set
            {
                if (SetProperty(ref _selectedBloodSample, value))
                {
                    LoadBloodServicesForSample();
                }
            }
        }

        public Service SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }

        public BloodService SelectedBloodService
        {
            get => _selectedBloodService;
            set => SetProperty(ref _selectedBloodService, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    SearchPatients();
                }
            }
        }

        private void LoadData()
        {
            LoadPatients();
            LoadServices();
        }

        private void LoadPatients()
        {
            try
            {
                Patients = new ObservableCollection<Patient>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM patients";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Patients.Add(new Patient
                                {
                                    Id = !reader.IsDBNull(reader.GetOrdinal("id")) ? reader.GetInt32(reader.GetOrdinal("id")) : 0,
                                    FullName = !reader.IsDBNull(reader.GetOrdinal("fullname")) ? reader.GetString(reader.GetOrdinal("fullname")) : string.Empty,
                                    Login = !reader.IsDBNull(reader.GetOrdinal("login")) ? reader.GetString(reader.GetOrdinal("login")) : string.Empty,
                                    Password = !reader.IsDBNull(reader.GetOrdinal("pwd")) ? reader.GetString(reader.GetOrdinal("pwd")) : string.Empty,
                                    Email = !reader.IsDBNull(reader.GetOrdinal("email")) ? reader.GetString(reader.GetOrdinal("email")) : string.Empty,
                                    SocialSecNumber = !reader.IsDBNull(reader.GetOrdinal("social_sec_number")) ? (double?)reader.GetDouble(reader.GetOrdinal("social_sec_number")) : null,
                                    Phone = !reader.IsDBNull(reader.GetOrdinal("phone")) ? reader.GetString(reader.GetOrdinal("phone")) : string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBloodSamples()
        {
            if (SelectedPatient == null) return;

            try
            {
                BloodSamples = new ObservableCollection<Blood>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM blood WHERE patient = @patientId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@patientId", SelectedPatient.Id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BloodSamples.Add(new Blood
                                {
                                    Id = !reader.IsDBNull(reader.GetOrdinal("id")) ? reader.GetInt32(reader.GetOrdinal("id")) : 0,
                                    PatientId = !reader.IsDBNull(reader.GetOrdinal("patient")) ? (int?)reader.GetInt32(reader.GetOrdinal("patient")) : null,
                                    Barcode = !reader.IsDBNull(reader.GetOrdinal("barcode")) ? (double?)reader.GetDouble(reader.GetOrdinal("barcode")) : null,
                                    Date = !reader.IsDBNull(reader.GetOrdinal("date")) ? (DateTime?)reader.GetDateTime(reader.GetOrdinal("date")) : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading blood samples: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBloodServicesForSample()
        {
            if (SelectedBloodSample == null) return;

            try
            {
                BloodServices = new ObservableCollection<BloodService>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT bs.*, s.Service as ServiceName " +
                                 "FROM bloodservices bs " +
                                 "JOIN services s ON bs.service = s.Code " +
                                 "WHERE bs.blood = @bloodId";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@bloodId", SelectedBloodSample.Id);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                BloodServices.Add(new BloodService
                                {
                                    BloodId = !reader.IsDBNull(reader.GetOrdinal("blood")) ? reader.GetInt32(reader.GetOrdinal("blood")) : 0,
                                    ServiceId = !reader.IsDBNull(reader.GetOrdinal("service")) ? reader.GetInt32(reader.GetOrdinal("service")) : 0,
                                    Result = !reader.IsDBNull(reader.GetOrdinal("result")) ? (double?)reader.GetDouble(reader.GetOrdinal("result")) : null,
                                    Finished = !reader.IsDBNull(reader.GetOrdinal("finished")) ? (DateTime?)reader.GetDateTime(reader.GetOrdinal("finished")) : null,
                                    Accepted = !reader.IsDBNull(reader.GetOrdinal("accepted")) ? reader.GetBoolean(reader.GetOrdinal("accepted")) : false,
                                    Status = !reader.IsDBNull(reader.GetOrdinal("status")) ? reader.GetString(reader.GetOrdinal("status")) : string.Empty,
                                    Analyzer = !reader.IsDBNull(reader.GetOrdinal("analyzer")) ? reader.GetString(reader.GetOrdinal("analyzer")) : string.Empty,
                                    UserId = !reader.IsDBNull(reader.GetOrdinal("user")) ? reader.GetInt32(reader.GetOrdinal("user")) : 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading blood services: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                Services = new ObservableCollection<Service>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM services";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Services.Add(new Service
                                {
                                    Code = !reader.IsDBNull(reader.GetOrdinal("Code")) ? reader.GetInt32(reader.GetOrdinal("Code")) : 0,
                                    Name = !reader.IsDBNull(reader.GetOrdinal("Service")) ? reader.GetString(reader.GetOrdinal("Service")) : string.Empty,
                                    Price = !reader.IsDBNull(reader.GetOrdinal("Price")) ? reader.GetDouble(reader.GetOrdinal("Price")) : 0.0
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

        private bool CanAddBloodService()
        {
            return SelectedBloodSample != null && SelectedService != null && CurrentUser != null;
        }

        private void AddBloodService()
        {
            try
            {
                if (!CanAddBloodService()) return;

                var dialog = new BloodServiceDialog();
                var viewModel = new BloodServiceDialogViewModel(new BloodService
                {
                    BloodId = SelectedBloodSample.Id,
                    ServiceId = SelectedService.Code,
                    UserId = CurrentUser.Id,
                    Status = "Pending",
                    Accepted = false
                }, dialog);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = @"INSERT INTO bloodservices (blood, service, result, finished, accepted, status, analyzer, [user]) 
                                       VALUES (@bloodId, @serviceId, @result, @finished, @accepted, @status, @analyzer, @userId)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@bloodId", viewModel.BloodService.BloodId);
                            command.Parameters.AddWithValue("@serviceId", viewModel.BloodService.ServiceId);
                            command.Parameters.AddWithValue("@result", viewModel.BloodService.Result ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@finished", viewModel.BloodService.Finished ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@accepted", viewModel.BloodService.Accepted);
                            command.Parameters.AddWithValue("@status", viewModel.BloodService.Status);
                            command.Parameters.AddWithValue("@analyzer", viewModel.BloodService.Analyzer ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@userId", viewModel.BloodService.UserId);

                            command.ExecuteNonQuery();
                        }
                    }

                    LoadBloodServicesForSample();
                    MessageBox.Show("Blood service added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding blood service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanUpdateBloodService()
        {
            return SelectedBloodService != null && CurrentUser != null;
        }

        private void UpdateBloodService()
        {
            try
            {
                if (!CanUpdateBloodService()) return;

                var dialog = new BloodServiceDialog();
                var viewModel = new BloodServiceDialogViewModel(SelectedBloodService, dialog);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = @"UPDATE bloodservices 
                                       SET result = @result, 
                                           finished = @finished, 
                                           accepted = @accepted, 
                                           status = @status, 
                                           analyzer = @analyzer, 
                                           [user] = @userId 
                                       WHERE blood = @bloodId AND service = @serviceId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@bloodId", viewModel.BloodService.BloodId);
                            command.Parameters.AddWithValue("@serviceId", viewModel.BloodService.ServiceId);
                            command.Parameters.AddWithValue("@result", viewModel.BloodService.Result ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@finished", viewModel.BloodService.Finished ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@accepted", viewModel.BloodService.Accepted);
                            command.Parameters.AddWithValue("@status", viewModel.BloodService.Status);
                            command.Parameters.AddWithValue("@analyzer", viewModel.BloodService.Analyzer ?? (object)DBNull.Value);
                            command.Parameters.AddWithValue("@userId", viewModel.BloodService.UserId);

                            command.ExecuteNonQuery();
                        }
                    }

                    LoadBloodServicesForSample();
                    MessageBox.Show("Blood service updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating blood service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteBloodService()
        {
            return SelectedBloodService != null && CurrentUser != null;
        }

        private void DeleteBloodService()
        {
            try
            {
                if (!CanDeleteBloodService()) return;

                var result = MessageBox.Show("Are you sure you want to delete this blood service?", 
                                           "Confirm Delete", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM bloodservices WHERE blood = @bloodId AND service = @serviceId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@bloodId", SelectedBloodService.BloodId);
                            command.Parameters.AddWithValue("@serviceId", SelectedBloodService.ServiceId);

                            command.ExecuteNonQuery();
                        }
                    }

                    LoadBloodServicesForSample();
                    MessageBox.Show("Blood service deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting blood service: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchPatients()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadPatients();
                    return;
                }

                Patients = new ObservableCollection<Patient>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT * FROM patients 
                                   WHERE fullname LIKE @searchText 
                                   OR login LIKE @searchText 
                                   OR email LIKE @searchText 
                                   OR phone LIKE @searchText";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@searchText", $"%{SearchText}%");
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Patients.Add(new Patient
                                {
                                    Id = !reader.IsDBNull(reader.GetOrdinal("id")) ? reader.GetInt32(reader.GetOrdinal("id")) : 0,
                                    FullName = !reader.IsDBNull(reader.GetOrdinal("fullname")) ? reader.GetString(reader.GetOrdinal("fullname")) : string.Empty,
                                    Login = !reader.IsDBNull(reader.GetOrdinal("login")) ? reader.GetString(reader.GetOrdinal("login")) : string.Empty,
                                    Password = !reader.IsDBNull(reader.GetOrdinal("pwd")) ? reader.GetString(reader.GetOrdinal("pwd")) : string.Empty,
                                    Email = !reader.IsDBNull(reader.GetOrdinal("email")) ? reader.GetString(reader.GetOrdinal("email")) : string.Empty,
                                    SocialSecNumber = !reader.IsDBNull(reader.GetOrdinal("social_sec_number")) ? (double?)reader.GetDouble(reader.GetOrdinal("social_sec_number")) : null,
                                    Phone = !reader.IsDBNull(reader.GetOrdinal("phone")) ? reader.GetString(reader.GetOrdinal("phone")) : string.Empty
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching patients: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddBlood()
        {
            return SelectedPatient != null;
        }

        private void AddBlood()
        {
            try
            {
                var dialog = new BloodDialog();
                var viewModel = new BloodDialogViewModel(new Blood
                {
                    PatientId = SelectedPatient.Id,
                    Date = DateTime.Now
                }, dialog);
                dialog.DataContext = viewModel;

                if (dialog.ShowDialog() == true && viewModel.Blood.Barcode.HasValue)
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = @"INSERT INTO blood (patient, barcode, date) 
                                       VALUES (@patientId, @barcode, @date)";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@patientId", viewModel.Blood.PatientId);
                            command.Parameters.AddWithValue("@barcode", viewModel.Blood.Barcode.Value);
                            command.Parameters.AddWithValue("@date", viewModel.Blood.Date ?? (object)DBNull.Value);

                            command.ExecuteNonQuery();
                        }
                    }

                    LoadBloodSamples();
                    MessageBox.Show("Blood sample added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding blood sample: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", 
                                       "Confirm Logout", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var authWindow = new AuthWindow();
                authWindow.Show();
                CloseWindow();
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
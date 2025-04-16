using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using UPApp.Models;

namespace UPApp.ViewModels
{
    public class BloodServiceDialogViewModel : BaseViewModel
    {
        private string _connectionString = "Data Source=DESKTOP-LS8VIPM;Database=UPDB;Integrated Security=True;";
        private BloodService _bloodService;
        private ObservableCollection<string> _analyzers;
        private ObservableCollection<string> _statuses;
        BloodServiceDialog dialog;
        public BloodServiceDialogViewModel(BloodService bloodService, BloodServiceDialog dialog)
        {
            this.dialog = dialog;
            BloodService = bloodService;
            LoadAnalyzers();
            LoadStatuses();
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }
        //public BloodServiceDialogViewModel(BloodService bloodService)
        //{
        //    BloodService = bloodService;
        //    LoadAnalyzers();
        //    LoadStatuses();
        //    SaveCommand = new RelayCommand(Save, CanSave);
        //    CancelCommand = new RelayCommand(Cancel);
        //}
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public BloodService BloodService
        {
            get => _bloodService;
            set => SetProperty(ref _bloodService, value);
        }

        public ObservableCollection<string> Analyzers
        {
            get => _analyzers;
            set => SetProperty(ref _analyzers, value);
        }

        public ObservableCollection<string> Statuses
        {
            get => _statuses;
            set => SetProperty(ref _statuses, value);
        }

        private void LoadAnalyzers()
        {
            try
            {
                Analyzers = new ObservableCollection<string>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT analyzer FROM analyzer";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Analyzers.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analyzers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadStatuses()
        {
            try
            {
                Statuses = new ObservableCollection<string>();
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT status FROM status";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Statuses.Add(reader.GetString(0));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statuses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave()
        {
            return BloodService != null &&
                   !string.IsNullOrWhiteSpace(BloodService.Status) &&
                   !string.IsNullOrWhiteSpace(BloodService.Analyzer);
        }

        private void Save()
        {
            if (Application.Current.MainWindow is Window window)
            {
                dialog.DialogResult = true;
                dialog.Close();
            }
        }

        private void Cancel()
        {
            if (Application.Current.MainWindow is Window window)
            {
                dialog.DialogResult = false;
                dialog.Close();
            }
        }
    }
} 
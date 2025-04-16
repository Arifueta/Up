using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Input;
using UPApp.Models;

namespace UPApp.ViewModels
{
    public class BloodDialogViewModel : BaseViewModel
    {
        private readonly string _connectionString = "Data Source=DESKTOP-LS8VIPM;Database=UPDB;Integrated Security=True;";
        private Blood _blood;
        private BloodDialog _dialog;
        private Random _random;

        public BloodDialogViewModel(Blood blood, BloodDialog dialog)
        {
            Blood = blood;
            _dialog = dialog;
            _random = new Random();
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            GenerateBarcodeCommand = new RelayCommand(GenerateBarcode);
            
            // Generate initial barcode
            GenerateBarcode();
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand GenerateBarcodeCommand { get; }

        public Blood Blood
        {
            get => _blood;
            set => SetProperty(ref _blood, value);
        }

        private bool CanSave()
        {
            return Blood != null && Blood.PatientId.HasValue && Blood.Barcode.HasValue;
        }

        private void Save()
        {
            _dialog.DialogResult = true;
            _dialog.Close();
        }

        private void Cancel()
        {
            _dialog.DialogResult = false;
            _dialog.Close();
        }

        private void GenerateBarcode()
        {
            try
            {
                double barcode;
                bool isUnique = false;

                do
                {
                    // Генерируем 7-значное число
                    barcode = _random.Next(1000000, 10000000);

                    // Проверяем уникальность
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = "SELECT COUNT(*) FROM blood WHERE barcode = @barcode";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@barcode", barcode);
                            int count = (int)command.ExecuteScalar();
                            isUnique = count == 0;
                        }
                    }
                } while (!isUnique);

                // Устанавливаем сгенерированный штрих-код
                Blood.Barcode = barcode;
                OnPropertyChanged(nameof(Blood));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating barcode: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 
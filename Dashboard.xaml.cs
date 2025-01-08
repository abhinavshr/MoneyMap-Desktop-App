using System;
using System.Linq;
using System.Collections.ObjectModel;
using MoneyMap.Data;
using Microsoft.Data.Sqlite;

namespace MoneyMap
{
    public partial class DashboardPage : ContentPage
    {
        private static readonly string dbPath = Path.Combine(FileSystem.AppDataDirectory, "moneyapp.db");

        public DashboardPage()
        {
            InitializeComponent();

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadDashboardDataAsync();
        }

        private async void TransactionPageOnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionPage());
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                // Fetch metrics
                var totalInflows = await DatabaseHelper.GetTotalInflowAsync();
                var totalOutflows = await DatabaseHelper.GetTotalOutflowAsync();
                var totalDebtLeft = await DatabaseHelper.GetTotalDebtLeftAsync();
                var totalDebtPaid = await DatabaseHelper.GetTotalClearedAmountAsync();
                var totalnumberoftransaction = DatabaseHelper.CalculateTotalTransactions();

                // Update labels with fetched data
                TotalInflowsLabel.Text = totalInflows.ToString("C");
                TotalOutflowsLabel.Text = totalOutflows.ToString("C");
                RemainingDebtLabel.Text = totalDebtLeft.ToString("C");
                ClearedDebtLabel.Text = totalDebtPaid.ToString("C");
                TotalTransactionsLabel.Text = (totalInflows - totalOutflows + totalDebtLeft).ToString("C");
                TotalTransactionsCountLabel.Text = totalnumberoftransaction.ToString();


                // Load pending debts
                var pendingDebts = await GetPendingDebtsAsync();
                PendingDebtsList.ItemsSource = pendingDebts;

            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
                await DisplayAlert("Error", "Failed to load dashboard data. Please try again later.", "OK");
            }
        }

        private async Task<ObservableCollection<DebtTrackings>> GetPendingDebtsAsync()
        {
            var pendingDebts = new ObservableCollection<DebtTrackings>();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    // Query to fetch debts with ClearedAmount = 0
                    string query = "SELECT Id, Name, Amount, ClearedAmount, DueDate FROM DebtTracking WHERE ClearedAmount = 0";
                    using (var command = new SqliteCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var debt = new DebtTrackings
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Amount = reader.GetDecimal(2),
                                    ClearedAmount = reader.GetDecimal(3),
                                    DueDate = reader.GetDateTime(4)
                                };
                                pendingDebts.Add(debt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                updateDebtTracking($"Error fetching pending debts: {ex}");
            }

            return pendingDebts;
        }

        private void OnApplyFilterClicked(object sender, EventArgs e)
        {
            var startDate = StartDatePicker.Date;
            var endDate = EndDatePicker.Date;

            if (startDate > endDate)
            {
                DisplayAlert("Error", "Start date must be earlier than end date.", "OK");
                return;
            }
        }
        private static void updateDebtTracking(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "Upadte_DebtTracking.txt");

            try
            {
                // Ensure the directory exists
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Write the log message to the file
                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                // Fallback for logging failure
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }


    }

}

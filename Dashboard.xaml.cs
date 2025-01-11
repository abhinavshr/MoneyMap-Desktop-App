using System;
using System.Linq;
using System.Collections.ObjectModel;
using MoneyMap.Data;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.ComponentModel;

namespace MoneyMap
{
    public partial class DashboardPage : ContentPage
    {
        private static readonly string dbPath = Path.Combine(FileSystem.AppDataDirectory, "moneyapp.db");

        public ObservableCollection<CashInFlow> RecentCashInflows { get; set; } = new ObservableCollection<CashInFlow>();
        public ObservableCollection<CashOutFlow> RecentCashOutflows { get; set; } = new ObservableCollection<CashOutFlow>();

        private ObservableCollection<DebtTrackings> allPendingDebts = new ObservableCollection<DebtTrackings>();

        private List<CashInFlow> allCashInflows = new List<CashInFlow>();
        private List<CashOutFlow> allCashOutflows = new List<CashOutFlow>();

        public DashboardPage()
        {
            InitializeComponent();
            BindingContext = this;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            LoadDataAsync();
            await LoadDashboardDataAsync();

            allPendingDebts = await GetPendingDebtsAsync();
            allCashInflows = await DatabaseHelper.GetCashInflowsAsync();
            allCashOutflows = await DatabaseHelper.GetCashOutflowsAsync();

            PendingDebtsList.ItemsSource = new ObservableCollection<DebtTrackings>(allPendingDebts);
            CashInflowsCollectionView.ItemsSource = new ObservableCollection<CashInFlow>(allCashInflows);
            CashOutflowsCollectionView.ItemsSource = new ObservableCollection<CashOutFlow>(allCashOutflows);
        }

        private async void TransactionPageOnClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionPage());
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var inflows = await DatabaseHelper.GetCashInflowsAsync();
                var outflows = await DatabaseHelper.GetCashOutflowsAsync();

                if (inflows?.Any() == true)
                {
                    RecentCashInflows.Clear();
                    var recentInflows = inflows.OrderByDescending(i => i.Date).Take(5);
                    foreach (var inflow in recentInflows)
                    {
                        RecentCashInflows.Add(inflow);
                    }
                }

                if (outflows?.Any() == true)
                {
                    RecentCashOutflows.Clear();
                    var recentOutflows = outflows.OrderByDescending(o => o.Date).Take(5);
                    foreach (var outflow in recentOutflows)
                    {
                        RecentCashOutflows.Add(outflow);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private async Task LoadDashboardDataAsync()
        {
            try
            {
                var totalInflows = await DatabaseHelper.GetTotalInflowAsync();
                var totalOutflows = await DatabaseHelper.GetTotalOutflowAsync();
                var totalDebtLeft = await DatabaseHelper.GetTotalDebtLeftAsync();
                var totalDebtPaid = await DatabaseHelper.GetTotalClearedAmountAsync();
                var totalnumberoftransaction = DatabaseHelper.CalculateTotalTransactions();

                TotalInflowsLabel.Text = totalInflows.ToString("C");
                TotalOutflowsLabel.Text = totalOutflows.ToString("C");
                RemainingDebtLabel.Text = totalDebtLeft.ToString("C");
                ClearedDebtLabel.Text = totalDebtPaid.ToString("C");
                TotalTransactionsLabel.Text = (totalInflows - totalOutflows + totalDebtLeft).ToString("C");
                TotalTransactionsCountLabel.Text = totalnumberoftransaction.ToString();


                var pendingDebts = await GetPendingDebtsAsync();
                PendingDebtsList.ItemsSource = pendingDebts;

            }
            catch (Exception ex)
            {
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
                DisplayAlert("Error", "Start date must be earlier than or equal to the end date.", "OK");
                return;
            }

            try
            {
                var filteredPendingDebts = allPendingDebts
                    .Where(debt => debt.DueDate >= startDate && debt.DueDate <= endDate)
                    .ToList();

                PendingDebtsList.ItemsSource = new ObservableCollection<DebtTrackings>(filteredPendingDebts);

                var filteredCashInflows = allCashInflows
                    .Where(inflow => inflow.Date >= startDate && inflow.Date <= endDate)
                    .ToList();

                CashInflowsCollectionView.ItemsSource = new ObservableCollection<CashInFlow>(filteredCashInflows);

                var filteredCashOutflows = allCashOutflows
                    .Where(outflow => outflow.Date >= startDate && outflow.Date <= endDate)
                    .ToList();

                CashOutflowsCollectionView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredCashOutflows);

                DisplayAlert("Filter Applied", $"Filtered data from {startDate:MM/dd/yyyy} to {endDate:MM/dd/yyyy}.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during filtering: {ex.Message}");
                DisplayAlert("Error", "An error occurred while applying the filter. Please try again.", "OK");
            }
        }


        private static void updateDebtTracking(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "Upadte_DebtTracking.txt");

            try
            {
                string directoryPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logging failed: {ex.Message}");
            }
        }


    }

}

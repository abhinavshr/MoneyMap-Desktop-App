using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Maui.Controls;
using MoneyMap.Data;

namespace MoneyMap
{
    public partial class TransactionPage : ContentPage
    {
        private static readonly string DbPath = Path.Combine(FileSystem.AppDataDirectory, "moneyapp.db");

        public ObservableCollection<CashInFlow> CashInflows { get; set; } = new();
        public ObservableCollection<CashOutFlow> CashOutflows { get; set; } = new();
        public ObservableCollection<DebtTrackings> DebtTracking { get; set; } = new();

        public TransactionPage()
        {
            InitializeComponent();
            LoadDataAsync();
            BindingContext = this;
        }

        private async void LoadDataAsync()
        {
            try
            {
                var inflows = await DatabaseHelper.GetCashInflowsAsync();
                var outflows = await DatabaseHelper.GetCashOutflowsAsync();
                var debtTrackings = await DatabaseHelper.GetDebtTrackingAsync();

                if (inflows?.Any() == true)
                {
                    CashInflows.Clear();
                    var lastInflows = inflows.OrderByDescending(i => i.Id).Take(3); // Assuming Id determines the order
                    foreach (var inflow in lastInflows)
                    {
                        CashInflows.Add(inflow);
                    }
                }

                if (outflows?.Any() == true)
                {
                    CashOutflows.Clear();
                    var lastOutflows = outflows.OrderByDescending(o => o.Id).Take(3); // Assuming Id determines the order
                    foreach (var outflow in lastOutflows)
                    {
                        CashOutflows.Add(outflow);
                    }
                }

                if (debtTrackings?.Any() == true)
                {
                    DebtTracking.Clear();
                    var lastDebts = debtTrackings.OrderByDescending(d => d.Id).Take(3); // Assuming Id determines the order
                    foreach (var debt in lastDebts)
                    {
                        DebtTracking.Add(debt);
                    }
                }

                LogMessage($"Data Loaded: Inflows={CashInflows.Count}, Outflows={CashOutflows.Count}, DebtTrackings={DebtTracking.Count}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading data: {ex.Message}");
                await DisplayAlert("Error", "Failed to load data.", "OK");
            }
        }


        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue;

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Filter CashInflows
                var filteredInflows = CashInflows.Where(c => c.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                CashInflowsListView.ItemsSource = new ObservableCollection<CashInFlow>(filteredInflows);

                // Filter CashOutflows
                var filteredOutflows = CashOutflows.Where(c => c.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                CashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredOutflows);

                // Filter DebtTracking
                var filteredDebts = DebtTracking.Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                DebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredDebts);
            }
            else
            {
                // Reset to original lists when search is cleared
                CashInflowsListView.ItemsSource = CashInflows;
                CashOutflowsListView.ItemsSource = CashOutflows;
                DebtTrackingListView.ItemsSource = DebtTracking;
            }
        }

        private void OnDateFilterChanged(object sender, DateChangedEventArgs e)
        {
            ApplyDateFilter();
        }

        private void ApplyDateFilter()
        {
            var fromDate = FromDatePicker.Date;
            var toDate = ToDatePicker.Date;

            // Filter and sort inflows by date
            var filteredInflows = CashInflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            CashInflowsListView.ItemsSource = new ObservableCollection<CashInFlow>(filteredInflows);

            // Filter and sort outflows by date
            var filteredOutflows = CashOutflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            CashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredOutflows);

            // Filter and sort debt tracking by due date
            var filteredDebts = DebtTracking
                .Where(d => d.DueDate.Date >= fromDate && d.DueDate.Date <= toDate)
                .OrderBy(d => d.DueDate);
            DebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredDebts);
        }


        private async void OnCashInflowsPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CashInflows());
        }

        private async void OnCashOutflowsPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CashOutflows());
        }

        private async void OnDebtTrackingPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DebtTracking());
        }
        private async void OnDashboardPageClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DashboardPage());
        }

        private static void LogMessage(string message)
        {
            var logFilePath = Path.Combine("D:\\DotNetError", "transactionerror.txt");

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
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

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

        private System.Timers.Timer _refreshTimer;

        public ObservableCollection<CashInFlow> CashInflows { get; set; } = new();
        public ObservableCollection<CashOutFlow> CashOutflows { get; set; } = new();
        public ObservableCollection<DebtTrackings> DebtTracking { get; set; } = new();
        public ObservableCollection<CashInFlow> LowestCashInflows { get; set; } = new();
        public ObservableCollection<CashOutFlow> LowestCashOutflows { get; set; } = new();
        public ObservableCollection<DebtTrackings> LowestDebtTracking { get; set; } = new();


        public TransactionPage()
        {
            InitializeComponent();
            LoadDataAsync();
            InitializeAutoRefresh();
            BindingContext = this;
        }

        private void InitializeAutoRefresh()
        {
            _refreshTimer = new System.Timers.Timer
            {
                Interval = 60000, // 1 minute in milliseconds
                AutoReset = true,
                Enabled = true
            };
            _refreshTimer.Elapsed += async (s, e) => await LoadDataAsyncWrapper();
        }

        private void StopAutoRefresh()
        {
            if (_refreshTimer != null)
            {
                _refreshTimer.Stop();
                _refreshTimer.Dispose();
                _refreshTimer = null;
            }
        }

        private async Task LoadDataAsyncWrapper()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() => LoadDataAsync());
            }
            catch (Exception ex)
            {
                LogMessage($"Error in LoadDataAsyncWrapper: {ex.Message}");
            }
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
                    // Show top 3 highest inflows
                    CashInflows.Clear();
                    var topInflows = inflows.OrderByDescending(i => i.Amount).Take(3);
                    foreach (var inflow in topInflows)
                    {
                        CashInflows.Add(inflow);
                    }

                    // Show top 3 lowest inflows
                    LowestCashInflows.Clear();
                    var lowestInflows = inflows.OrderBy(i => i.Amount).Take(3);
                    foreach (var inflow in lowestInflows)
                    {
                        LowestCashInflows.Add(inflow);
                    }
                }

                if (outflows?.Any() == true)
                {
                    // Show top 3 highest outflows
                    CashOutflows.Clear();
                    var topOutflows = outflows.OrderByDescending(o => o.Amount).Take(3);
                    foreach (var outflow in topOutflows)
                    {
                        CashOutflows.Add(outflow);
                    }

                    // Show top 3 lowest outflows
                    LowestCashOutflows.Clear();
                    var lowestOutflows = outflows.OrderBy(o => o.Amount).Take(3);
                    foreach (var outflow in lowestOutflows)
                    {
                        LowestCashOutflows.Add(outflow);
                    }
                }

                if (debtTrackings?.Any() == true)
                {
                    // Show top 3 highest debts
                    DebtTracking.Clear();
                    var topDebts = debtTrackings.OrderByDescending(d => d.Amount).Take(3);
                    foreach (var debt in topDebts)
                    {
                        DebtTracking.Add(debt);
                    }

                    // Show top 3 lowest debts
                    LowestDebtTracking.Clear();
                    var lowestDebts = debtTrackings.OrderBy(d => d.Amount).Take(3);
                    foreach (var debt in lowestDebts)
                    {
                        LowestDebtTracking.Add(debt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
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

                // Filter LowestCashInflows
                var filteredLowestInflows = LowestCashInflows.Where(c => c.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                LowestCashInflowsListView.ItemsSource = new ObservableCollection<CashInFlow>(filteredLowestInflows);

                // Filter CashOutflows
                var filteredOutflows = CashOutflows.Where(c => c.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                CashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredOutflows);

                // Filter LowestCashOutflows
                var filteredLowestOutflows = LowestCashOutflows.Where(c => c.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                LowestCashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredLowestOutflows);

                // Filter DebtTracking
                var filteredDebts = DebtTracking.Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                DebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredDebts);

                // Filter LowestDebtTracking
                var filteredLowestDebts = LowestDebtTracking.Where(d => d.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase));
                LowestDebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredLowestDebts);
            }
            else
            {
                // Reset to original lists when search is cleared
                CashInflowsListView.ItemsSource = CashInflows;
                LowestCashInflowsListView.ItemsSource = LowestCashInflows;
                CashOutflowsListView.ItemsSource = CashOutflows;
                LowestCashOutflowsListView.ItemsSource = LowestCashOutflows;
                DebtTrackingListView.ItemsSource = DebtTracking;
                LowestDebtTrackingListView.ItemsSource = LowestDebtTracking;
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

            var filteredInflows = CashInflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            CashInflowsListView.ItemsSource = new ObservableCollection<CashInFlow>(filteredInflows);

            var filteredLowestInflows = LowestCashInflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            LowestCashInflowsListView.ItemsSource = new ObservableCollection<CashInFlow>(filteredLowestInflows);

            var filteredOutflows = CashOutflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            CashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredOutflows);

            var filteredLowestOutflows = LowestCashOutflows
                .Where(c => c.Date.Date >= fromDate && c.Date.Date <= toDate)
                .OrderBy(c => c.Date);
            LowestCashOutflowsListView.ItemsSource = new ObservableCollection<CashOutFlow>(filteredLowestOutflows);

            var filteredDebts = DebtTracking
                .Where(d => d.DueDate.Date >= fromDate && d.DueDate.Date <= toDate)
                .OrderBy(d => d.DueDate);
            DebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredDebts);

            var filteredLowestDebts = LowestDebtTracking
                .Where(d => d.DueDate.Date >= fromDate && d.DueDate.Date <= toDate)
                .OrderBy(d => d.DueDate);
            LowestDebtTrackingListView.ItemsSource = new ObservableCollection<DebtTrackings>(filteredLowestDebts);
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

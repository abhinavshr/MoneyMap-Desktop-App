using System;
using System.Linq;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace MoneyMap
{
    public partial class DashboardPage : ContentPage
    {
        // Sample data
        private ObservableCollection<Transaction> _allTransactions;
        public ObservableCollection<Transaction> FilteredTransactions { get; set; }

        public DashboardPage()
        {
            InitializeComponent();

            // Initialize sample data
            _allTransactions = new ObservableCollection<Transaction>
            {
                new Transaction { Title = "Salary", Amount = 5000, Date = new DateTime(2024, 12, 1), Type = "Inflow" },
                new Transaction { Title = "Groceries", Amount = -200, Date = new DateTime(2024, 12, 3), Type = "Outflow" },
                new Transaction { Title = "Loan Payment", Amount = -1000, Date = new DateTime(2024, 12, 5), Type = "Debt" },
            };

            // FilteredTransactions is initially set to show all transactions
            FilteredTransactions = new ObservableCollection<Transaction>(_allTransactions);
            TransactionsList.ItemsSource = FilteredTransactions;
        }

        // Navigation methods
        private async void OnCashInflowsPageClicked(object sender, EventArgs e)
        {
            // Navigate to Cash Inflows page
            await Navigation.PushAsync(new CashInflows());
        }

        private async void OnCashOutflowsPageClicked(object sender, EventArgs e)
        {
            // Navigate to Cash Outflows page
            await Navigation.PushAsync(new CashOutflows());
        }

        private async void OnDebtTrackingPageClicked(object sender, EventArgs e)
        {
            // Navigate to Debt Tracking page
            await Shell.Current.GoToAsync("DebtTracking");
        }

        // Apply filter based on date range
        private void OnApplyFilterClicked(object sender, EventArgs e)
        {
            var startDate = StartDatePicker.Date;
            var endDate = EndDatePicker.Date;

            if (startDate > endDate)
            {
                DisplayAlert("Error", "Start date must be earlier than end date.", "OK");
                return;
            }

            FilteredTransactions.Clear();
            foreach (var transaction in _allTransactions)
            {
                if (transaction.Date >= startDate && transaction.Date <= endDate)
                {
                    FilteredTransactions.Add(transaction);
                }
            }

            // Update the summary after filtering
            UpdateSummary();
        }

        // Update the metrics (Total Inflows, Outflows, and Remaining Debt)
        private void UpdateSummary()
        {
            var totalInflows = FilteredTransactions.Where(t => t.Type == "Inflow").Sum(t => t.Amount);
            var totalOutflows = FilteredTransactions.Where(t => t.Type == "Outflow").Sum(t => t.Amount);
            var remainingDebt = FilteredTransactions.Where(t => t.Type == "Debt").Sum(t => t.Amount);

            TotalInflowsLabel.Text = $"{totalInflows:C}";
            TotalOutflowsLabel.Text = $"{totalOutflows:C}";
            RemainingDebtLabel.Text = $"{remainingDebt:C}";
        }
    }

    // Transaction class to represent each transaction
    public class Transaction
    {
        public string Title { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}

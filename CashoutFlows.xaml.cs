using System;
using System.Collections.ObjectModel;
using System.Formats.Tar;
using System.Linq;
using Microsoft.Maui.Controls;
using MoneyMap.Data;

namespace MoneyMap
{
    public partial class CashOutflows : ContentPage
    {
        public ObservableCollection<CashOutFlow> CashOutflowsList { get; set; }
        public ObservableCollection<CashOutFlow> FilteredCashOutflowsList { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;

        public CashOutflows()
        {
            InitializeComponent();

            CashOutflowsList = new ObservableCollection<CashOutFlow>();
            FilteredCashOutflowsList = new ObservableCollection<CashOutFlow>();

            BindingContext = this;
            LoadCashOutflows();
        }

        private async void LoadCashOutflows()
        {
            try
            {
                var cashOutflows = await DatabaseHelper.GetCashOutflowsAsync();
                FilteredCashOutflowsList.Clear();

                foreach (var outflow in cashOutflows)
                {
                    FilteredCashOutflowsList.Add(outflow);
                }
                WriteCashOutFlowError_log("Loaded successfully");
            }
            catch (Exception ex)
            {
                WriteCashOutFlowError_log($"Error loading cash outflows: {ex.Message}");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue.ToLower();
            FilteredCashOutflowsList.Clear();

            var allOutflows = DatabaseHelper.GetCashOutflowsAsync().Result; 
            var filtered = allOutflows.Where(o => o.Title.ToLower().Contains(searchText) ||
                                                  o.Note.ToLower().Contains(searchText));

            foreach (var outflow in filtered)
            {
                FilteredCashOutflowsList.Add(outflow);
            }
        }

        private void FilterCashOutflows(string searchText)
        {
            FilteredCashOutflowsList.Clear();

            var filtered = CashOutflowsList.Where(c =>
                (string.IsNullOrEmpty(searchText) || c.Title.ToLower().Contains(searchText)) &&
                c.Date >= StartDate && c.Date <= EndDate);

            foreach (var cashOutflow in filtered)
            {
                FilteredCashOutflowsList.Add(cashOutflow);
            }
        }

        private async void OnAddCashOutflowClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TitleEntry.Text) || string.IsNullOrEmpty(AmountEntry.Text) || string.IsNullOrEmpty(NoteEntry.Text) || string.IsNullOrEmpty(TagsEntry.Text))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out var amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            try
            {
                var totalInflow = await DatabaseHelper.GetTotalInflowAsync(); 
                var totalOutflow = await DatabaseHelper.GetTotalOutflowAsync(); 
                var currentBalance = totalInflow - totalOutflow;

                if (amount > currentBalance)
                {
                    await DisplayAlert("Error", $"Insufficient funds! Current balance: {currentBalance:C}", "OK");
                    return;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to check balance: {ex.Message}", "OK");
                return;
            }

            var newOutflow = new CashOutFlow
            {
                Title = TitleEntry.Text,
                Amount = amount,
                Date = DateTime.Now,
                Note = NoteEntry.Text,
                Tags = TagsEntry.Text
            };

            CashOutflowsList.Add(newOutflow);

            CashOutFlowError.TextColor = Colors.Green;
            CashOutFlowError.Text = "Cash Outflow Added Successfully!";
            CashOutFlowError.IsVisible = true;

            TitleEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
            NoteEntry.Text = string.Empty;
            TagsEntry.Text = string.Empty;

            try
            {
                await DatabaseHelper.InsertCashoutFlow(newOutflow);
                WriteCashOutFlowError_log("Successfully inserted cash outflow into database");
            }
            catch (Exception ex)
            {
                WriteCashOutFlowError_log($"Error inserting cash outflow: {ex.Message}");
            }
        }



        private void OnStartDateChanged(object sender, DateChangedEventArgs e)
        {
            StartDate = e.NewDate;
            FilterCashOutflows(CashOutflowSearchBar.Text);
        }

        private void OnEndDateChanged(object sender, DateChangedEventArgs e)
        {
            EndDate = e.NewDate;
            FilterCashOutflows(CashOutflowSearchBar.Text);
        }

        private async void OnGoToTransactionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionPage());
        }

        private static void WriteCashOutFlowError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_cashoutflow_error.txt");

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

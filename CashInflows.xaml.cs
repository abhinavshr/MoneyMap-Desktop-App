using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using MoneyMap.Data;

namespace MoneyMap
{
    public partial class CashInflows : ContentPage
    {
        public ObservableCollection<CashInFlow> CashInflowsList { get; set; }
        public ObservableCollection<CashInFlow> FilteredCashInflowsList { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today;

        public CashInflows()
        {
            InitializeComponent();
            CashInflowsList = new ObservableCollection<CashInFlow>();
            FilteredCashInflowsList = new ObservableCollection<CashInFlow>(CashInflowsList);

            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;

            BindingContext = this;

            LoadCashInflowsAsync();
        }

        private void OnSearchTextChangedCustom(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

            // Clear and update the existing filtered collection
            FilteredCashInflowsList.Clear();

            var filteredList = string.IsNullOrEmpty(searchText)
                ? CashInflowsList
                : CashInflowsList.Where(inflow =>
                      (!string.IsNullOrEmpty(inflow.Title) && inflow.Title.ToLower().Contains(searchText)) ||
                      (!string.IsNullOrEmpty(inflow.Note) && inflow.Note.ToLower().Contains(searchText)));

            foreach (var inflow in filteredList)
            {
                FilteredCashInflowsList.Add(inflow);
            }

            // Ensure the ListView is bound to the correct source
            CashInflowListView.ItemsSource = FilteredCashInflowsList;
        }


        private async void LoadCashInflowsAsync()
        {
            try
            {
                var cashInflows = await DatabaseHelper.GetCashInflowsAsync();

                // Clear and populate the list
                FilteredCashInflowsList.Clear();
                foreach (var cashInFlow in cashInflows)
                {
                    FilteredCashInflowsList.Add(cashInFlow);
                }

                // Automatically update the UI if it's bound to FilteredCashInflowsList
                WriteCashInFlowError_log("Successfully loaded cash inflows.");
                WriteCashInFlowError_log($"Fetched {cashInflows.Count} cash inflows.");
            }
            catch (Exception ex)
            {
                WriteCashInFlowError_log($"Error loading cash inflows: {ex.Message}");
            }
        }



        private async void OnAddCashInflowClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TitleEntry.Text) || string.IsNullOrEmpty(AmountEntry.Text) || string.IsNullOrEmpty(TagsEntry.Text))
            {
                await DisplayAlert("Error", "Please fill in all.", "OK");
                return;
            }

            if (!decimal.TryParse(AmountEntry.Text, out var amount))
            {
                await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                return;
            }

            var newInflow = new CashInFlow
            {
                Title = TitleEntry.Text,
                Amount = amount,
                Date = DateTime.Now,
                Note = NoteEntry.Text,
                Tags = TagsEntry.Text
            };

            CashInflowsList.Add(newInflow);

            CashInFlowError.TextColor = Colors.Green;
            CashInFlowError.Text = "Cash Inflow Added Successfully!";
            CashInFlowError.IsVisible = true;

            TitleEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
            NoteEntry.Text = string.Empty;
            TagsEntry.Text = string.Empty;

            try
            {
                await DatabaseHelper.InsertCashInflowAsync(newInflow);
                WriteCashInFlowError_log("Sucessfully inserted cash inflow into database");
            }
            catch (Exception ex)
            {
                WriteCashInFlowError_log($"Error inserting cash inflow: {ex.Message}");
            }
        }

        private async void OnCashInflowSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedInflow = e.CurrentSelection.FirstOrDefault() as CashInFlow;
            if (selectedInflow != null)
            {
                await DisplayAlert("Cash Inflow Details",
                                   $"Title: {selectedInflow.Title}\nAmount: {selectedInflow.Amount:C}\nDate: {selectedInflow.Date:MM/dd/yyyy}",
                                   "OK");

                CashInflowListView.SelectedItem = null;
            }
        }

        private async void OnGoToTransactionClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransactionPage());
        }

        private static void WriteCashInFlowError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_error.txt");

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

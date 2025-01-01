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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } = DateTime.Now;

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

        private void FilterCashInflows()
        {
            var filteredList = CashInflowsList
                .Where(x => x.Date >= StartDate && x.Date <= EndDate)
                .ToList();

            FilteredCashInflowsList.Clear();

            foreach (var item in filteredList)
            {
                FilteredCashInflowsList.Add(item);
            }
        }

        private void OnStartDateChanged(object sender, DateChangedEventArgs e)
        {
            StartDate = e.NewDate;

            if (EndDate < StartDate)
            {
                EndDate = StartDate;
            }

            // Refresh filtered list
            FilterCashInflows();
        }

        private void OnEndDateChanged(object sender, DateChangedEventArgs e)
        {
            EndDate = e.NewDate;
            if (EndDate < StartDate)
            {
                EndDate = StartDate;
            }

            FilterCashInflows();
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                FilteredCashInflowsList = new ObservableCollection<CashInFlow>(CashInflowsList);
            }
            else
            {
                var filteredList = CashInflowsList
                    .Where(inflow => inflow.Title.ToLower().Contains(searchText) || inflow.Note.ToLower().Contains(searchText))
                    .ToList();

                FilteredCashInflowsList = new ObservableCollection<CashInFlow>(filteredList);
            }

            CashInflowListView.ItemsSource = FilteredCashInflowsList;
        }
        private async void LoadCashInflowsAsync()
        {
            try
            {
                var cashInflows = await DatabaseHelper.GetCashInflowsAsync();

                Device.BeginInvokeOnMainThread(() =>
                {
                    CashInflowsList.Clear();
                    foreach (var cashInFlow in cashInflows)
                    {
                        CashInflowsList.Add(cashInFlow);
                    }

                    CashInflowListView.ItemsSource = CashInflowsList;
                });

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
            if (string.IsNullOrEmpty(TitleEntry.Text) || string.IsNullOrEmpty(AmountEntry.Text) || string.IsNullOrEmpty(NoteEntry.Text))
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
                Note = NoteEntry.Text
            };

            CashInflowsList.Add(newInflow);

            CashInFlowError.TextColor = Colors.Green;
            CashInFlowError.Text = "Cash Inflow Added Successfully!";
            CashInFlowError.IsVisible = true;

            TitleEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
            NoteEntry.Text = string.Empty;

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

        private async void OnGoToDashboardClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DashboardPage());
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

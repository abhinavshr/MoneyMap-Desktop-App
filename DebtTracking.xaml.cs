using Microsoft.Maui.Controls;
using MoneyMap.Data;
using System;
using System.Collections.ObjectModel;

namespace MoneyMap
{
    public partial class DebtTracking : ContentPage
    {
        public ObservableCollection<DebtTrackings> Debts { get; set; }

        public DebtTracking()
        {
            InitializeComponent();
            Debts = new ObservableCollection<DebtTrackings>();
            DebtListView.ItemsSource = Debts;
            LoadDebtTrackingData();
        }
        private async void LoadDebtTrackingData()
        {
            try
            {
                var debtList = await DatabaseHelper.GetDebtTrackingAsync();
                Debts.Clear();

                foreach (var debt in debtList)
                {
                    Console.WriteLine($"Debt: {debt.Name}, Amount: {debt.Amount}, Due Date: {debt.DueDate}");
                    Debts.Add(debt);
                }

                // Log success after adding data to the collection
                WriteDebtTrackingError_log("Successfully loaded debt tracking data from database.");
            }
            catch (Exception ex)
            {
                // Log the error message
                WriteDebtTrackingError_log($"Failed to load debt tracking data: {ex.Message}");
                await DisplayAlert("Error", $"Failed to load debts: {ex.Message}", "OK");
            }
        }




        private async void OnAddDebtClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameEntry.Text) || string.IsNullOrWhiteSpace(AmountEntry.Text) || !decimal.TryParse(AmountEntry.Text, out var amount))
            {
                await DisplayAlert("Error", "Please enter valid debt details.", "OK");
                return;
            }

            var newDebt = new DebtTrackings
            {
                Name = NameEntry.Text,
                Amount = amount,
                ClearedAmount = 0,
                DueDate = DueDatePicker.Date
            };

            NameEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;
            DueDatePicker.Date = DateTime.Now;

            DebtTrackingError.TextColor = Colors.Green;
            DebtTrackingError.Text = "Debt added successfully!";
            DebtTrackingError.IsVisible = true;

            try
            {
                await DatabaseHelper.InsertDebtTracking(newDebt);
                WriteDebtTrackingError_log("Successfully inserted debt tracking into database");
            }
            catch (Exception ex)
            {
                WriteDebtTrackingError_log($"Error inserting debt tracking: {ex.Message}");
                await DisplayAlert("Error", $"Failed to add debt: {ex.Message}", "OK");
            }
        }

        private async void OnClearDebtsClicked(object sender, EventArgs e)
        {
            try
            {
                // Get the amount entered in ClearAmountEntry
                if (!decimal.TryParse(ClearAmountEntry.Text, out decimal clearedAmount))
                {
                    await DisplayAlert("Error", "Please enter a valid amount.", "OK");
                    return;
                }

                // Fetch total inflow and outflow using DatabaseHelper
                decimal totalInflow = await DatabaseHelper.GetTotalInflowAsync();
                decimal totalOutflow = await DatabaseHelper.GetTotalOutflowAsync();
                decimal availableFunds = totalInflow - totalOutflow;

                // Fetch the total debt before any payment
                decimal totalDebtLeft = await DatabaseHelper.GetTotalDebtLeftAsync();

                // Check if sufficient funds are available
                if (availableFunds >= clearedAmount)
                {
                    // Update debt tracking
                    bool isDebtCleared = await DatabaseHelper.ClearDebtAsync(clearedAmount);

                    if (isDebtCleared)
                    {
                        // Calculate total debt paid and remaining debt after clearing
                        decimal totalDebtPaid = await DatabaseHelper.GetTotalDebtPaidAsync(totalDebtLeft + clearedAmount);
                        decimal remainingDebt = totalDebtLeft - clearedAmount;

                        // Show total debt paid and remaining debt after clearing
                        await DisplayAlert("Debt Cleared",
                            $"Debt cleared successfully.\n\n" +
                            $"Total Debt To Pay: {totalDebtPaid:C}\n" +
                            $"Remaining Debt: {remainingDebt:C}",
                            "OK");

                        // Optionally reset ClearAmountEntry
                        ClearAmountEntry.Text = string.Empty;
                    }
                    else
                    {
                        await DisplayAlert("Error", "Failed to clear the debt. Please try again.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Insufficient funds to clear the debt.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }


        private static void WriteDebtTrackingError_log(string message)
        {
            string logFilePath = Path.Combine("D:\\DotNetError", "log_debttracking_error.txt");

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

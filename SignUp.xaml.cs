using Microsoft.Maui.Controls;
using MoneyMap.Data;

namespace MoneyMap
{
    public partial class SignUp : ContentPage
    {
        public SignUp()
        {
            InitializeComponent();
        }
        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            string fullName = FullNameEntry.Text;
            string username = UsernameEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            //await DisplayAlert("Success", "Account created successfully!", "OK");

            //await Navigation.PopAsync();

            try
            {
                // Save user to the database
                DatabaseHelper.AddUser(fullName, username, password);
                SignUpError.TextColor = Colors.Green;
                SignUpError.Text = "Registration successful!";
                SignUpError.IsVisible = true;

                // Clear fields
                FullNameEntry.Text = string.Empty;
                PasswordEntry.Text = string.Empty;
                ConfirmPasswordEntry.Text = string.Empty;

                // Optionally navigate back to login page
                // await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                SignUpError.Text = $"Error: {ex.Message}";
                SignUpError.IsVisible = true;
            }
        }

        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Navigate back to the Login page
            await Navigation.PopAsync();
        }
    }
}

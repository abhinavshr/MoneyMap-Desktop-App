using Microsoft.Maui.Controls;

namespace MoneyMap
{
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();
        }


        // Login button click handler
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry.Text?.Trim();
            string password = PasswordEntry.Text?.Trim();

            if (username == "admin" && password == "password")
            {
                // Navigate to the dashboard page
                await Navigation.PushAsync(new DashboardPage());
            }
            else
            {
                // Display error message
                await DisplayAlert("Login Failed", "Invalid username or password. Please try again.", "OK");
            }
        }


        // Sign Up button click handler
        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            // Navigate to the Sign-Up page
            await Navigation.PushAsync(new SignUp());
        }
    }
}

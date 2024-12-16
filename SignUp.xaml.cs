using Microsoft.Maui.Controls;

namespace MoneyMap
{
    public partial class SignUp : ContentPage
    {
        public SignUp()
        {
            InitializeComponent();
        }

        // Sign Up button click handler
        private async void OnSignUpClicked(object sender, EventArgs e)
        {
            // Validate input
            string fullName = FullNameEntry.Text;
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;
            string confirmPassword = ConfirmPasswordEntry.Text;

            // Basic validation for empty fields
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                await DisplayAlert("Error", "Please fill in all fields.", "OK");
                return;
            }

            // Check if passwords match
            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            // Here, you can add logic for signing up the user (e.g., API call)

            // If successful, navigate to the login page or home page
            await DisplayAlert("Success", "Account created successfully!", "OK");

            // Navigate to the login page or home screen
            await Navigation.PopAsync();  // Go back to Login page
        }

        // Login text link click handler
        private async void OnLoginTapped(object sender, EventArgs e)
        {
            // Navigate back to the Login page
            await Navigation.PopAsync();
        }
    }
}

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
        private void OnLoginClicked(object sender, EventArgs e)
        {
            // Implement login logic here (e.g., validate username/password)
        }

        // Sign Up button click handler
        private async void OnSignUpTapped(object sender, EventArgs e)
        {
            // Navigate to the Sign-Up page
            await Navigation.PushAsync(new SignUp());
        }
    }
}

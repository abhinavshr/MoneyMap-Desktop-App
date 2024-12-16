using Microsoft.Maui.Controls;

namespace MoneyMap
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Login());
        }
    }
}

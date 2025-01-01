using Microsoft.Maui.Controls;
using MoneyMap.Data;

namespace MoneyMap
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new Login());
            DatabaseHelper.InitializeDatabase();
            //MainPage = new AppShell();
        }
    }
}

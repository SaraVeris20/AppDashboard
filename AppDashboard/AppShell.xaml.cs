namespace AppDashboard
{
    public partial class App : Application
    {
        // Remova o construtor duplicado "App()" desta classe.
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
    }
}
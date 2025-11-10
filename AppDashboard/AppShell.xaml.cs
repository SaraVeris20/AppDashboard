using AppDashboard.Views;

namespace AppDashboard
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AdicionarUsuarioPage), typeof(AdicionarUsuarioPage));
            Routing.RegisterRoute(nameof(AreaAdministrativaPage), typeof(AreaAdministrativaPage));
        }
    }
}
using AppDashboard.ViewModels;
using AppDashboard.Services;

namespace AppDashboard.Views
{
    public partial class AreaAdministrativaPage : ContentPage
    {
        private readonly AreaAdministrativaViewModel _viewModel;

        public AreaAdministrativaPage(UsuarioService usuarioService)
        {
            InitializeComponent();

            _viewModel = new AreaAdministrativaViewModel(usuarioService);
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.CarregarDados();
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnMenuClicked(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet(
                "Opções",
                "Cancelar",
                null,
                "Configurações",
                "Exportar Lista",
                "Atualizar");

            if (action == "Atualizar")
            {
                _viewModel.CarregarDados();
            }
        }

        private async void OnAdicionarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AdicionarUsuarioPage));
        }
    }
}
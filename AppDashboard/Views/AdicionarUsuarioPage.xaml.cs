using AppDashboard.ViewModels;
using AppDashboard.Services;

namespace AppDashboard.Views
{
    public partial class AdicionarUsuarioPage : ContentPage
    {
        private readonly AdicionarUsuarioViewModel _viewModel;

        public AdicionarUsuarioPage(UsuarioService usuarioService)
        {
            InitializeComponent();

            _viewModel = new AdicionarUsuarioViewModel(usuarioService);
            BindingContext = _viewModel;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            bool resposta = await DisplayAlert(
                "Confirmação",
                "Deseja sair sem salvar?",
                "Sim",
                "Não");

            if (resposta)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private async void OnSalvarClicked(object sender, EventArgs e)
        {
            if (_viewModel.SalvarCommand.CanExecute(null))
            {
                _viewModel.SalvarCommand.Execute(null);
            }
            else
            {
                await DisplayAlert(
                    "Atenção",
                    "Por favor, preencha todos os campos corretamente.",
                    "OK");
            }
        }
    }
}
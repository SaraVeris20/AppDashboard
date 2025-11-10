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
            AtualizarContador();
        }

        private void AtualizarContador()
        {
            var total = _viewModel.Usuarios.Count;
            lblContador.Text = total == 1
                ? "1 usuário cadastrado"
                : $"{total} usuários cadastrados";
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
                "Limpar Todos",
                "Atualizar",
                "Exportar Lista");

            switch (action)
            {
                case "Atualizar":
                    _viewModel.CarregarDados();
                    AtualizarContador();
                    break;

                case "Limpar Todos":
                    if (_viewModel.LimparTodosCommand.CanExecute(null))
                    {
                        _viewModel.LimparTodosCommand.Execute(null);
                        AtualizarContador();
                    }
                    break;

                case "Exportar Lista":
                    await DisplayAlert("Em Desenvolvimento", "Funcionalidade em desenvolvimento", "OK");
                    break;
            }
        }

        private async void OnAdicionarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AdicionarUsuarioPage));
        }

        private async void OnRemoverUsuario(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var usuarioId = button.CommandParameter?.ToString();

            if (!string.IsNullOrEmpty(usuarioId))
            {
                if (_viewModel.RemoverUsuarioCommand.CanExecute(usuarioId))
                {
                    _viewModel.RemoverUsuarioCommand.Execute(usuarioId);
                    AtualizarContador();
                }
            }
        }
    }
}
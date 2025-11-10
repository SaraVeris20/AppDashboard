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
            if (_viewModel.VoltarCommand.CanExecute(null))
            {
                _viewModel.VoltarCommand.Execute(null);
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}
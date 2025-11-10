using AppDashboard.Models;
using AppDashboard.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppDashboard.ViewModels
{
    public class AdicionarUsuarioViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private string _nome = string.Empty;
        private string _email = string.Empty;
        private string _cargoSelecionado = string.Empty;
        private List<string> _cargosDisponiveis;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public string CargoSelecionado
        {
            get => _cargoSelecionado;
            set
            {
                _cargoSelecionado = value;
                OnPropertyChanged();
                ((Command)SalvarCommand).ChangeCanExecute();
            }
        }

        public List<string> CargosDisponiveis
        {
            get => _cargosDisponiveis;
            set
            {
                _cargosDisponiveis = value;
                OnPropertyChanged();
            }
        }

        public ICommand SalvarCommand { get; }
        public ICommand VoltarCommand { get; }

        public AdicionarUsuarioViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _cargosDisponiveis = new List<string>();

            SalvarCommand = new Command(async () => await SalvarUsuario(), PodeSalvar);
            VoltarCommand = new Command(async () => await Voltar());

            CarregarCargos();
        }

        private void CarregarCargos()
        {
            CargosDisponiveis = _usuarioService.ObterCargosDisponiveis();
        }

        private bool PodeSalvar()
        {
            return !string.IsNullOrWhiteSpace(Nome) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(CargoSelecionado) &&
                   IsEmailValido(Email);
        }

        private bool IsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SalvarUsuario()
        {
            var novoUsuario = new Usuario
            {
                Nome = Nome,
                Email = Email,
                Cargo = CargoSelecionado,
                FotoUrl = "default_avatar.png"
            };

            bool sucesso = _usuarioService.AdicionarUsuario(novoUsuario);

            if (sucesso)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso",
                    "Usuário adicionado com sucesso!",
                    "OK");

                LimparCampos();
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Erro",
                    "Não foi possível adicionar o usuário. Tente novamente.",
                    "OK");
            }
        }

        private async Task Voltar()
        {
            bool resposta = await Application.Current.MainPage.DisplayAlert(
                "Confirmação",
                "Deseja sair sem salvar?",
                "Sim",
                "Não");

            if (resposta)
            {
                LimparCampos();
                await Shell.Current.GoToAsync("..");
            }
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            Email = string.Empty;
            CargoSelecionado = string.Empty;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
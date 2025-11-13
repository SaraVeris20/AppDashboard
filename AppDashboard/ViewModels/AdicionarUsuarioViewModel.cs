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
        private string? _unidadeGrupoSelecionada;
        private List<string> _cargosDisponiveis;
        private List<string> _unidadesGrupos;

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

        public string? UnidadeGrupoSelecionada
        {
            get => _unidadeGrupoSelecionada;
            set
            {
                _unidadeGrupoSelecionada = value;
                OnPropertyChanged();
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

        public List<string> UnidadesGrupos
        {
            get => _unidadesGrupos;
            set
            {
                _unidadesGrupos = value;
                OnPropertyChanged();
            }
        }

        public ICommand SalvarCommand { get; }
        public ICommand VoltarCommand { get; }

        public AdicionarUsuarioViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _cargosDisponiveis = new List<string>();
            _unidadesGrupos = new List<string>();

            SalvarCommand = new Command(async () => await SalvarUsuario(), PodeSalvar);
            VoltarCommand = new Command(async () => await Voltar());

            CarregarDados();
        }

        private void CarregarDados()
        {
            CargosDisponiveis = _usuarioService.ObterCargosDisponiveis();
            var unidades = _usuarioService.ObterUnidadesGrupos();
            // Remove "Todas as Unidades" da lista de seleção
            UnidadesGrupos = unidades.Where(u => u != "Todas as Unidades").ToList();
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
                Nome = Nome.Trim(),
                Cargo = CargoSelecionado,
                UnidadeGrupo = UnidadeGrupoSelecionada,
                FotoUrl = "default_avatar.png"
            };

            bool sucesso = await _usuarioService.AdicionarUsuario(novoUsuario);

            if (sucesso)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Sucesso! 🎉",
                    $"Usuário '{novoUsuario.Nome}' adicionado com sucesso!",
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
            bool temDados = !string.IsNullOrWhiteSpace(Nome) ||
                           !string.IsNullOrWhiteSpace(Email) ||
                           !string.IsNullOrWhiteSpace(CargoSelecionado);

            if (temDados)
            {
                bool resposta = await Application.Current.MainPage.DisplayAlert(
                    "Confirmação",
                    "Deseja sair sem salvar?",
                    "Sim",
                    "Não");

                if (!resposta)
                    return;
            }

            LimparCampos();
            await Shell.Current.GoToAsync("..");
        }

        private void LimparCampos()
        {
            Nome = string.Empty;
            Email = string.Empty;
            CargoSelecionado = string.Empty;
            UnidadeGrupoSelecionada = null;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
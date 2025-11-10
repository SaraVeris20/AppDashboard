using AppDashboard.Models;
using AppDashboard.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AppDashboard.ViewModels
{
    public class AreaAdministrativaViewModel : INotifyPropertyChanged
    {
        private readonly UsuarioService _usuarioService;
        private ObservableCollection<Usuario> _usuarios;
        private List<string> _unidadesGrupos;
        private string _unidadeSelecionada;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set
            {
                _usuarios = value;
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

        public string UnidadeSelecionada
        {
            get => _unidadeSelecionada;
            set
            {
                _unidadeSelecionada = value;
                OnPropertyChanged();
                FiltrarUsuarios();
            }
        }

        public ICommand AdicionarUsuarioCommand { get; }
        public ICommand AtualizarListaCommand { get; }
        public ICommand RemoverUsuarioCommand { get; }
        public ICommand LimparTodosCommand { get; }

        public AreaAdministrativaViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _usuarios = new ObservableCollection<Usuario>();
            _unidadesGrupos = new List<string>();
            _unidadeSelecionada = "Todas as Unidades";

            AdicionarUsuarioCommand = new Command(async () => await AdicionarUsuario());
            AtualizarListaCommand = new Command(CarregarDados);
            RemoverUsuarioCommand = new Command<string>(async (id) => await RemoverUsuario(id));
            LimparTodosCommand = new Command(async () => await LimparTodos());

            CarregarDados();
        }

        public void CarregarDados()
        {
            Usuarios = _usuarioService.ObterTodosUsuarios();
            UnidadesGrupos = _usuarioService.ObterUnidadesGrupos();
        }

        private void FiltrarUsuarios()
        {
            Usuarios = _usuarioService.ObterUsuariosPorUnidade(UnidadeSelecionada);
        }

        private async Task AdicionarUsuario()
        {
            await Shell.Current.GoToAsync("AdicionarUsuarioPage");
        }

        private async Task RemoverUsuario(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            var usuario = _usuarioService.ObterUsuarioPorId(id);
            if (usuario == null)
                return;

            bool confirmacao = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Exclusão",
                $"Deseja realmente remover o usuário '{usuario.Nome}'?",
                "Sim",
                "Não");

            if (confirmacao)
            {
                bool sucesso = await _usuarioService.RemoverUsuario(id);

                if (sucesso)
                {
                    await Application.Current.MainPage.DisplayAlert("Sucesso", "Usuário removido com sucesso!", "OK");
                    CarregarDados();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Não foi possível remover o usuário.", "OK");
                }
            }
        }

        private async Task LimparTodos()
        {
            var total = _usuarioService.ObterTodosUsuarios().Count;

            if (total == 0)
            {
                await Application.Current.MainPage.DisplayAlert("Aviso", "Não há usuários para remover.", "OK");
                return;
            }

            bool confirmacao = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Limpeza",
                $"Deseja realmente remover TODOS os {total} usuários?",
                "Sim",
                "Não");

            if (confirmacao)
            {
                await _usuarioService.LimparTodos();
                await Application.Current.MainPage.DisplayAlert("Sucesso", "Todos os usuários foram removidos!", "OK");
                CarregarDados();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
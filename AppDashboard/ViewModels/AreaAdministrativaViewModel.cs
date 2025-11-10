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

        public AreaAdministrativaViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _usuarios = new ObservableCollection<Usuario>();
            _unidadesGrupos = new List<string>();
            _unidadeSelecionada = "Todas as Unidades";

            AdicionarUsuarioCommand = new Command(async () => await AdicionarUsuario());
            AtualizarListaCommand = new Command(CarregarDados);

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

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
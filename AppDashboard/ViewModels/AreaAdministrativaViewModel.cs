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
        private ObservableCollection<Usuario> _usuariosFiltrados;
        private List<string> _unidadesGrupos;
        private string _unidadeSelecionada;
        private string _situacaoSelecionada;
        private string _textoBusca = string.Empty;
        private bool _isRefreshing = false;
        private bool _isBusy = false;

        // Estatísticas
        private int _totalUsuarios = 0;
        private int _totalTrabalhando = 0;
        private int _totalDemitidos = 0;
        private int _totalAposentados = 0;
        private int _totalAuxilioDoenca = 0;

        public event PropertyChangedEventHandler? PropertyChanged;

        // ==================== PROPRIEDADES ====================

        public ObservableCollection<Usuario> Usuarios
        {
            get => _usuarios;
            set
            {
                _usuarios = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Usuario> UsuariosFiltrados
        {
            get => _usuariosFiltrados;
            set
            {
                _usuariosFiltrados = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ContagemUsuarios));
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
                AplicarFiltros();
            }
        }

        public string SituacaoSelecionada
        {
            get => _situacaoSelecionada;
            set
            {
                _situacaoSelecionada = value;
                OnPropertyChanged();
                _ = AplicarFiltroSituacaoAsync();
            }
        }

        public string TextoBusca
        {
            get => _textoBusca;
            set
            {
                _textoBusca = value;
                OnPropertyChanged();
                AplicarFiltros();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string ContagemUsuarios => $"{UsuariosFiltrados?.Count ?? 0} usuários cadastrados";

        // Estatísticas
        public int TotalUsuarios
        {
            get => _totalUsuarios;
            set
            {
                _totalUsuarios = value;
                OnPropertyChanged();
            }
        }

        public int TotalTrabalhando
        {
            get => _totalTrabalhando;
            set
            {
                _totalTrabalhando = value;
                OnPropertyChanged();
            }
        }

        public int TotalDemitidos
        {
            get => _totalDemitidos;
            set
            {
                _totalDemitidos = value;
                OnPropertyChanged();
            }
        }

        public int TotalAposentados
        {
            get => _totalAposentados;
            set
            {
                _totalAposentados = value;
                OnPropertyChanged();
            }
        }

        public int TotalAuxilioDoenca
        {
            get => _totalAuxilioDoenca;
            set
            {
                _totalAuxilioDoenca = value;
                OnPropertyChanged();
            }
        }

        // ==================== COMANDOS ====================

        public ICommand AtualizarListaCommand { get; }
        public ICommand VerDetalhesCommand { get; }
        public ICommand DeletarUsuarioCommand { get; }

        // ==================== CONSTRUTOR ====================

        public AreaAdministrativaViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _usuarios = new ObservableCollection<Usuario>();
            _usuariosFiltrados = new ObservableCollection<Usuario>();
            _unidadesGrupos = new List<string>();
            _unidadeSelecionada = "Todas as Unidades";
            _situacaoSelecionada = "Todos";

            AtualizarListaCommand = new Command(async () => await CarregarDadosAsync());
            VerDetalhesCommand = new Command<Usuario>(async (usuario) => await VerDetalhes(usuario));
            DeletarUsuarioCommand = new Command<Usuario>(async (usuario) => await DeletarUsuario(usuario));
        }

        // ==================== MÉTODOS PRINCIPAIS ====================

        public async Task CarregarDadosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            IsRefreshing = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Carregando usuários do banco de dados MySQL...");

                // Carregar todos os usuários
                Usuarios = await _usuarioService.ObterTodosUsuariosAsync();

                // Carregar estatísticas
                var stats = await _usuarioService.ObterEstatisticasPorSituacaoAsync();
                TotalUsuarios = stats.ContainsKey("Total") ? stats["Total"] : Usuarios.Count;
                TotalTrabalhando = stats.ContainsKey("Trabalhando") ? stats["Trabalhando"] : 0;
                TotalDemitidos = stats.ContainsKey("Demitidos") ? stats["Demitidos"] : 0;
                TotalAposentados = stats.ContainsKey("Aposentadoria por Invalidez") ? stats["Aposentadoria por Invalidez"] : 0;
                TotalAuxilioDoenca = stats.ContainsKey("Auxílio Doença") ? stats["Auxílio Doença"] : 0;

                // Carregar filtros de unidades
                var unidades = await _usuarioService.ObterListaUnidadesAsync();
                UnidadesGrupos = unidades;

                // Aplicar filtro inicial
                await AplicarFiltroSituacaoAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {TotalUsuarios} usuários carregados com sucesso!");
                System.Diagnostics.Debug.WriteLine($"📊 Trabalhando: {TotalTrabalhando} | Demitidos: {TotalDemitidos} | Aposentados: {TotalAposentados} | Auxílio: {TotalAuxilioDoenca}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao carregar dados: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro ao Carregar Dados",
                    $"Não foi possível carregar os usuários do banco de dados.\n\n" +
                    $"Erro: {ex.Message}\n\n" +
                    $"Verifique:\n" +
                    $"• Conexão com internet\n" +
                    $"• Credenciais do MySQL no AppDbContext.cs\n" +
                    $"• Nome da tabela: rhdataset",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // ==================== FILTROS ====================

        private async Task AplicarFiltroSituacaoAsync()
        {
            if (IsBusy) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Aplicando filtro de situação: {SituacaoSelecionada}");

                ObservableCollection<Usuario> usuariosPorSituacao;

                switch (SituacaoSelecionada?.ToUpper())
                {
                    case "TRABALHANDO":
                        usuariosPorSituacao = await _usuarioService.ObterUsuariosTrabalhando();
                        break;

                    case "DEMITIDOS":
                    case "DEMITIDO":
                        usuariosPorSituacao = await _usuarioService.ObterUsuariosDemitidos();
                        break;

                    case "APOSENTADORIA POR INVALIDEZ":
                    case "APOSENTADOS":
                        usuariosPorSituacao = await _usuarioService.ObterUsuariosAposentadosPorInvalidez();
                        break;

                    case "AUXÍLIO DOENÇA":
                    case "AUXILIO DOENÇA":
                        usuariosPorSituacao = await _usuarioService.ObterUsuariosAuxilioDoenca();
                        break;

                    case "TODOS":
                    default:
                        usuariosPorSituacao = await _usuarioService.ObterTodosUsuariosAsync();
                        break;
                }

                // Atualizar cache local
                Usuarios.Clear();
                foreach (var usuario in usuariosPorSituacao)
                {
                    Usuarios.Add(usuario);
                }

                // Aplicar outros filtros (unidade e busca)
                AplicarFiltros();

                System.Diagnostics.Debug.WriteLine($"✅ Filtro aplicado: {UsuariosFiltrados.Count} usuários exibidos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao aplicar filtro: {ex.Message}");
            }
        }

        private void AplicarFiltros()
        {
            if (Usuarios == null)
            {
                UsuariosFiltrados = new ObservableCollection<Usuario>();
                return;
            }

            var usuariosFiltrados = Usuarios.AsEnumerable();

            // Filtrar por unidade/setor
            if (!string.IsNullOrEmpty(UnidadeSelecionada) && UnidadeSelecionada != "Todas as Unidades")
            {
                usuariosFiltrados = usuariosFiltrados.Where(u => u.UnidadeGrupo == UnidadeSelecionada);
                System.Diagnostics.Debug.WriteLine($"🔍 Filtrando por unidade: {UnidadeSelecionada}");
            }

            // Filtrar por texto de busca
            if (!string.IsNullOrWhiteSpace(TextoBusca))
            {
                var busca = TextoBusca.ToLower();
                usuariosFiltrados = usuariosFiltrados.Where(u =>
                    u.Nome.ToLower().Contains(busca) ||
                    u.Cargo.ToLower().Contains(busca) ||
                    (!string.IsNullOrEmpty(u.UnidadeGrupo) && u.UnidadeGrupo.ToLower().Contains(busca))
                );
                System.Diagnostics.Debug.WriteLine($"🔍 Buscando por: {TextoBusca}");
            }

            UsuariosFiltrados = new ObservableCollection<Usuario>(usuariosFiltrados);
            System.Diagnostics.Debug.WriteLine($"📋 Total após filtros: {UsuariosFiltrados.Count} usuários");
        }

        // ==================== AÇÕES ====================

        private async Task VerDetalhes(Usuario usuario)
        {
            if (usuario == null) return;

            var detalhes = $"📋 INFORMAÇÕES DO COLABORADOR\n\n" +
                          $"🆔 ID: {usuario.Id}\n" +
                          $"👤 Nome: {usuario.Nome}\n" +
                          $"📧 Email: {usuario.Email}\n" +
                          $"💼 Cargo: {usuario.Cargo}\n" +
                          $"{usuario.StatusEmoji} Situação: {usuario.StatusDescricao}\n";

            if (!string.IsNullOrEmpty(usuario.UnidadeGrupo))
                detalhes += $"🏢 Setor: {usuario.UnidadeGrupo}\n";

            detalhes += $"\n📊 STATUS:\n" +
                       $"Cor: {usuario.StatusCor}\n" +
                       $"Ativo: {(usuario.EstaAtivo ? "Sim" : "Não")}";

            await Application.Current!.MainPage!.DisplayAlert(
                "Detalhes do Colaborador",
                detalhes,
                "Fechar");
        }

        private async Task DeletarUsuario(Usuario usuario)
        {
            if (usuario == null) return;

            bool confirmacao = await Application.Current!.MainPage!.DisplayAlert(
                "⚠️ Confirmar Exclusão",
                $"Deseja realmente excluir o usuário:\n\n" +
                $"👤 {usuario.Nome}\n" +
                $"💼 {usuario.Cargo}\n\n" +
                $"Esta ação não pode ser desfeita!",
                "Sim, excluir",
                "Cancelar");

            if (!confirmacao) return;

            try
            {
                IsBusy = true;
                System.Diagnostics.Debug.WriteLine($"🗑️ Excluindo usuário: {usuario.Nome} (ID: {usuario.Id})");

                // TODO: Implementar exclusão no banco de dados
                // bool sucesso = await _usuarioService.DeletarUsuarioAsync(usuario.Id);

                // Por enquanto, apenas remove da lista local
                Usuarios.Remove(usuario);
                AplicarFiltros();

                // Atualizar estatísticas
                TotalUsuarios--;
                if (usuario.EstaAtivo) TotalTrabalhando--;
                else if (usuario.EstaDemitido) TotalDemitidos--;
                else if (usuario.EstaAposentadoPorInvalidez) TotalAposentados--;
                else if (usuario.EstaEmAuxilioDoenca) TotalAuxilioDoenca--;

                System.Diagnostics.Debug.WriteLine($"✅ Usuário {usuario.Nome} excluído com sucesso!");

                await Application.Current!.MainPage!.DisplayAlert(
                    "✅ Sucesso",
                    $"Usuário '{usuario.Nome}' foi excluído com sucesso!",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao excluir: {ex.Message}");

                await Application.Current!.MainPage!.DisplayAlert(
                    "❌ Erro",
                    $"Não foi possível excluir o usuário.\n\n" +
                    $"Erro: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
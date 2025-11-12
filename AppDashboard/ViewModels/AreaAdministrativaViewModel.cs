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
        private List<string> _situacoesDisponiveis;
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
        private string _mensagemStatus = "Carregando...";

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

        public List<string> SituacoesDisponiveis
        {
            get => _situacoesDisponiveis;
            set
            {
                _situacoesDisponiveis = value;
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

        public string MensagemStatus
        {
            get => _mensagemStatus;
            set
            {
                _mensagemStatus = value;
                OnPropertyChanged();
            }
        }

        // ==================== COMANDOS ====================

        public ICommand AtualizarListaCommand { get; }
        public ICommand DiagnosticoCommand { get; }
        public ICommand VerDetalhesCommand { get; }

        // ==================== CONSTRUTOR ====================

        public AreaAdministrativaViewModel(UsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
            _usuarios = new ObservableCollection<Usuario>();
            _usuariosFiltrados = new ObservableCollection<Usuario>();
            _unidadesGrupos = new List<string>();
            _situacoesDisponiveis = new List<string>();
            _unidadeSelecionada = "Todas as Unidades";
            _situacaoSelecionada = "Todos";

            // Lista de situações disponíveis
            SituacoesDisponiveis = new List<string>
            {
                "Todos",
                "Trabalhando",
                "Demitidos",
                "Aposentadoria por Invalidez",
                "Auxílio Doença"
            };

            AtualizarListaCommand = new Command(async () => await CarregarDadosAsync());
            DiagnosticoCommand = new Command(async () => await ExecutarDiagnosticoAsync());
            VerDetalhesCommand = new Command<Usuario>(async (usuario) => await VerDetalhes(usuario));
        }

        // ==================== MÉTODOS PRINCIPAIS ====================

        public async Task InicializarAsync()
        {
            await CarregarDadosAsync();
        }

        public async Task CarregarDadosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            IsRefreshing = true;
            MensagemStatus = "Conectando ao MySQL AWS...";

            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Iniciando carregamento da tabela rhdataset...");

                // Carregar TODOS os usuários
                MensagemStatus = "Buscando usuários da tabela rhdataset...";
                Usuarios = await _usuarioService.ObterTodosUsuariosAsync();

                // Carregar estatísticas
                MensagemStatus = "Calculando estatísticas...";
                var stats = await _usuarioService.ObterEstatisticasPorSituacaoAsync();

                TotalUsuarios = stats.ContainsKey("Total") ? stats["Total"] : Usuarios.Count;
                TotalTrabalhando = stats.ContainsKey("Trabalhando") ? stats["Trabalhando"] : 0;
                TotalDemitidos = stats.ContainsKey("Demitidos") ? stats["Demitidos"] : 0;
                TotalAposentados = stats.ContainsKey("Aposentadoria por Invalidez") ? stats["Aposentadoria por Invalidez"] : 0;
                TotalAuxilioDoenca = stats.ContainsKey("Auxílio Doença") ? stats["Auxílio Doença"] : 0;

                // Carregar filtros
                MensagemStatus = "Carregando filtros...";
                var unidadesDoBanco = await _usuarioService.ObterListaUnidadesAsync();
                UnidadesGrupos = unidadesDoBanco;

                // Aplicar filtro inicial
                await AplicarFiltroSituacaoAsync();

                MensagemStatus = $"✅ {TotalUsuarios} colaboradores | " +
                                $"{TotalTrabalhando} trabalhando | " +
                                $"{TotalDemitidos} demitidos | " +
                                $"{TotalAposentados} aposentados | " +
                                $"{TotalAuxilioDoenca} auxílio doença";

                System.Diagnostics.Debug.WriteLine("✅ Carregamento concluído!");
                System.Diagnostics.Debug.WriteLine($"📊 Total: {TotalUsuarios} | Trabalhando: {TotalTrabalhando} | Demitidos: {TotalDemitidos}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                MensagemStatus = "❌ Erro ao carregar dados";

                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro ao Carregar",
                    $"Não foi possível carregar os dados da tabela rhdataset.\n\n" +
                    $"Erro: {ex.Message}\n\n" +
                    $"Verifique:\n" +
                    $"• Conexão com internet\n" +
                    $"• Nome da tabela: rhdataset\n" +
                    $"• Nome da coluna: Descrição (Situação)",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        public void CarregarDados()
        {
            Task.Run(async () => await CarregarDadosAsync());
        }

        // ==================== FILTROS ====================

        private async Task AplicarFiltroSituacaoAsync()
        {
            if (IsBusy) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Aplicando filtro: {SituacaoSelecionada}");

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

                // Aplicar outros filtros
                AplicarFiltros();

                System.Diagnostics.Debug.WriteLine($"✅ Filtro aplicado: {UsuariosFiltrados.Count} usuários exibidos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao filtrar: {ex.Message}");
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

            // Filtrar por unidade
            if (!string.IsNullOrEmpty(UnidadeSelecionada) && UnidadeSelecionada != "Todas as Unidades")
            {
                usuariosFiltrados = usuariosFiltrados.Where(u => u.UnidadeGrupo == UnidadeSelecionada);
            }

            // Filtrar por texto de busca
            if (!string.IsNullOrWhiteSpace(TextoBusca))
            {
                var busca = TextoBusca.ToLower();
                usuariosFiltrados = usuariosFiltrados.Where(u =>
                    u.Nome.ToLower().Contains(busca) ||
                    u.Cargo.ToLower().Contains(busca) ||
                    (!string.IsNullOrEmpty(u.DescricaoSituacao) && u.DescricaoSituacao.ToLower().Contains(busca))
                );
            }

            UsuariosFiltrados = new ObservableCollection<Usuario>(usuariosFiltrados);
        }

        // ==================== OUTROS MÉTODOS ====================

        private async Task ExecutarDiagnosticoAsync()
        {
            IsBusy = true;
            MensagemStatus = "Executando diagnóstico...";

            try
            {
                var diagnostico = await _usuarioService.DiagnosticarBancoDadosAsync();

                await Application.Current!.MainPage!.DisplayAlert(
                    "🔍 Diagnóstico - Tabela rhdataset",
                    diagnostico,
                    "OK");
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro",
                    ex.Message,
                    "OK");
            }
            finally
            {
                IsBusy = false;
                MensagemStatus = $"✅ {TotalUsuarios} colaboradores carregados";
            }
        }

        private async Task VerDetalhes(Usuario usuario)
        {
            if (usuario == null) return;

            var detalhes = $"📋 INFORMAÇÕES DO COLABORADOR\n\n" +
                          $"ID: {usuario.Id}\n" +
                          $"👤 Nome: {usuario.Nome}\n" +
                          $"💼 Cargo: {usuario.Cargo}\n" +
                          $"{usuario.StatusEmoji} Situação: {usuario.StatusDescricao}\n";

            if (!string.IsNullOrEmpty(usuario.UnidadeGrupo))
                detalhes += $"🏢 Setor: {usuario.UnidadeGrupo}\n";

            await Application.Current!.MainPage!.DisplayAlert(
                "Detalhes do Colaborador",
                detalhes,
                "Fechar");
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
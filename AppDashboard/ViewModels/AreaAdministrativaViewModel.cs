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
                System.Diagnostics.Debug.WriteLine($"🔄 UsuariosFiltrados atualizado: {value?.Count ?? 0} itens");
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
                System.Diagnostics.Debug.WriteLine($"🏢 Unidade selecionada: {value}");
                AplicarFiltrosLocal();
            }
        }

        public string SituacaoSelecionada
        {
            get => _situacaoSelecionada;
            set
            {
                System.Diagnostics.Debug.WriteLine($"🎯 Situação mudou para '{value}'");
                _situacaoSelecionada = value;
                OnPropertyChanged();
                AplicarFiltrosLocal();
            }
        }

        public string TextoBusca
        {
            get => _textoBusca;
            set
            {
                _textoBusca = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"🔍 Busca: {value}");
                AplicarFiltrosLocal();
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
            set { _totalUsuarios = value; OnPropertyChanged(); }
        }

        public int TotalTrabalhando
        {
            get => _totalTrabalhando;
            set { _totalTrabalhando = value; OnPropertyChanged(); }
        }

        public int TotalDemitidos
        {
            get => _totalDemitidos;
            set { _totalDemitidos = value; OnPropertyChanged(); }
        }

        public int TotalAposentados
        {
            get => _totalAposentados;
            set { _totalAposentados = value; OnPropertyChanged(); }
        }

        public int TotalAuxilioDoenca
        {
            get => _totalAuxilioDoenca;
            set { _totalAuxilioDoenca = value; OnPropertyChanged(); }
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

            System.Diagnostics.Debug.WriteLine("✅ ViewModel criado");
        }

        // ==================== MÉTODOS PRINCIPAIS ====================

        public async Task CarregarDadosAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            IsRefreshing = true;

            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Carregando TODOS os usuários do banco...");

                // Carregar TODOS os usuários de uma vez
                Usuarios = await _usuarioService.ObterTodosUsuariosAsync();
                System.Diagnostics.Debug.WriteLine($"📥 {Usuarios.Count} usuários carregados!");

                // Calcular estatísticas localmente
                TotalUsuarios = Usuarios.Count;
                TotalTrabalhando = Usuarios.Count(u => u.EstaAtivo);
                TotalDemitidos = Usuarios.Count(u => u.EstaDemitido);
                TotalAposentados = Usuarios.Count(u => u.EstaAposentadoPorInvalidez);
                TotalAuxilioDoenca = Usuarios.Count(u => u.EstaEmAuxilioDoenca);

                System.Diagnostics.Debug.WriteLine($"📊 Estatísticas calculadas:");
                System.Diagnostics.Debug.WriteLine($"   Total: {TotalUsuarios}");
                System.Diagnostics.Debug.WriteLine($"   ✅ Trabalhando: {TotalTrabalhando}");
                System.Diagnostics.Debug.WriteLine($"   ❌ Demitidos: {TotalDemitidos}");
                System.Diagnostics.Debug.WriteLine($"   🏥 Aposentados: {TotalAposentados}");
                System.Diagnostics.Debug.WriteLine($"   🤕 Auxílio: {TotalAuxilioDoenca}");

                // Verificar valores únicos na coluna situação
                var situacoesUnicas = Usuarios
                    .Select(u => u.DescricaoSituacao)
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"📋 Situações encontradas no banco ({situacoesUnicas.Count}):");
                foreach (var sit in situacoesUnicas.Take(10))
                {
                    var count = Usuarios.Count(u => u.DescricaoSituacao == sit);
                    System.Diagnostics.Debug.WriteLine($"   '{sit}': {count} usuários");
                }

                // Carregar unidades
                var unidades = await _usuarioService.ObterListaUnidadesAsync();
                UnidadesGrupos = unidades;

                // Aplicar filtro inicial (Todos)
                AplicarFiltrosLocal();

                System.Diagnostics.Debug.WriteLine($"✅ Carregamento concluído! Exibindo: {UsuariosFiltrados.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert(
                    "Erro",
                    $"Não foi possível carregar os dados.\n\n{ex.Message}",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                IsRefreshing = false;
            }
        }

        // ==================== FILTROS LOCAIS ====================

        private void AplicarFiltrosLocal()
        {
            System.Diagnostics.Debug.WriteLine($"🔧 Aplicando filtros localmente...");
            System.Diagnostics.Debug.WriteLine($"   Situação: {SituacaoSelecionada}");
            System.Diagnostics.Debug.WriteLine($"   Unidade: {UnidadeSelecionada}");
            System.Diagnostics.Debug.WriteLine($"   Busca: {TextoBusca}");
            System.Diagnostics.Debug.WriteLine($"   Base: {Usuarios?.Count ?? 0} usuários");

            if (Usuarios == null || Usuarios.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Nenhum usuário para filtrar");
                UsuariosFiltrados = new ObservableCollection<Usuario>();
                return;
            }

            var filtrados = Usuarios.AsEnumerable();

            // 1. Filtrar por SITUAÇÃO
            if (!string.IsNullOrEmpty(SituacaoSelecionada) && SituacaoSelecionada != "Todos")
            {
                switch (SituacaoSelecionada.ToUpper())
                {
                    case "TRABALHANDO":
                        filtrados = filtrados.Where(u => u.EstaAtivo);
                        System.Diagnostics.Debug.WriteLine($"   ✅ Filtrando ativos: {filtrados.Count()}");
                        break;

                    case "DEMITIDOS":
                    case "DEMITIDO":
                        filtrados = filtrados.Where(u => u.EstaDemitido);
                        System.Diagnostics.Debug.WriteLine($"   ❌ Filtrando demitidos: {filtrados.Count()}");
                        break;

                    case "APOSENTADORIA POR INVALIDEZ":
                    case "APOSENTADOS":
                        filtrados = filtrados.Where(u => u.EstaAposentadoPorInvalidez);
                        System.Diagnostics.Debug.WriteLine($"   🏥 Filtrando aposentados: {filtrados.Count()}");
                        break;

                    case "AUXÍLIO DOENÇA":
                    case "AUXILIO DOENÇA":
                        filtrados = filtrados.Where(u => u.EstaEmAuxilioDoenca);
                        System.Diagnostics.Debug.WriteLine($"   🤕 Filtrando auxílio: {filtrados.Count()}");
                        break;
                }
            }

            // 2. Filtrar por UNIDADE
            if (!string.IsNullOrEmpty(UnidadeSelecionada) && UnidadeSelecionada != "Todas as Unidades")
            {
                filtrados = filtrados.Where(u => u.UnidadeGrupo == UnidadeSelecionada);
                System.Diagnostics.Debug.WriteLine($"   🏢 Após filtro unidade: {filtrados.Count()}");
            }

            // 3. Filtrar por BUSCA
            if (!string.IsNullOrWhiteSpace(TextoBusca))
            {
                var busca = TextoBusca.ToLower();
                filtrados = filtrados.Where(u =>
                    u.Nome.ToLower().Contains(busca) ||
                    u.Cargo.ToLower().Contains(busca) ||
                    (!string.IsNullOrEmpty(u.UnidadeGrupo) && u.UnidadeGrupo.ToLower().Contains(busca))
                );
                System.Diagnostics.Debug.WriteLine($"   🔍 Após busca: {filtrados.Count()}");
            }

            var resultado = filtrados.ToList();
            UsuariosFiltrados = new ObservableCollection<Usuario>(resultado);

            System.Diagnostics.Debug.WriteLine($"✅ Filtros aplicados! Resultado: {UsuariosFiltrados.Count} usuários");
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
                       $"Situação Real: '{usuario.DescricaoSituacao}'\n" +
                       $"Ativo: {(usuario.EstaAtivo ? "Sim" : "Não")}\n" +
                       $"Demitido: {(usuario.EstaDemitido ? "Sim" : "Não")}\n" +
                       $"Aposentado: {(usuario.EstaAposentadoPorInvalidez ? "Sim" : "Não")}\n" +
                       $"Auxílio Doença: {(usuario.EstaEmAuxilioDoenca ? "Sim" : "Não")}";

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
                System.Diagnostics.Debug.WriteLine($"🗑️ Excluindo: {usuario.Nome}");

                Usuarios.Remove(usuario);
                AplicarFiltrosLocal();

                TotalUsuarios--;
                if (usuario.EstaAtivo) TotalTrabalhando--;
                else if (usuario.EstaDemitido) TotalDemitidos--;
                else if (usuario.EstaAposentadoPorInvalidez) TotalAposentados--;
                else if (usuario.EstaEmAuxilioDoenca) TotalAuxilioDoenca--;

                await Application.Current!.MainPage!.DisplayAlert(
                    "✅ Sucesso",
                    $"Usuário '{usuario.Nome}' excluído!",
                    "OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                await Application.Current!.MainPage!.DisplayAlert("❌ Erro", ex.Message, "OK");
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
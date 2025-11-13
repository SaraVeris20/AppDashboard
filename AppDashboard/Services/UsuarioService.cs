using AppDashboard.Data;
using AppDashboard.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace AppDashboard.Services
{
    public class UsuarioService
    {
        private readonly AppDbContext _context;
        private ObservableCollection<Usuario> _usuariosCache;

        public UsuarioService()
        {
            _context = new AppDbContext();
            _usuariosCache = new ObservableCollection<Usuario>();
        }

        // ==================== TESTE DE CONEXÃO ====================

        public async Task<bool> TestarConexaoAsync()
        {
            try
            {
                await _context.Database.CanConnectAsync();
                System.Diagnostics.Debug.WriteLine("✅ Conexão com MySQL estabelecida!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao conectar: {ex.Message}");
                return false;
            }
        }

        // ==================== LISTAGEM COM FILTROS DE SITUAÇÃO ====================

        /// <summary>
        /// Obter TODOS os usuários (todas as situações)
        /// </summary>
        public async Task<ObservableCollection<Usuario>> ObterTodosUsuariosAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando TODOS os usuários da tabela rhdataset...");

                var usuarios = await _context.Usuarios
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários encontrados");

                _usuariosCache.Clear();
                foreach (var usuario in usuarios)
                {
                    if (string.IsNullOrEmpty(usuario.FotoUrl))
                    {
                        usuario.FotoUrl = ObterGravatarPadrao();
                    }
                    _usuariosCache.Add(usuario);

                    System.Diagnostics.Debug.WriteLine(
                        $"  👤 {usuario.Nome} | {usuario.StatusEmoji} {usuario.DescricaoSituacao}");
                }

                return _usuariosCache;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return _usuariosCache;
            }
        }

        /// <summary>
        /// Obter apenas usuários TRABALHANDO
        /// </summary>
        public async Task<ObservableCollection<Usuario>> ObterUsuariosTrabalhando()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários TRABALHANDO...");

                var usuarios = await _context.Usuarios
                    .Where(u => u.DescricaoSituacao == "Trabalhando")
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários trabalhando");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<Usuario>();
            }
        }

        /// <summary>
        /// Obter apenas usuários DEMITIDOS
        /// </summary>
        public async Task<ObservableCollection<Usuario>> ObterUsuariosDemitidos()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários DEMITIDOS...");

                var usuarios = await _context.Usuarios
                    .Where(u => u.DescricaoSituacao == "Demitido")
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários demitidos");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<Usuario>();
            }
        }

        /// <summary>
        /// Obter apenas usuários em APOSENTADORIA POR INVALIDEZ
        /// </summary>
        public async Task<ObservableCollection<Usuario>> ObterUsuariosAposentadosPorInvalidez()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários APOSENTADOS POR INVALIDEZ...");

                var usuarios = await _context.Usuarios
                    .Where(u => u.DescricaoSituacao == "Aposentadoria por Invalidez")
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários aposentados por invalidez");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<Usuario>();
            }
        }

        /// <summary>
        /// Obter apenas usuários em AUXÍLIO DOENÇA
        /// </summary>
        public async Task<ObservableCollection<Usuario>> ObterUsuariosAuxilioDoenca()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Buscando usuários em AUXÍLIO DOENÇA...");

                var usuarios = await _context.Usuarios
                    .Where(u => u.DescricaoSituacao == "Auxilio Doença" ||
                               u.DescricaoSituacao == "Auxílio Doença")
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($"✅ {usuarios.Count} usuários em auxílio doença");

                return CriarCollectionComFotos(usuarios);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro: {ex.Message}");
                return new ObservableCollection<Usuario>();
            }
        }

        /// <summary>
        /// Filtrar usuários por situação
        /// </summary>
        public async Task<ObservableCollection<Usuario>> FiltrarPorSituacaoAsync(string situacao)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Filtrando por situação: {situacao}");

            switch (situacao?.ToUpper())
            {
                case "TRABALHANDO":
                    return await ObterUsuariosTrabalhando();

                case "DEMITIDOS":
                case "DEMITIDO":
                    return await ObterUsuariosDemitidos();

                case "APOSENTADORIA POR INVALIDEZ":
                case "APOSENTADOS":
                    return await ObterUsuariosAposentadosPorInvalidez();

                case "AUXÍLIO DOENÇA":
                case "AUXILIO DOENÇA":
                case "AUXILIO DOENCA":
                    return await ObterUsuariosAuxilioDoenca();

                case "TODOS":
                default:
                    return await ObterTodosUsuariosAsync();
            }
        }

        // ==================== OUTROS FILTROS ====================

        public ObservableCollection<Usuario> ObterUsuariosPorUnidade(string unidadeGrupo)
        {
            if (string.IsNullOrEmpty(unidadeGrupo) || unidadeGrupo == "Todas as Unidades")
            {
                return _usuariosCache;
            }

            var filtrados = _usuariosCache
                .Where(u => u.UnidadeGrupo == unidadeGrupo)
                .ToList();

            return new ObservableCollection<Usuario>(filtrados);
        }

        public async Task<ObservableCollection<Usuario>> ObterUsuariosPorCargoAsync(string cargo)
        {
            try
            {
                var usuarios = await _context.Usuarios
                    .Where(u => u.Cargo == cargo)
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                return CriarCollectionComFotos(usuarios);
            }
            catch
            {
                return new ObservableCollection<Usuario>();
            }
        }

        // ==================== ESTATÍSTICAS ====================

        public async Task<Dictionary<string, int>> ObterEstatisticasPorSituacaoAsync()
        {
            try
            {
                var stats = new Dictionary<string, int>();

                // Total geral
                var total = await _context.Usuarios.CountAsync();
                stats["Total"] = total;

                // Por situação específica
                var trabalhando = await _context.Usuarios
                    .CountAsync(u => u.DescricaoSituacao == "Trabalhando");
                stats["Trabalhando"] = trabalhando;

                var demitidos = await _context.Usuarios
                    .CountAsync(u => u.DescricaoSituacao == "Demitido");
                stats["Demitidos"] = demitidos;

                var aposentados = await _context.Usuarios
                    .CountAsync(u => u.DescricaoSituacao == "Aposentadoria por Invalidez");
                stats["Aposentadoria por Invalidez"] = aposentados;

                var auxilioDoenca = await _context.Usuarios
                    .CountAsync(u => u.DescricaoSituacao == "Auxilio Doença" ||
                                   u.DescricaoSituacao == "Auxílio Doença");
                stats["Auxílio Doença"] = auxilioDoenca;

                System.Diagnostics.Debug.WriteLine("📊 ESTATÍSTICAS:");
                foreach (var stat in stats)
                {
                    System.Diagnostics.Debug.WriteLine($"  {stat.Key}: {stat.Value}");
                }

                return stats;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao obter estatísticas: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        public async Task<List<string>> ObterListaCargosAsync()
        {
            try
            {
                return await _context.Usuarios
                    .Select(u => u.Cargo)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<List<string>> ObterListaUnidadesAsync()
        {
            try
            {
                var unidades = await _context.Usuarios
                    .Where(u => !string.IsNullOrEmpty(u.UnidadeGrupo))
                    .Select(u => u.UnidadeGrupo!)
                    .Distinct()
                    .OrderBy(u => u)
                    .ToListAsync();

                unidades.Insert(0, "Todas as Unidades");
                return unidades;
            }
            catch
            {
                return new List<string> { "Todas as Unidades" };
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        public ObservableCollection<Usuario> ObterTodosUsuarios()
        {
            return _usuariosCache;
        }

        public Usuario? ObterUsuarioPorId(int id)
        {
            return _usuariosCache.FirstOrDefault(u => u.Id == id);
        }

        public List<string> ObterSituacoesDisponiveis()
        {
            return new List<string>
            {
                "Todos",
                "Trabalhando",
                "Demitidos",
                "Aposentadoria por Invalidez",
                "Auxílio Doença"
            };
        }

        public List<string> ObterUnidadesGrupos()
        {
            var unidades = _usuariosCache
                .Where(u => !string.IsNullOrEmpty(u.UnidadeGrupo))
                .Select(u => u.UnidadeGrupo!)
                .Distinct()
                .OrderBy(u => u)
                .ToList();

            unidades.Insert(0, "Todas as Unidades");

            if (unidades.Count == 1)
            {
                return new List<string> { "Todas as Unidades" };
            }

            return unidades;
        }

        private ObservableCollection<Usuario> CriarCollectionComFotos(List<Usuario> usuarios)
        {
            var result = new ObservableCollection<Usuario>();
            foreach (var usuario in usuarios)
            {
                if (string.IsNullOrEmpty(usuario.FotoUrl))
                {
                    usuario.FotoUrl = ObterGravatarPadrao();
                }
                result.Add(usuario);
            }
            return result;
        }

        private static string ObterGravatarPadrao()
        {
            return "https://www.gravatar.com/avatar/?d=identicon&s=200";
        }

        // ==================== ADICIONAR USUÁRIO ====================

        public async Task<bool> AdicionarUsuario(Usuario usuario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"➕ Adicionando usuário: {usuario.Nome}");

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Usuário {usuario.Nome} adicionado com sucesso!");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erro ao adicionar usuário: {ex.Message}");
                return false;
            }
        }

        public List<string> ObterCargosDisponiveis()
        {
            var cargos = _usuariosCache
                .Select(u => u.Cargo)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (cargos.Count == 0)
            {
                // Lista padrão de cargos caso o cache esteja vazio
                return new List<string>
                {
                    "Analista",
                    "Assistente",
                    "Coordenador",
                    "Desenvolvedor",
                    "Diretor",
                    "Gerente",
                    "Supervisor",
                    "Técnico"
                };
            }

            return cargos;
        }

        // ==================== DIAGNÓSTICO ====================

        public async Task<string> DiagnosticarBancoDadosAsync()
        {
            try
            {
                var resultado = new System.Text.StringBuilder();

                bool conectado = await _context.Database.CanConnectAsync();
                resultado.AppendLine($"Conexão: {(conectado ? "✅ OK" : "❌ FALHOU")}");
                resultado.AppendLine($"Banco: rhsenior_heicomp");
                resultado.AppendLine($"Tabela: rhdataset");
                resultado.AppendLine($"Coluna Situação: Descrição (Situação)");
                resultado.AppendLine();

                var total = await _context.Usuarios.CountAsync();
                resultado.AppendLine($"Total de registros: {total}");
                resultado.AppendLine();

                var stats = await ObterEstatisticasPorSituacaoAsync();
                resultado.AppendLine("Estatísticas por Situação:");
                foreach (var stat in stats)
                {
                    resultado.AppendLine($"  • {stat.Key}: {stat.Value}");
                }
                resultado.AppendLine();

                var primeiros = await _context.Usuarios.Take(3).ToListAsync();
                resultado.AppendLine($"Primeiros 3 registros:");
                foreach (var u in primeiros)
                {
                    resultado.AppendLine($"  {u.StatusEmoji} ID: {u.Id} | Nome: {u.Nome} | Situação: {u.DescricaoSituacao}");
                }

                return resultado.ToString();
            }
            catch (Exception ex)
            {
                return $"❌ Erro: {ex.Message}\n\nStack: {ex.StackTrace}";
            }
        }
    }
}
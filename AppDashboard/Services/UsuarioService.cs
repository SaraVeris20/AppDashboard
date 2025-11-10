using AppDashboard.Models;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace AppDashboard.Services
{
    public class UsuarioService
    {
        private const string USUARIOS_KEY = "usuarios_list";
        private ObservableCollection<Usuario> _usuarios;

        public UsuarioService()
        {
            _usuarios = new ObservableCollection<Usuario>();
            CarregarUsuarios();

            // Se não houver usuários, adiciona dados de exemplo
            if (_usuarios.Count == 0)
            {
                InicializarDadosExemplo();
            }
        }

        private void InicializarDadosExemplo()
        {
            var usuariosExemplo = new List<Usuario>
            {
                new Usuario
                {
                    Nome = "Ana Silva",
                    Email = "ana.silva@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "ana_foto.jpg",
                    UnidadeGrupo = "Unidade A"
                },
                new Usuario
                {
                    Nome = "Carlos Santos",
                    Email = "carlos.santos@empresa.com",
                    Cargo = "Vice-Reitor",
                    FotoUrl = "carlos_foto.jpg",
                    UnidadeGrupo = "Unidade B"
                },
                new Usuario
                {
                    Nome = "Maria Oliveira",
                    Email = "maria.oliveira@empresa.com",
                    Cargo = "Diretor",
                    FotoUrl = "maria_foto.jpg",
                    UnidadeGrupo = "Unidade A"
                },
                new Usuario
                {
                    Nome = "João Costa",
                    Email = "joao.costa@empresa.com",
                    Cargo = "Coordenador",
                    FotoUrl = "joao_foto.jpg",
                    UnidadeGrupo = "Grupo 1"
                },
                new Usuario
                {
                    Nome = "Fernanda Lima",
                    Email = "fernanda.lima@empresa.com",
                    Cargo = "Professor",
                    FotoUrl = "fernanda_foto.jpg",
                    UnidadeGrupo = "Grupo 2"
                }
            };

            foreach (var usuario in usuariosExemplo)
            {
                _usuarios.Add(usuario);
            }

            Task.Run(async () => await SalvarUsuarios());
        }

        public ObservableCollection<Usuario> ObterTodosUsuarios()
        {
            return _usuarios;
        }

        public Usuario? ObterUsuarioPorId(string id)
        {
            return _usuarios.FirstOrDefault(u => u.Id == id);
        }

        public ObservableCollection<Usuario> ObterUsuariosPorUnidade(string unidadeGrupo)
        {
            if (string.IsNullOrEmpty(unidadeGrupo) || unidadeGrupo == "Todas as Unidades")
            {
                return _usuarios;
            }

            var usuariosFiltrados = _usuarios.Where(u => u.UnidadeGrupo == unidadeGrupo).ToList();
            return new ObservableCollection<Usuario>(usuariosFiltrados);
        }

        public async Task<bool> AdicionarUsuario(Usuario usuario)
        {
            try
            {
                _usuarios.Add(usuario);
                await SalvarUsuarios();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar usuário: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AtualizarUsuario(Usuario usuario)
        {
            try
            {
                var usuarioExistente = ObterUsuarioPorId(usuario.Id);
                if (usuarioExistente != null)
                {
                    int index = _usuarios.IndexOf(usuarioExistente);
                    _usuarios[index] = usuario;
                    await SalvarUsuarios();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar usuário: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoverUsuario(string id)
        {
            try
            {
                var usuario = ObterUsuarioPorId(id);
                if (usuario != null)
                {
                    _usuarios.Remove(usuario);
                    await SalvarUsuarios();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao remover usuário: {ex.Message}");
                return false;
            }
        }

        public List<Usuario> BuscarPorNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                return _usuarios.ToList();

            return _usuarios
                .Where(u => u.Nome.Contains(nome, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<string> ObterCargosDisponiveis()
        {
            return new List<string>
            {
                "Reitor",
                "Vice-Reitor",
                "Diretor",
                "Coordenador",
                "Professor",
                "Gerente",
                "Analista",
                "Assistente"
            };
        }

        public List<string> ObterUnidadesGrupos()
        {
            return new List<string>
            {
                "Todas as Unidades",
                "Unidade A",
                "Unidade B",
                "Unidade C",
                "Grupo 1",
                "Grupo 2",
                "Grupo 3"
            };
        }

        public async Task LimparTodos()
        {
            _usuarios.Clear();
            await SalvarUsuarios();
        }

        private async Task SalvarUsuarios()
        {
            try
            {
                var json = JsonSerializer.Serialize(_usuarios.ToList());
                await SecureStorage.SetAsync(USUARIOS_KEY, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao salvar usuários: {ex.Message}");
            }
        }

        private void CarregarUsuarios()
        {
            try
            {
                var json = SecureStorage.GetAsync(USUARIOS_KEY).Result;
                if (!string.IsNullOrEmpty(json))
                {
                    var usuarios = JsonSerializer.Deserialize<List<Usuario>>(json);
                    if (usuarios != null)
                    {
                        _usuarios.Clear();
                        foreach (var usuario in usuarios)
                        {
                            _usuarios.Add(usuario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar usuários: {ex.Message}");
            }
        }
    }
}
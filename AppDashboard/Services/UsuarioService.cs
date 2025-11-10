using AppDashboard.Models;
using System.Collections.ObjectModel;

namespace AppDashboard.Services
{
    public class UsuarioService
    {
        private ObservableCollection<Usuario> _usuarios;
        private int _proximoId = 6;

        public UsuarioService()
        {
            _usuarios = new ObservableCollection<Usuario>
            {
                new Usuario
                {
                    Id = 1,
                    Nome = "Ana Silva",
                    Email = "ana.silva@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "ana_foto.jpg"
                },
                new Usuario
                {
                    Id = 2,
                    Nome = "Carlos Santos",
                    Email = "carlos.santos@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "carlos_foto.jpg"
                },
                new Usuario
                {
                    Id = 3,
                    Nome = "Maria Oliveira",
                    Email = "maria.oliveira@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "maria_foto.jpg"
                },
                new Usuario
                {
                    Id = 4,
                    Nome = "João Costa",
                    Email = "joao.costa@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "joao_foto.jpg"
                },
                new Usuario
                {
                    Id = 5,
                    Nome = "Fernanda Lima",
                    Email = "fernanda.lima@empresa.com",
                    Cargo = "Reitor",
                    FotoUrl = "fernanda_foto.jpg"
                }
            };
        }

        public ObservableCollection<Usuario> ObterTodosUsuarios()
        {
            return _usuarios;
        }

        public Usuario? ObterUsuarioPorId(int id)
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

        public bool AdicionarUsuario(Usuario usuario)
        {
            try
            {
                usuario.Id = _proximoId++;
                usuario.DataCriacao = DateTime.Now;
                _usuarios.Add(usuario);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AtualizarUsuario(Usuario usuario)
        {
            try
            {
                var usuarioExistente = ObterUsuarioPorId(usuario.Id);
                if (usuarioExistente != null)
                {
                    int index = _usuarios.IndexOf(usuarioExistente);
                    _usuarios[index] = usuario;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoverUsuario(int id)
        {
            try
            {
                var usuario = ObterUsuarioPorId(id);
                if (usuario != null)
                {
                    _usuarios.Remove(usuario);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
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
    }
}
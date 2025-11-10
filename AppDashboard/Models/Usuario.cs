namespace AppDashboard.Models
{
    public class Usuario
    {
        public string Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string FotoUrl { get; set; } = string.Empty;
        public string? UnidadeGrupo { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;

        public Usuario()
        {
            Id = Guid.NewGuid().ToString();
            DataCriacao = DateTime.Now;
        }

        // Para exibição em listas
        public string ResumoInfo => $"{Nome} - {Cargo}";
    }
}
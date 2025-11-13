using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppDashboard.Models
{
    [Table("rhdataset")]
    public class Usuario
    {
        [Key]
        [Column("cadastro")]
        public int Id { get; set; }

        [Column("nome")]
        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;

        [Column("cargo")]
        [Required]
        [MaxLength(100)]
        public string Cargo { get; set; } = string.Empty;

        [Column("Descrição (Situação)")]
        [MaxLength(100)]
        public string? DescricaoSituacao { get; set; }

        [Column("Descrição (C.Custo)")]
        [MaxLength(200)]
        public string? UnidadeGrupo { get; set; }

        [Column("foto_url")]
        [MaxLength(500)]
        public string? FotoUrl { get; set; }

        // Para exibir email fictício na interface
        [NotMapped]
        public string Email => $"{Nome.ToLower().Replace(" ", ".")}@empresa.com";

        // ==================== PROPRIEDADES CALCULADAS (NotMapped) ====================

        [NotMapped]
        public bool EstaAtivo
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Trabalhando", StringComparison.OrdinalIgnoreCase);
            }
        }

        [NotMapped]
        public bool EstaDemitido
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Demitido", StringComparison.OrdinalIgnoreCase);
            }
        }

        [NotMapped]
        public bool EstaAposentadoPorInvalidez
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Aposentadoria por Invalidez", StringComparison.OrdinalIgnoreCase);
            }
        }

        [NotMapped]
        public bool EstaEmAuxilioDoenca
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return false;

                return DescricaoSituacao.Equals("Auxilio Doença", StringComparison.OrdinalIgnoreCase) ||
                       DescricaoSituacao.Equals("Auxílio Doença", StringComparison.OrdinalIgnoreCase);
            }
        }

        [NotMapped]
        public string StatusDescricao
        {
            get
            {
                if (string.IsNullOrEmpty(DescricaoSituacao))
                    return "Não Informado";

                return DescricaoSituacao;
            }
        }

        [NotMapped]
        public string StatusEmoji
        {
            get
            {
                if (EstaAtivo) return "✅";
                if (EstaDemitido) return "❌";
                if (EstaAposentadoPorInvalidez) return "🏥";
                if (EstaEmAuxilioDoenca) return "🤕";
                return "❓";
            }
        }

        [NotMapped]
        public string StatusCor
        {
            get
            {
                if (EstaAtivo) return "#4CAF50"; // Verde
                if (EstaDemitido) return "#F44336"; // Vermelho
                if (EstaAposentadoPorInvalidez) return "#9C27B0"; // Roxo
                if (EstaEmAuxilioDoenca) return "#FF9800"; // Laranja
                return "#9E9E9E"; // Cinza
            }
        }

        [NotMapped]
        public string StatusCorFundo
        {
            get
            {
                if (EstaAtivo) return "#E8F5E9";
                if (EstaDemitido) return "#FFEBEE";
                if (EstaAposentadoPorInvalidez) return "#F3E5F5";
                if (EstaEmAuxilioDoenca) return "#FFF3E0";
                return "#F5F5F5";
            }
        }
    }
}
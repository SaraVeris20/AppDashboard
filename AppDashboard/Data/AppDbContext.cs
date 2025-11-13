using AppDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace AppDashboard.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // ⚠️ SUBSTITUA ESTAS INFORMAÇÕES PELAS SUAS CREDENCIAIS REAIS DO AWS RDS
                var connectionString = "Server=cursoslivres.cl0yia62segf.sa-east-1.rds.amazonaws.com;" +
                                      "Port=3306;" +
                                      "Database=rhsenior_heicomp;" +
                                      "User=heicomp;" +
                                      "Password=heicomp2025;" +
                                      "SslMode=Required;" +
                                      "AllowPublicKeyRetrieval=True;";

                optionsBuilder.UseMySql(
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions => mySqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null)
                );

                // Log de conexão para debug
#if DEBUG
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
#endif
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração da tabela Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                // Nome da tabela
                entity.ToTable("rhdataset");

                // Chave primária
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("cadastro");

                // Mapeamento das colunas
                entity.Property(e => e.Nome)
                    .HasColumnName("nome")
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Cargo)
                    .HasColumnName("cargo")
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DescricaoSituacao)
                    .HasColumnName("Descrição (Situação)")
                    .HasMaxLength(100);

                entity.Property(e => e.UnidadeGrupo)
                    .HasColumnName("Descrição (C.Custo)")
                    .HasMaxLength(200);

                entity.Property(e => e.FotoUrl)
                    .HasColumnName("foto_url")
                    .HasMaxLength(500);

                // Propriedades calculadas (NotMapped) são ignoradas automaticamente
                entity.Ignore(e => e.EstaAtivo);
                entity.Ignore(e => e.EstaDemitido);
                entity.Ignore(e => e.EstaAposentadoPorInvalidez);
                entity.Ignore(e => e.EstaEmAuxilioDoenca);
                entity.Ignore(e => e.StatusDescricao);
                entity.Ignore(e => e.StatusEmoji);
                entity.Ignore(e => e.StatusCor);
                entity.Ignore(e => e.StatusCorFundo);
            });
        }
    }
}

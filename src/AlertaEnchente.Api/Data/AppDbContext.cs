using AlertaEnchente.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AlertaEnchente.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Regiao> Regioes => Set<Regiao>();
    public DbSet<LeituraNivel> Leituras => Set<LeituraNivel>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Regiao>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasMaxLength(60);
            e.Property(r => r.Nome).HasMaxLength(100).IsRequired();
            e.Property(r => r.Corrego).HasMaxLength(100).IsRequired();
            e.HasMany(r => r.Leituras)
                .WithOne()
                .HasForeignKey(l => l.RegiaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeituraNivel>(e =>
        {
            e.HasKey(l => l.Id);
            e.HasIndex(l => new { l.RegiaoId, l.MedidoEm });
        });

        modelBuilder.Entity<Ocorrencia>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Local).HasMaxLength(120).IsRequired();
            e.Property(o => o.Descricao).HasMaxLength(500);
            e.Property(o => o.Tipo).HasConversion<string>().HasMaxLength(30);
            e.HasOne<Regiao>()
                .WithMany()
                .HasForeignKey(o => o.RegiaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

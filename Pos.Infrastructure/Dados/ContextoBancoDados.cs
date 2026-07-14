using Microsoft.EntityFrameworkCore;
using Pos.Domain;

namespace Pos.Infrastructure.Dados;

/// <summary>
/// Representa a sessão do Entity Framework com o banco de dados da aplicação.
/// </summary>
public sealed class ContextoBancoDados(DbContextOptions<ContextoBancoDados> opcoes)
    : DbContext(opcoes)
{
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(produto =>
        {
            produto.Property(p => p.Codigo).IsRequired();
            produto.Property(p => p.Descricao).IsRequired();
            produto.Property(p => p.PrecoCusto).HasPrecision(18, 2);
            produto.Property(p => p.PrecoVenda).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Usuario>(usuario =>
        {
            usuario.Property(u => u.NomeUsuario).IsRequired();
            usuario.Property(u => u.NomeCompleto).IsRequired();
            usuario.Property(u => u.SenhaHash).IsRequired();
            usuario.Property(u => u.Papel).HasConversion<string>();
            usuario.HasIndex(u => u.NomeUsuario).IsUnique();
        });
    }
}

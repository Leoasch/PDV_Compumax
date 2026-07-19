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
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

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

        modelBuilder.Entity<Venda>(venda =>
        {
            venda.Property(v => v.UsuarioNomeCompleto).IsRequired();
            venda.Property(v => v.Total).HasPrecision(18, 2);
            venda.Property(v => v.FormaPagamento).HasConversion<string>();

            venda.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.SetNull);

            venda.HasMany(v => v.Itens)
                .WithOne()
                .HasForeignKey(i => i.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemVenda>(item =>
        {
            item.Property(i => i.ProdutoCodigo).IsRequired();
            item.Property(i => i.ProdutoDescricao).IsRequired();
            item.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            item.Property(i => i.PrecoDesconto).HasPrecision(18, 2);

            item.HasOne<Produto>()
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

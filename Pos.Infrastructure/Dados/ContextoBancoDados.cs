using Microsoft.EntityFrameworkCore;

namespace Pos.Infrastructure.Dados;

/// <summary>
/// Representa a sessão do Entity Framework com o banco de dados da aplicação.
/// </summary>
public sealed class ContextoBancoDados(DbContextOptions<ContextoBancoDados> opcoes)
    : DbContext(opcoes)
{
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Pos.Infrastructure.Dados;

/// <summary>
/// Centraliza o registro dos serviços de persistência.
/// </summary>
public static class ConfiguracaoPersistencia
{
    public static IServiceCollection AdicionarPersistenciaSqlite(
        this IServiceCollection servicos,
        string cadeiaDeConexao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadeiaDeConexao);

        servicos.AddDbContext<ContextoBancoDados>(opcoes =>
            opcoes.UseSqlite(cadeiaDeConexao));

        return servicos;
    }
}

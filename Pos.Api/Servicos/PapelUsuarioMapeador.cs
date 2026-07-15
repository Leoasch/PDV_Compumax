using DominioPapel = Pos.Domain.PapelUsuario;
using CompartilhadoPapel = Pos.Shared.PapelUsuario;

namespace Pos.Api.Servicos;

public static class PapelUsuarioMapeador
{
    public static DominioPapel ParaDominio(this CompartilhadoPapel papel) => papel switch
    {
        CompartilhadoPapel.Admin => DominioPapel.Admin,
        CompartilhadoPapel.Caixa => DominioPapel.Caixa,
        _ => throw new NotSupportedException($"Papel '{papel}' não possui mapeamento para o domínio.")
    };

    public static CompartilhadoPapel ParaCompartilhado(this DominioPapel papel) => papel switch
    {
        DominioPapel.Admin => CompartilhadoPapel.Admin,
        DominioPapel.Caixa => CompartilhadoPapel.Caixa,
        _ => throw new NotSupportedException($"Papel '{papel}' não possui mapeamento para o contrato compartilhado.")
    };
}

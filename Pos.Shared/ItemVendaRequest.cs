using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record ItemVendaRequest(
    int ProdutoId,
    [Range(1, int.MaxValue)] int Quantidade,
    [Range(0, double.MaxValue)] decimal? PrecoDesconto = null);

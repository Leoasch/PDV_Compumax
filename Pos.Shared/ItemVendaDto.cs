namespace Pos.Shared;

public record ItemVendaDto(
    int? ProdutoId,
    string ProdutoCodigo,
    string ProdutoDescricao,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal,
    decimal? PrecoDesconto);

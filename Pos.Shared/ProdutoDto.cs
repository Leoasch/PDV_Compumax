namespace Pos.Shared;

public record ProdutoDto(
    int Id,
    string Codigo,
    string Descricao,
    decimal PrecoCusto,
    decimal PrecoVenda,
    int QuantidadeEstoque);

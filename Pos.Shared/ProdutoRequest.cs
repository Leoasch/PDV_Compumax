using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record ProdutoRequest(
    [Required] string Codigo,
    [Required] string Descricao,
    [Range(0, double.MaxValue)] decimal PrecoCusto,
    [Range(0, double.MaxValue)] decimal PrecoVenda,
    [Range(0, int.MaxValue)] int QuantidadeEstoque);

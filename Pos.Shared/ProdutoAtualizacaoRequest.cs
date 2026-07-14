using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record ProdutoAtualizacaoRequest(
    string? Codigo,
    string? Descricao,
    [Range(0, double.MaxValue)] decimal? PrecoCusto,
    [Range(0, double.MaxValue)] decimal? PrecoVenda,
    [Range(0, int.MaxValue)] int? QuantidadeEstoque);

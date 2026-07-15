using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record VendaRequest(
    [Required, MinLength(1)] List<ItemVendaRequest> Itens,
    [Required] FormaPagamento? FormaPagamento);

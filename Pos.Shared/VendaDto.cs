namespace Pos.Shared;

public record VendaDto(
    int Id,
    int? UsuarioId,
    string UsuarioNomeCompleto,
    DateTime DataHora,
    decimal Total,
    FormaPagamento FormaPagamento,
    bool Cancelada,
    DateTime? DataCancelamento,
    List<ItemVendaDto> Itens);

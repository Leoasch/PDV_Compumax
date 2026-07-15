namespace Pos.Shared;

public record VendaResumoDto(
    int Id,
    string UsuarioNomeCompleto,
    DateTime DataHora,
    decimal Total,
    FormaPagamento FormaPagamento,
    bool Cancelada);

namespace Pos.Shared;

public record LoginResponse(
    string Token,
    DateTime ExpiraEm,
    int UsuarioId,
    string NomeUsuario,
    string NomeCompleto,
    string Papel);

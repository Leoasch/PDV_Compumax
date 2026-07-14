namespace Pos.Shared;

public record UsuarioDto(
    int Id,
    string NomeUsuario,
    string NomeCompleto,
    PapelUsuario Papel,
    bool Ativo);

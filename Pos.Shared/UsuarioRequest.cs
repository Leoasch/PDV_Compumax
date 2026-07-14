using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record UsuarioRequest(
    [Required] string NomeUsuario,
    [Required] string NomeCompleto,
    [Required, MinLength(6)] string Senha,
    [Required] PapelUsuario? Papel);

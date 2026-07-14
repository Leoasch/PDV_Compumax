using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record LoginRequest(
    [Required] string NomeUsuario,
    [Required] string Senha);

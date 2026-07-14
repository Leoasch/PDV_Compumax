using System.ComponentModel.DataAnnotations;

namespace Pos.Shared;

public record UsuarioAtualizacaoRequest(
    string? NomeUsuario,
    string? NomeCompleto,
    [MinLength(6)] string? Senha,
    PapelUsuario? Papel,
    bool? Ativo);

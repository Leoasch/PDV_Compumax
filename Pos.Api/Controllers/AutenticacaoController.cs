using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Servicos;
using Pos.Domain;
using Pos.Infrastructure.Dados;
using Pos.Shared;

namespace Pos.Api.Controllers;

[ApiController]
[Route("autenticacao")]
public class AutenticacaoController : ControllerBase
{
    private readonly ContextoBancoDados _contextoBancoDados;
    private readonly ServicoToken _servicoToken;

    public AutenticacaoController(ContextoBancoDados contextoBancoDados, ServicoToken servicoToken)
    {
        _contextoBancoDados = contextoBancoDados;
        _servicoToken = servicoToken;
    }

    [HttpPost("entrar")]
    public async Task<ActionResult<LoginResponse>> Entrar([FromBody] LoginRequest request)
    {
        var usuario = await _contextoBancoDados.Usuarios
            .SingleOrDefaultAsync(u => u.NomeUsuario == request.NomeUsuario && u.Ativo);

        if (usuario is null)
        {
            return Unauthorized();
        }

        var hasher = new PasswordHasher<Usuario>();
        var resultado = hasher.VerifyHashedPassword(usuario, usuario.SenhaHash, request.Senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var (token, expiraEm) = _servicoToken.GerarToken(usuario);

        return Ok(new LoginResponse(
            token,
            expiraEm,
            usuario.Id,
            usuario.NomeUsuario,
            usuario.NomeCompleto,
            usuario.Papel.ToString()));
    }
}

using Microsoft.AspNetCore.Mvc;
using Pos.Infrastructure.Dados;

namespace Pos.Api.Controllers;

[ApiController]
[Route("banco-dados")]
public class BancoDadosController : ControllerBase
{
    private readonly ContextoBancoDados _contextoBancoDados;

    public BancoDadosController(ContextoBancoDados contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    [HttpGet("status")]
    public async Task<IActionResult> ObterStatus()
    {
        var conectado = await _contextoBancoDados.Database.CanConnectAsync();
        return Ok(new { sqlite = conectado });
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Servicos;
using Pos.Infrastructure.Dados;
using Pos.Shared;
using Usuario = Pos.Domain.Usuario;
using PapelUsuarioDominio = Pos.Domain.PapelUsuario;

namespace Pos.Api.Controllers;

[ApiController]
[Route("usuarios")]
[Authorize(Roles = "Admin")]
public class UsuariosController : ControllerBase
{
    private readonly ContextoBancoDados _contextoBancoDados;
    private readonly IPasswordHasher<Usuario> _hasher;

    public UsuariosController(ContextoBancoDados contextoBancoDados, IPasswordHasher<Usuario> hasher)
    {
        _contextoBancoDados = contextoBancoDados;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<IEnumerable<UsuarioDto>> Listar()
    {
        var usuarios = await _contextoBancoDados.Usuarios
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

        return usuarios
            .Select(u => new UsuarioDto(u.Id, u.NomeUsuario, u.NomeCompleto, u.Papel.ParaCompartilhado(), u.Ativo))
            .ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<UsuarioDto?> PegarUsuario(int id)
    {
        var usuario = await _contextoBancoDados.Usuarios.FindAsync(id);
        return usuario is null
            ? null
            : new UsuarioDto(usuario.Id, usuario.NomeUsuario, usuario.NomeCompleto, usuario.Papel.ParaCompartilhado(), usuario.Ativo);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Criar([FromBody] UsuarioRequest request)
    {
        var jaExiste = await _contextoBancoDados.Usuarios.AnyAsync(u => u.NomeUsuario == request.NomeUsuario);
        if (jaExiste)
        {
            return Problem(detail: $"Já existe um usuário com o nome '{request.NomeUsuario}'.", statusCode: StatusCodes.Status409Conflict);
        }

        var usuario = new Usuario
        {
            NomeUsuario = request.NomeUsuario,
            NomeCompleto = request.NomeCompleto,
            Papel = request.Papel!.Value.ParaDominio()
        };

        usuario.SenhaHash = _hasher.HashPassword(usuario, request.Senha);

        _contextoBancoDados.Usuarios.Add(usuario);
        await _contextoBancoDados.SaveChangesAsync();

        var dto = new UsuarioDto(usuario.Id, usuario.NomeUsuario, usuario.NomeCompleto, usuario.Papel.ParaCompartilhado(), usuario.Ativo);
        return CreatedAtAction(nameof(Listar), dto);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Editar(int id, [FromBody] UsuarioAtualizacaoRequest request)
    {
        var usuario = await _contextoBancoDados.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        var idUsuarioAutenticado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var alterandoPapelOuAtivo = (request.Papel is not null && request.Papel.Value != usuario.Papel.ParaCompartilhado())
            || (request.Ativo is not null && request.Ativo.Value != usuario.Ativo);

        if (id == idUsuarioAutenticado && alterandoPapelOuAtivo)
        {
            return Problem(detail: "Não é possível alterar seu próprio papel ou status de ativação.", statusCode: StatusCodes.Status409Conflict);
        }

        var eraAdminAtivo = usuario.Papel == PapelUsuarioDominio.Admin && usuario.Ativo;
        var continuaAdminAtivo = (request.Papel ?? usuario.Papel.ParaCompartilhado()) == PapelUsuario.Admin
            && (request.Ativo ?? usuario.Ativo);

        if (eraAdminAtivo && !continuaAdminAtivo && !await ExisteOutroAdminAtivoAsync(id))
        {
            return Problem(detail: "Não é possível remover o último administrador ativo do sistema.", statusCode: StatusCodes.Status409Conflict);
        }

        if (request.NomeUsuario is not null)
        {
            var jaExiste = await _contextoBancoDados.Usuarios.AnyAsync(u => u.Id != id && u.NomeUsuario == request.NomeUsuario);
            if (jaExiste)
            {
                return Problem(detail: $"Já existe um usuário com o nome '{request.NomeUsuario}'.", statusCode: StatusCodes.Status409Conflict);
            }

            usuario.NomeUsuario = request.NomeUsuario;
        }

        if (request.NomeCompleto is not null)
        {
            usuario.NomeCompleto = request.NomeCompleto;
        }

        if (request.Senha is not null)
        {
            usuario.SenhaHash = _hasher.HashPassword(usuario, request.Senha);
        }

        if (request.Papel is not null)
        {
            usuario.Papel = request.Papel.Value.ParaDominio();
        }

        if (request.Ativo is not null)
        {
            usuario.Ativo = request.Ativo.Value;
        }

        await _contextoBancoDados.SaveChangesAsync();

        return Ok(new UsuarioDto(usuario.Id, usuario.NomeUsuario, usuario.NomeCompleto, usuario.Papel.ParaCompartilhado(), usuario.Ativo));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var usuario = await _contextoBancoDados.Usuarios.FindAsync(id);
        if (usuario is null)
        {
            return NotFound();
        }

        var eraAdminAtivo = usuario.Papel == PapelUsuarioDominio.Admin && usuario.Ativo;
        if (eraAdminAtivo && !await ExisteOutroAdminAtivoAsync(id))
        {
            return Problem(detail: "Não é possível remover o último administrador ativo do sistema.", statusCode: StatusCodes.Status409Conflict);
        }

        _contextoBancoDados.Usuarios.Remove(usuario);
        await _contextoBancoDados.SaveChangesAsync();

        return NoContent();
    }

    private Task<bool> ExisteOutroAdminAtivoAsync(int idExcluido)
    {
        return _contextoBancoDados.Usuarios
            .AnyAsync(u => u.Id != idExcluido && u.Papel == PapelUsuarioDominio.Admin && u.Ativo);
    }
}

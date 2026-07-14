using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    public UsuariosController(ContextoBancoDados contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    [HttpGet]
    public async Task<IEnumerable<UsuarioDto>> Listar()
    {
        var usuarios = await _contextoBancoDados.Usuarios
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

        return usuarios
            .Select(u => new UsuarioDto(u.Id, u.NomeUsuario, u.NomeCompleto, (PapelUsuario)u.Papel, u.Ativo))
            .ToList();
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Criar([FromBody] UsuarioRequest request)
    {
        var jaExiste = await _contextoBancoDados.Usuarios.AnyAsync(u => u.NomeUsuario == request.NomeUsuario);
        if (jaExiste)
        {
            return Conflict($"Já existe um usuário com o nome '{request.NomeUsuario}'.");
        }

        var usuario = new Usuario
        {
            NomeUsuario = request.NomeUsuario,
            NomeCompleto = request.NomeCompleto,
            Papel = (PapelUsuarioDominio)request.Papel!.Value
        };

        var hasher = new PasswordHasher<Usuario>();
        usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);

        _contextoBancoDados.Usuarios.Add(usuario);
        await _contextoBancoDados.SaveChangesAsync();

        var dto = new UsuarioDto(usuario.Id, usuario.NomeUsuario, usuario.NomeCompleto, (PapelUsuario)usuario.Papel, usuario.Ativo);
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

        var eraAdminAtivo = usuario.Papel == PapelUsuarioDominio.Admin && usuario.Ativo;
        var continuaAdminAtivo = (request.Papel ?? (PapelUsuario)usuario.Papel) == PapelUsuario.Admin
            && (request.Ativo ?? usuario.Ativo);

        if (eraAdminAtivo && !continuaAdminAtivo && !await ExisteOutroAdminAtivoAsync(id))
        {
            return Conflict("Não é possível remover o último administrador ativo do sistema.");
        }

        if (request.NomeUsuario is not null)
        {
            var jaExiste = await _contextoBancoDados.Usuarios.AnyAsync(u => u.Id != id && u.NomeUsuario == request.NomeUsuario);
            if (jaExiste)
            {
                return Conflict($"Já existe um usuário com o nome '{request.NomeUsuario}'.");
            }

            usuario.NomeUsuario = request.NomeUsuario;
        }

        if (request.NomeCompleto is not null)
        {
            usuario.NomeCompleto = request.NomeCompleto;
        }

        if (request.Senha is not null)
        {
            var hasher = new PasswordHasher<Usuario>();
            usuario.SenhaHash = hasher.HashPassword(usuario, request.Senha);
        }

        if (request.Papel is not null)
        {
            usuario.Papel = (PapelUsuarioDominio)request.Papel.Value;
        }

        if (request.Ativo is not null)
        {
            usuario.Ativo = request.Ativo.Value;
        }

        await _contextoBancoDados.SaveChangesAsync();

        return Ok(new UsuarioDto(usuario.Id, usuario.NomeUsuario, usuario.NomeCompleto, (PapelUsuario)usuario.Papel, usuario.Ativo));
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
            return Conflict("Não é possível remover o último administrador ativo do sistema.");
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

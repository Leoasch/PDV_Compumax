using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Domain;
using Pos.Infrastructure.Dados;
using Pos.Shared;

namespace Pos.Api.Controllers;

[ApiController]
[Route("produtos")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly ContextoBancoDados _contextoBancoDados;

    public ProdutosController(ContextoBancoDados contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    [HttpGet("buscar")]
    public async Task<IEnumerable<ProdutoDto>> Buscar([FromQuery] string? termo)
    {
        var consulta = _contextoBancoDados.Produtos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var padrao = $"%{termo}%";
            consulta = consulta.Where(p =>
                EF.Functions.Like(p.Codigo, padrao) || EF.Functions.Like(p.Descricao, padrao));
        }

        return await consulta
            .OrderBy(p => p.Descricao)
            .Select(p => new ProdutoDto(p.Id, p.Codigo, p.Descricao, p.PrecoCusto, p.PrecoVenda, p.QuantidadeEstoque))
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProdutoDto>> Criar([FromBody] ProdutoRequest request)
    {
        var produto = new Produto
        {
            Codigo = request.Codigo,
            Descricao = request.Descricao,
            PrecoCusto = request.PrecoCusto,
            PrecoVenda = request.PrecoVenda,
            QuantidadeEstoque = request.QuantidadeEstoque
        };

        _contextoBancoDados.Produtos.Add(produto);
        await _contextoBancoDados.SaveChangesAsync();

        var dto = new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.PrecoCusto, produto.PrecoVenda, produto.QuantidadeEstoque);
        return CreatedAtAction(nameof(Buscar), new { termo = produto.Codigo }, dto);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProdutoDto>> Editar(int id, [FromBody] ProdutoAtualizacaoRequest request)
    {
        var produto = await _contextoBancoDados.Produtos.FindAsync(id);
        if (produto is null)
        {
            return NotFound();
        }

        if (request.Codigo is not null)
        {
            produto.Codigo = request.Codigo;
        }

        if (request.Descricao is not null)
        {
            produto.Descricao = request.Descricao;
        }

        if (request.PrecoCusto is not null)
        {
            produto.PrecoCusto = request.PrecoCusto.Value;
        }

        if (request.PrecoVenda is not null)
        {
            produto.PrecoVenda = request.PrecoVenda.Value;
        }

        if (request.QuantidadeEstoque is not null)
        {
            produto.QuantidadeEstoque = request.QuantidadeEstoque.Value;
        }

        await _contextoBancoDados.SaveChangesAsync();

        return Ok(new ProdutoDto(produto.Id, produto.Codigo, produto.Descricao, produto.PrecoCusto, produto.PrecoVenda, produto.QuantidadeEstoque));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Remover(int id)
    {
        var produto = await _contextoBancoDados.Produtos.FindAsync(id);
        if (produto is null)
        {
            return NotFound();
        }

        _contextoBancoDados.Produtos.Remove(produto);
        await _contextoBancoDados.SaveChangesAsync();

        return NoContent();
    }
}

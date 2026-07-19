using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pos.Api.Servicos;
using Pos.Domain;
using Pos.Infrastructure.Dados;
using Pos.Shared;

namespace Pos.Api.Controllers;

[ApiController]
[Route("vendas")]
[Authorize]
public class VendasController : ControllerBase
{
    private readonly ContextoBancoDados _contextoBancoDados;

    public VendasController(ContextoBancoDados contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    [HttpGet]
    public async Task<IEnumerable<VendaResumoDto>> Listar()
    {
        var consulta = _contextoBancoDados.Vendas.AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            var idUsuarioAutenticado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            consulta = consulta.Where(v => v.UsuarioId == idUsuarioAutenticado);
        }

        var vendas = await consulta
            .OrderByDescending(v => v.DataHora)
            .ToListAsync();

        return vendas
            .Select(v => new VendaResumoDto(v.Id, v.UsuarioNomeCompleto, v.DataHora, v.Total, v.FormaPagamento.ParaCompartilhado(), v.Cancelada))
            .ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VendaDto>> ObterPorId(int id)
    {
        var venda = await _contextoBancoDados.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda is null)
        {
            return NotFound();
        }

        if (!PodeAcessarVenda(venda))
        {
            return Forbid();
        }

        return Ok(MapearParaDto(venda));
    }

    [HttpPost]
    public async Task<ActionResult<VendaDto>> Criar([FromBody] VendaRequest request)
    {
        var produtoIds = request.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtos = await _contextoBancoDados.Produtos
            .Where(p => produtoIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var idsNaoEncontrados = produtoIds.Except(produtos.Keys).ToList();
        if (idsNaoEncontrados.Count > 0)
        {
            return Problem(detail: $"Produto(s) não encontrado(s): {string.Join(", ", idsNaoEncontrados)}.", statusCode: StatusCodes.Status400BadRequest);
        }

        var idUsuarioAutenticado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var usuario = await _contextoBancoDados.Usuarios.FindAsync(idUsuarioAutenticado);

        var venda = new Venda
        {
            UsuarioId = usuario!.Id,
            UsuarioNomeCompleto = usuario.NomeCompleto,
            DataHora = DateTime.UtcNow,
            FormaPagamento = request.FormaPagamento!.Value.ParaDominio()
        };

        await using var transacao = await _contextoBancoDados.Database.BeginTransactionAsync();

        foreach (var itemRequest in request.Itens)
        {
            var produto = produtos[itemRequest.ProdutoId];

            venda.Itens.Add(new ItemVenda
            {
                ProdutoId = produto.Id,
                ProdutoCodigo = produto.Codigo,
                ProdutoDescricao = produto.Descricao,
                Quantidade = itemRequest.Quantidade,
                PrecoUnitario = produto.PrecoVenda,
                PrecoDesconto = itemRequest.PrecoDesconto
            });

            await _contextoBancoDados.Produtos
                .Where(p => p.Id == produto.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    p => p.QuantidadeEstoque,
                    p => p.QuantidadeEstoque - itemRequest.Quantidade));
        }

        venda.Total = venda.Itens.Sum(i => i.Subtotal);

        _contextoBancoDados.Vendas.Add(venda);
        await _contextoBancoDados.SaveChangesAsync();

        await transacao.CommitAsync();

        return CreatedAtAction(nameof(ObterPorId), new { id = venda.Id }, MapearParaDto(venda));
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<VendaDto>> Cancelar(int id)
    {
        var venda = await _contextoBancoDados.Vendas
            .Include(v => v.Itens)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venda is null)
        {
            return NotFound();
        }

        if (!PodeAcessarVenda(venda))
        {
            return Forbid();
        }

        if (venda.Cancelada)
        {
            return Problem(detail: "Esta venda já foi cancelada.", statusCode: StatusCodes.Status409Conflict);
        }

        await using var transacao = await _contextoBancoDados.Database.BeginTransactionAsync();

        foreach (var item in venda.Itens.Where(i => i.ProdutoId is not null))
        {
            await _contextoBancoDados.Produtos
                .Where(p => p.Id == item.ProdutoId)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    p => p.QuantidadeEstoque,
                    p => p.QuantidadeEstoque + item.Quantidade));
        }

        venda.Cancelada = true;
        venda.DataCancelamento = DateTime.UtcNow;

        await _contextoBancoDados.SaveChangesAsync();

        await transacao.CommitAsync();

        return Ok(MapearParaDto(venda));
    }

    private bool PodeAcessarVenda(Venda venda)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var idUsuarioAutenticado = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return venda.UsuarioId == idUsuarioAutenticado;
    }

    private static VendaDto MapearParaDto(Venda venda)
    {
        return new VendaDto(
            venda.Id,
            venda.UsuarioId,
            venda.UsuarioNomeCompleto,
            venda.DataHora,
            venda.Total,
            venda.FormaPagamento.ParaCompartilhado(),
            venda.Cancelada,
            venda.DataCancelamento,
            venda.Itens
                .Select(i => new ItemVendaDto(i.ProdutoId, i.ProdutoCodigo, i.ProdutoDescricao, i.Quantidade, i.PrecoUnitario, i.Subtotal, i.PrecoDesconto))
                .ToList());
    }
}

using System.Text.Json;
using Microsoft.JSInterop;
using Pos.Shared;

namespace Pos.Client.Servicos;

/// <summary>
/// Guarda a venda em andamento no localStorage, para não perder o carrinho
/// se a página for recarregada ou fechada por acidente no meio de uma venda.
/// </summary>
public class ArmazenamentoCarrinho(IJSRuntime jsRuntime)
{
    private const string Chave = "pos_carrinho_atual";

    public async Task<List<ItemVendaDto>> ObterAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", Chave);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<ItemVendaDto>>(json, OpcoesJson.Padrao) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task SalvarAsync(List<ItemVendaDto> itens)
    {
        var json = JsonSerializer.Serialize(itens, OpcoesJson.Padrao);
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", Chave, json);
    }

    public async Task LimparAsync()
        => await jsRuntime.InvokeVoidAsync("localStorage.removeItem", Chave);
}

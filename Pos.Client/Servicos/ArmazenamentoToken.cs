using Microsoft.JSInterop;

namespace Pos.Client.Servicos;

/// <summary>
/// Guarda o token JWT no localStorage do navegador.
/// </summary>
public class ArmazenamentoToken(IJSRuntime jsRuntime)
{
    private const string Chave = "pos_token";

    public async Task<string?> ObterAsync()
        => await jsRuntime.InvokeAsync<string?>("localStorage.getItem", Chave);

    public async Task DefinirAsync(string token)
        => await jsRuntime.InvokeVoidAsync("localStorage.setItem", Chave, token);

    public async Task RemoverAsync()
        => await jsRuntime.InvokeVoidAsync("localStorage.removeItem", Chave);
}

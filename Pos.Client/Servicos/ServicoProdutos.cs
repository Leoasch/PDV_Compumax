using System.Net.Http.Json;
using Pos.Shared;

namespace Pos.Client.Servicos;

/// <summary>
/// Exemplo de serviço de API: encapsula as chamadas HTTP relacionadas a produtos,
/// para os componentes .razor não chamarem HttpClient diretamente.
/// </summary>
public class ServicoProdutos(HttpClient httpClient)
{
    public async Task<List<ProdutoDto>> BuscarAsync(string? termo = null)
    {
        var url = string.IsNullOrWhiteSpace(termo)
            ? "produtos/buscar"
            : $"produtos/buscar?termo={Uri.EscapeDataString(termo)}";

        var produtos = await httpClient.GetFromJsonAsync<List<ProdutoDto>>(url);
        return produtos ?? [];
    }
}

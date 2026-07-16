using System.Net.Http.Json;
using Pos.Shared;

namespace Pos.Client.Servicos;


public class ServicoProdutos(HttpClient httpClient)
{
    public async Task<List<ProdutoDto>> BuscarAsync(string? termo = null)
    {
        var url = string.IsNullOrWhiteSpace(termo)
            ? "produtos/buscar"
            : $"produtos/buscar?termo={Uri.EscapeDataString(termo)}";

        var produtos = await httpClient.GetFromJsonAsync<List<ProdutoDto>>(url, OpcoesJson.Padrao);
        return produtos ?? [];
    }

    public async Task<ProdutoDto?> ObterPorIdAsync(int id)
    {
        var resposta = await httpClient.GetAsync($"produtos/{id}");
        if (!resposta.IsSuccessStatusCode)
        {
            return null;
        }

        return await resposta.Content.ReadFromJsonAsync<ProdutoDto>(OpcoesJson.Padrao);
    }

    public async Task<(bool Sucesso, string? Erro)> CriarAsync(ProdutoRequest request)
    {
        var resposta = await httpClient.PostAsJsonAsync("produtos", request, OpcoesJson.Padrao);
        return resposta.IsSuccessStatusCode ? (true, null) : (false, await LerErroAsync(resposta));
    }

    public async Task<(bool Sucesso, string? Erro)> EditarAsync(int id, ProdutoAtualizacaoRequest request)
    {
        var resposta = await httpClient.PatchAsJsonAsync($"produtos/{id}", request, OpcoesJson.Padrao);
        return resposta.IsSuccessStatusCode ? (true, null) : (false, await LerErroAsync(resposta));
    }

    public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
    {
        var resposta = await httpClient.DeleteAsync($"produtos/{id}");
        return resposta.IsSuccessStatusCode ? (true, null) : (false, await LerErroAsync(resposta));
    }

    private static async Task<string?> LerErroAsync(HttpResponseMessage resposta)
    {
        try
        {
            var problema = await resposta.Content.ReadFromJsonAsync<ProblemaHttp>(OpcoesJson.Padrao);
            return problema?.Detail ?? problema?.Title;
        }
        catch
        {
            return null;
        }
    }

    private record ProblemaHttp(string? Title, string? Detail);
}

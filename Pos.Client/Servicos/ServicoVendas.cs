using System.Net.Http.Json;
using Pos.Shared;

namespace Pos.Client.Servicos;

public class ServicoVendas(HttpClient httpClient)
{
    public async Task<List<VendaResumoDto>> ListarAsync()
    {
        var vendas = await httpClient.GetFromJsonAsync<List<VendaResumoDto>>("vendas", OpcoesJson.Padrao);
        return vendas ?? [];
    }

    public async Task<VendaDto?> ObterPorIdAsync(int id)
    {
        var resposta = await httpClient.GetAsync($"vendas/{id}");
        if (!resposta.IsSuccessStatusCode)
        {
            return null;
        }

        return await resposta.Content.ReadFromJsonAsync<VendaDto>(OpcoesJson.Padrao);
    }

    public async Task<(bool Sucesso, VendaDto? Venda, string? Erro)> CriarAsync(VendaRequest request)
    {
        var resposta = await httpClient.PostAsJsonAsync("vendas", request, OpcoesJson.Padrao);
        if (!resposta.IsSuccessStatusCode)
        {
            return (false, null, await LerErroAsync(resposta));
        }

        var venda = await resposta.Content.ReadFromJsonAsync<VendaDto>(OpcoesJson.Padrao);
        return (true, venda, null);
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

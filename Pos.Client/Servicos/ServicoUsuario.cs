using System.Net.Http.Json;
using Pos.Shared;

namespace Pos.Client.Servicos;

public class ServicoUsuario(HttpClient httpClient)
{
    public async Task<List<UsuarioDto>> GetUsuarios(string? termo = null)
    {
        var usuarios = await httpClient.GetFromJsonAsync<List<UsuarioDto>>("usuarios", OpcoesJson.Padrao);
        return usuarios ?? [];
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(int id)
    {
        var resposta = await httpClient.GetAsync($"usuarios/{id}");
        if (!resposta.IsSuccessStatusCode)
        {
            return null;
        }

        return await resposta.Content.ReadFromJsonAsync<UsuarioDto>(OpcoesJson.Padrao);
    }

    public async Task<(bool Sucesso, string? Erro)> CriarAsync(UsuarioRequest request)
    {
        var resposta = await httpClient.PostAsJsonAsync("usuarios", request, OpcoesJson.Padrao);
        return resposta.IsSuccessStatusCode ? (true, null) : (false, await LerErroAsync(resposta));
    }

    public async Task<(bool Sucesso, string? Erro)> EditarAsync(int id, UsuarioAtualizacaoRequest request)
    {
        var resposta = await httpClient.PatchAsJsonAsync($"usuarios/{id}", request, OpcoesJson.Padrao);
        return resposta.IsSuccessStatusCode ? (true, null) : (false, await LerErroAsync(resposta));
    }

    public async Task<(bool Sucesso, string? Erro)> RemoverAsync(int id)
    {
        var resposta = await httpClient.DeleteAsync($"usuarios/{id}");
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

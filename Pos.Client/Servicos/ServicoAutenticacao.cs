using System.Net.Http.Json;
using Pos.Shared;

namespace Pos.Client.Servicos;

public class ServicoAutenticacao(
    HttpClient httpClient,
    ArmazenamentoToken armazenamentoToken,
    ProviderEstadoAutenticacao providerEstadoAutenticacao)
{
    public async Task<bool> EntrarAsync(string nomeUsuario, string senha)
    {
        var resposta = await httpClient.PostAsJsonAsync("autenticacao/entrar", new LoginRequest(nomeUsuario, senha));

        if (!resposta.IsSuccessStatusCode)
        {
            return false;
        }

        var login = await resposta.Content.ReadFromJsonAsync<LoginResponse>(OpcoesJson.Padrao);
        await armazenamentoToken.DefinirAsync(login!.Token);

        providerEstadoAutenticacao.NotificarMudancaEstado();

        return true;
    }

    public async Task SairAsync()
    {
        await armazenamentoToken.RemoverAsync();
        providerEstadoAutenticacao.NotificarMudancaEstado();
    }
}

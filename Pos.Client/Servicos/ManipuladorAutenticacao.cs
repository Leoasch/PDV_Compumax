using System.Net.Http.Headers;

namespace Pos.Client.Servicos;

/// <summary>
/// Anexa o token JWT (se houver) em toda requisição feita pelo HttpClient da aplicação.
/// </summary>
public class ManipuladorAutenticacao(ArmazenamentoToken armazenamentoToken) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await armazenamentoToken.ObterAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}

using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Pos.Client.Servicos;

/// <summary>
/// Informa ao Blazor (AuthorizeView, [Authorize] em rotas, etc.) se há um usuário logado,
/// a partir do token JWT guardado no localStorage.
/// </summary>
public class ProviderEstadoAutenticacao(ArmazenamentoToken armazenamentoToken) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonimo = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await armazenamentoToken.ObterAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(Anonimo);
        }

        var claims = LerClaimsDoToken(token).ToList();
        var expiracao = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiracao is not null && long.TryParse(expiracao, out var expUnix)
            && DateTimeOffset.FromUnixTimeSeconds(expUnix) <= DateTimeOffset.UtcNow)
        {
            await armazenamentoToken.RemoverAsync();
            return new AuthenticationState(Anonimo);
        }

        var identidade = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identidade));
    }

    public void NotificarMudancaEstado()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> LerClaimsDoToken(string token)
    {
        var payload = token.Split('.')[1];
        var json = Base64UrlDecode(payload);
        var pares = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
            ?? [];

        return pares.Select(par => new Claim(par.Key, par.Value.ToString()));
    }

    private static string Base64UrlDecode(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - base64.Length % 4) % 4), '=');

        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }
}

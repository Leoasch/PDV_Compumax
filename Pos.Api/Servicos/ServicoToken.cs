using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pos.Domain;

namespace Pos.Api.Servicos;

public sealed class ServicoToken(IConfiguration configuracao)
{
    public (string Token, DateTime ExpiraEm) GerarToken(Usuario usuario)
    {
        var chaveSecreta = configuracao["Jwt:ChaveSecreta"]
            ?? throw new InvalidOperationException("A chave 'Jwt:ChaveSecreta' não foi configurada.");
        var emissor = configuracao["Jwt:Emissor"];
        var audiencia = configuracao["Jwt:Audiencia"];
        var expiracaoMinutos = configuracao.GetValue<int>("Jwt:ExpiracaoMinutos");

        var expiraEm = DateTime.UtcNow.AddMinutes(expiracaoMinutos);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, usuario.Papel.ToString())
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}

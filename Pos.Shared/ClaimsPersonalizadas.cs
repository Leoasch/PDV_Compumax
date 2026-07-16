namespace Pos.Shared;

/// <summary>
/// Tipos de claim customizadas incluídas no JWT, usadas tanto pela Api (para gerar o token)
/// quanto pelo Client (para ler as informações do usuário logado).
/// </summary>
public static class ClaimsPersonalizadas
{
    public const string NomeCompleto = "nome_completo";
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pos.Client.Servicos;

/// <summary>
/// Opções de JSON usadas em toda chamada à Api. Parte de JsonSerializerDefaults.Web —
/// o mesmo padrão que GetFromJsonAsync/ReadFromJsonAsync já usam quando nenhuma opção é
/// passada (PropertyNameCaseInsensitive=true, necessário porque a Api manda "token" em
/// camelCase e os records aqui usam PascalCase) — e adiciona o JsonStringEnumConverter,
/// que bate com o que o Program.cs da Api configura globalmente. Enums como PapelUsuario/
/// FormaPagamento chegam como string ("Admin"), não como número.
/// </summary>
public static class OpcoesJson
{
    public static readonly JsonSerializerOptions Padrao = CriarOpcoes();

    private static JsonSerializerOptions CriarOpcoes()
    {
        var opcoes = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opcoes.Converters.Add(new JsonStringEnumConverter());
        return opcoes;
    }
}

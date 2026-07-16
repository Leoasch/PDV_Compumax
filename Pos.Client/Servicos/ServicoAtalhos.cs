using Microsoft.JSInterop;

namespace Pos.Client.Servicos;

/// <summary>
/// Registro central de atalhos de teclado. Componentes se registram para uma "ação"
/// (não para uma tecla diretamente) enquanto estão ativos, e se desregistram ao serem
/// destruídos (via o IDisposable retornado). Isso permite que o mesmo atalho signifique
/// coisas diferentes dependendo da tela atual, e prepara terreno para tornar as teclas
/// configuráveis no futuro sem mexer nos componentes que usam os atalhos.
///
/// A captura é feita via JS no document (não via @onkeydown do Blazor): um handler do
/// Blazor só recebe a tecla se o foco estiver dentro da árvore de DOM do componente, e
/// num PDV pensado pra ser operado via teclado isso não pode depender de o usuário já
/// ter clicado em algo primeiro.
///
/// O JS também recebe, sempre que um registro muda, a lista de teclas atualmente com
/// algum callback ativo — só essas têm o comportamento padrão do navegador bloqueado
/// (preventDefault), como o F5 recarregando a página ou o F1 abrindo a ajuda do Chrome.
/// Isso precisa acontecer de forma síncrona dentro do handler JS, então o C# não pode
/// decidir isso a tempo sozinho: por isso o espelho do lado do JS.
/// </summary>
public class ServicoAtalhos
{
    private readonly Dictionary<string, string> _teclasConfiguradas = new();
    private readonly Dictionary<string, List<RegistroInterno>> _registros = new();
    private bool _capturaIniciada;
    private IJSRuntime? _jsRuntime;

    /// <summary>Define (ou redefine) qual combinação de tecla corresponde a uma ação.</summary>
    public void DefinirTecla(string acaoId, string tecla) => _teclasConfiguradas[acaoId] = tecla;

    /// <summary>Tecla atualmente configurada para a ação, ou null se nenhuma foi definida.</summary>
    public string? ObterTecla(string acaoId) => _teclasConfiguradas.GetValueOrDefault(acaoId);

    /// <summary>
    /// Registra um callback para a ação enquanto o componente chamador estiver vivo.
    /// Descarte o IDisposable retornado em Dispose()/DisposeAsync() do componente.
    /// Se duas ações forem registradas para a mesma tecla, a mais recente tem prioridade
    /// (e a anterior volta a valer quando a mais recente for descartada).
    /// </summary>
    public IDisposable Registrar(string acaoId, Func<Task> callback)
    {
        var tecla = ObterTecla(acaoId)
            ?? throw new InvalidOperationException($"Nenhuma tecla configurada para a ação '{acaoId}'.");

        var registro = new RegistroInterno(acaoId, callback);

        if (!_registros.TryGetValue(tecla, out var lista))
        {
            lista = [];
            _registros[tecla] = lista;
        }

        lista.Add(registro);
        _ = AtualizarTeclasReservadasAsync();

        return new AssinaturaAtalho(() =>
        {
            lista.Remove(registro);
            _ = AtualizarTeclasReservadasAsync();
        });
    }

    /// <summary>
    /// Liga a captura global de teclado via JS. Chame uma vez, a partir do OnAfterRenderAsync
    /// (firstRender) do layout raiz da aplicação.
    /// </summary>
    public async Task IniciarCapturaGlobalAsync(IJSRuntime jsRuntime)
    {
        if (_capturaIniciada)
        {
            return;
        }

        _capturaIniciada = true;
        _jsRuntime = jsRuntime;

        var referencia = DotNetObjectReference.Create(this);
        await jsRuntime.InvokeVoidAsync("posAtalhos.iniciar", referencia);
        await AtualizarTeclasReservadasAsync();
    }

    [JSInvokable]
    public async Task TratarTeclaJsAsync(string tecla, bool ctrl, bool shift, bool alt)
    {
        var chave = NormalizarTecla(tecla, ctrl, shift, alt);

        if (_registros.TryGetValue(chave, out var lista) && lista.Count > 0)
        {
            await lista[^1].Callback();
        }
    }

    private async Task AtualizarTeclasReservadasAsync()
    {
        if (_jsRuntime is null)
        {
            return;
        }

        var teclasAtivas = _registros
            .Where(par => par.Value.Count > 0)
            .Select(par => par.Key)
            .ToArray();

        await _jsRuntime.InvokeVoidAsync("posAtalhos.definirTeclasReservadas", teclasAtivas);
    }

    private static string NormalizarTecla(string tecla, bool ctrl, bool shift, bool alt)
    {
        var partes = new List<string>();
        if (ctrl) partes.Add("Ctrl");
        if (shift) partes.Add("Shift");
        if (alt) partes.Add("Alt");
        partes.Add(tecla);

        return string.Join("+", partes);
    }

    private sealed class RegistroInterno(string acaoId, Func<Task> callback)
    {
        public string AcaoId { get; } = acaoId;
        public Func<Task> Callback { get; } = callback;
    }

    private sealed class AssinaturaAtalho(Action aoDescartar) : IDisposable
    {
        public void Dispose() => aoDescartar();
    }
}

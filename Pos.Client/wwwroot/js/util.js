window.posUtil = {
    // Agenda uma chamada .NET fora do ciclo de renderização atual do Blazor.
    // Necessário para contornar um bug do MudSelect: fechar o popover a partir
    // do mesmo despacho de evento que originou o Enter (mesmo com Task.Delay)
    // não sincroniza a classe CSS do popover quando o select está dentro de um
    // MudDialog — só funciona vindo de um setTimeout JS genuíno.
    agendarChamada: function (dotNetRef, metodo, delayMs) {
        setTimeout(function () {
            dotNetRef.invokeMethodAsync(metodo);
        }, delayMs);
    }
};

window.posAtalhos = {
    teclasReservadas: new Set(),

    iniciar: function (dotNetRef) {
        document.addEventListener('keydown', function (e) {
            var chave = montarChave(e);

            if (window.posAtalhos.teclasReservadas.has(chave)) {
                e.preventDefault();
            }

            dotNetRef.invokeMethodAsync('TratarTeclaJsAsync', e.key, e.ctrlKey, e.shiftKey, e.altKey);
        });
    },

    definirTeclasReservadas: function (teclas) {
        window.posAtalhos.teclasReservadas = new Set(teclas);
    },

    mostrarTeclasReservadas: function () {
        console.table([...window.posAtalhos.teclasReservadas]);
    }
};

function montarChave(e) {
    var partes = [];
    if (e.ctrlKey) partes.push('Ctrl');
    if (e.shiftKey) partes.push('Shift');
    if (e.altKey) partes.push('Alt');
    partes.push(e.key);

    return partes.join('+');
}

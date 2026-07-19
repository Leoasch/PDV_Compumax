window.posAtalhos = {
    teclasReservadas: new Set(),

    iniciar: function (dotNetRef) {
        document.addEventListener('keydown', function (e) {
            if ((ehCaractereDigitavel(e) || ehTeclaDeEdicaoDeTexto(e)) && elementoAtivoEhEditavel()) {
                return;
            }

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

function ehCaractereDigitavel(e) {
    return e.key.length === 1 && !e.ctrlKey && !e.altKey;
}

// ArrowLeft/ArrowRight movem o cursor dentro de um campo de texto (e Shift/Ctrl+seta
// selecionam/pulam palavras) — isso não pode ser sequestrado por atalhos globais como
// trocar de página enquanto o usuário está editando um campo.
function ehTeclaDeEdicaoDeTexto(e) {
    return (e.key === 'ArrowLeft' || e.key === 'ArrowRight') && !e.ctrlKey && !e.altKey;
}

var TIPOS_INPUT_NAO_TEXTUAIS = ['radio', 'checkbox', 'button', 'submit', 'reset', 'range', 'color', 'file', 'image'];

function elementoAtivoEhEditavel() {
    var ativo = document.activeElement;
    if (!ativo) return false;
    if (ativo.isContentEditable) return true;

    var tag = ativo.tagName;
    if (tag === 'TEXTAREA' || tag === 'SELECT') return true;
    if (tag !== 'INPUT') return false;

    // Radio/checkbox/switch (mesmo sendo <input>) não recebem caracteres digitados,
    // então não devem bloquear os atalhos — só tipos textuais (text, number etc.) bloqueiam.
    var tipo = (ativo.type || 'text').toLowerCase();
    return TIPOS_INPUT_NAO_TEXTUAIS.indexOf(tipo) === -1;
}

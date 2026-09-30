// Confirmação antes de enviar formulários de ações sensíveis (ex.: desativar usuário ou profissional).
// Uso: <form data-confirmacao="Deseja realmente ...?">. A proteção real é sempre feita no servidor.
(function () {
    'use strict';

    document.addEventListener('submit', function (evento) {
        const mensagem = evento.target.getAttribute('data-confirmacao');
        if (mensagem && !window.confirm(mensagem)) evento.preventDefault();
    });
})();

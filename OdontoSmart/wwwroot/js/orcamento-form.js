// Formulário de orçamento: adicionar/remover itens e prévia dos totais.
// Os valores exibidos aqui são apenas uma prévia; o servidor recalcula tudo ao salvar.
(function () {
    'use strict';

    const form = document.querySelector('[data-orcamento-form]');
    if (!form) return;

    const corpo = form.querySelector('[data-itens]');
    const modelo = document.getElementById('modelo-item-orcamento');
    const total = form.querySelector('[data-orcamento-total]');
    const vazio = form.querySelector('[data-itens-vazio]');
    const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    const linhas = () => Array.from(corpo.querySelectorAll('[data-item]'));

    function valor(input) {
        const numero = parseFloat(input ? input.value : '');
        return Number.isFinite(numero) ? numero : 0;
    }

    // Calcula em centavos para evitar erros de arredondamento de ponto flutuante na exibição.
    function totalDoItemEmCentavos(linha) {
        const quantidade = valor(linha.querySelector('[data-quantidade]'));
        const valorUnitario = valor(linha.querySelector('[data-valor-unitario]'));
        if (quantidade <= 0 || valorUnitario < 0) return 0;
        return Math.round(quantidade * valorUnitario * 100 + Number.EPSILON);
    }

    function recalcular() {
        let totalEmCentavos = 0;
        for (const linha of linhas()) {
            const centavos = totalDoItemEmCentavos(linha);
            totalEmCentavos += centavos;
            linha.querySelector('[data-item-total]').textContent = moeda.format(centavos / 100);
        }
        total.textContent = moeda.format(totalEmCentavos / 100);
        vazio.hidden = linhas().length > 0;
    }

    // Mantém os índices contíguos (Itens[0], Itens[1], ...) exigidos pelo model binding.
    function reindexar() {
        linhas().forEach(function (linha, indice) {
            linha.querySelectorAll('[name], [id], [for], [data-valmsg-for]').forEach(function (elemento) {
                for (const atributo of ['name', 'id', 'for', 'data-valmsg-for']) {
                    const atual = elemento.getAttribute(atributo);
                    if (!atual) continue;
                    elemento.setAttribute(atributo, atual
                        .replace(/Itens\[\d+\]/g, 'Itens[' + indice + ']')
                        .replace(/Itens_\d+__/g, 'Itens_' + indice + '__'));
                }
            });
        });
    }

    // Registra as regras de validação das linhas adicionadas dinamicamente.
    function atualizarValidacao() {
        if (!window.jQuery || !window.jQuery.validator || !window.jQuery.validator.unobtrusive) return;
        const $form = window.jQuery(form);
        $form.removeData('validator').removeData('unobtrusiveValidation');
        window.jQuery.validator.unobtrusive.parse($form);
    }

    function adicionarItem() {
        const html = modelo.innerHTML.replaceAll('__index__', String(linhas().length));
        corpo.insertAdjacentHTML('beforeend', html);
        reindexar();
        atualizarValidacao();
        recalcular();

        const nova = linhas().at(-1);
        const descricao = nova && nova.querySelector('input');
        if (descricao) descricao.focus();
    }

    form.querySelector('[data-adicionar-item]').addEventListener('click', adicionarItem);

    corpo.addEventListener('click', function (evento) {
        const botao = evento.target.closest('[data-remover-item]');
        if (!botao) return;
        botao.closest('[data-item]').remove();
        reindexar();
        atualizarValidacao();
        recalcular();
    });

    corpo.addEventListener('input', function (evento) {
        if (evento.target.matches('[data-quantidade], [data-valor-unitario]')) recalcular();
    });

    recalcular();
})();

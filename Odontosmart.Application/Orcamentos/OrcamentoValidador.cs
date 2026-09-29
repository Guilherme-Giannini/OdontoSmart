using System.Globalization;
using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Orcamentos;

public static class OrcamentoValidador
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <param name="dataCadastro">Data (local) de cadastro do orçamento; a validade não pode ser anterior a ela.</param>
    public static List<ErroValidacao> Validar(OrcamentoDados dados, DateTime dataCadastro)
    {
        var erros = new List<ErroValidacao>();

        if (dados.PacienteId is null || dados.PacienteId == Guid.Empty)
            erros.Add(new(nameof(OrcamentoDados.PacienteId), "Selecione o paciente do orçamento."));

        if (dados.Validade is not null && dados.Validade.Value.Date < dataCadastro.Date)
            erros.Add(new(nameof(OrcamentoDados.Validade), "A validade não pode ser anterior à data de cadastro."));

        if (dados.Observacoes?.Trim().Length > Orcamento.ObservacoesTamanhoMaximo)
            erros.Add(new(nameof(OrcamentoDados.Observacoes),
                $"As observações devem possuir no máximo {Orcamento.ObservacoesTamanhoMaximo} caracteres."));

        var itens = dados.Itens ?? [];
        if (itens.Count == 0)
            erros.Add(new(nameof(OrcamentoDados.Itens), "Adicione pelo menos um item ao orçamento."));
        else if (itens.Count > Orcamento.QuantidadeMaximaItens)
            erros.Add(new(nameof(OrcamentoDados.Itens),
                $"O orçamento deve possuir no máximo {Orcamento.QuantidadeMaximaItens} itens."));

        for (var i = 0; i < itens.Count; i++)
            ValidarItem(itens[i], $"{nameof(OrcamentoDados.Itens)}[{i}]", erros);

        return erros;
    }

    private static void ValidarItem(OrcamentoItemDados item, string prefixo, List<ErroValidacao> erros)
    {
        var descricao = item.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length == 0)
            erros.Add(new($"{prefixo}.{nameof(OrcamentoItemDados.Descricao)}", "Informe a descrição do item."));
        else if (descricao.Length > OrcamentoItem.DescricaoTamanhoMaximo)
            erros.Add(new($"{prefixo}.{nameof(OrcamentoItemDados.Descricao)}",
                $"A descrição deve possuir no máximo {OrcamentoItem.DescricaoTamanhoMaximo} caracteres."));

        var campoQuantidade = $"{prefixo}.{nameof(OrcamentoItemDados.Quantidade)}";
        if (item.Quantidade is null)
            erros.Add(new(campoQuantidade, "Informe a quantidade."));
        else if (item.Quantidade <= 0)
            erros.Add(new(campoQuantidade, "A quantidade deve ser maior que zero."));
        else if (item.Quantidade > OrcamentoItem.QuantidadeMaxima)
            erros.Add(new(campoQuantidade, $"A quantidade deve ser no máximo {OrcamentoItem.QuantidadeMaxima.ToString("N0", PtBr)}."));
        else if (!OrcamentoItem.PossuiCasasDecimaisValidas(item.Quantidade.Value))
            erros.Add(new(campoQuantidade, "Informe a quantidade com no máximo duas casas decimais."));

        var campoValor = $"{prefixo}.{nameof(OrcamentoItemDados.ValorUnitario)}";
        if (item.ValorUnitario is null)
            erros.Add(new(campoValor, "Informe o valor unitário."));
        else if (item.ValorUnitario < 0)
            erros.Add(new(campoValor, "O valor unitário não pode ser negativo."));
        else if (item.ValorUnitario > OrcamentoItem.ValorUnitarioMaximo)
            erros.Add(new(campoValor, $"O valor unitário deve ser no máximo {OrcamentoItem.ValorUnitarioMaximo.ToString("C", PtBr)}."));
        else if (!OrcamentoItem.PossuiCasasDecimaisValidas(item.ValorUnitario.Value))
            erros.Add(new(campoValor, "Informe o valor unitário com no máximo duas casas decimais (centavos)."));

        if (item.Observacoes?.Trim().Length > OrcamentoItem.ObservacoesTamanhoMaximo)
            erros.Add(new($"{prefixo}.{nameof(OrcamentoItemDados.Observacoes)}",
                $"As observações do item devem possuir no máximo {OrcamentoItem.ObservacoesTamanhoMaximo} caracteres."));
    }
}

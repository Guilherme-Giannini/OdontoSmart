using OdontoSmart.Domain.Common;

namespace OdontoSmart.Domain.Entities;

public class OrcamentoItem
{
    public const int DescricaoTamanhoMaximo = 500;
    public const int ObservacoesTamanhoMaximo = 500;
    public const int CasasDecimais = 2;
    public const decimal QuantidadeMaxima = 10_000m;
    public const decimal ValorUnitarioMaximo = 1_000_000m;

    public Guid Id { get; private set; }
    public Guid OrcamentoId { get; private set; }

    /// <summary>Posição do item no orçamento, preservando a ordem informada pelo usuário.</summary>
    public int Ordem { get; private set; }

    public string Descricao { get; private set; } = string.Empty;
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? Observacoes { get; private set; }

    // Utilizado pelo EF Core.
    private OrcamentoItem() { }

    /// <summary>
    /// Itens são criados somente pelo <see cref="Orcamento"/>, garantindo que nunca existam sem orçamento.
    /// </summary>
    internal static OrcamentoItem Criar(Guid orcamentoId, int ordem, DadosItemOrcamento dados)
    {
        var descricao = dados.Descricao?.Trim() ?? string.Empty;
        if (descricao.Length is 0 or > DescricaoTamanhoMaximo)
            throw new DomainException($"A descrição do item deve possuir entre 1 e {DescricaoTamanhoMaximo} caracteres.");

        if (dados.Quantidade <= 0 || dados.Quantidade > QuantidadeMaxima || !PossuiCasasDecimaisValidas(dados.Quantidade))
            throw new DomainException("A quantidade do item deve ser maior que zero.");

        if (dados.ValorUnitario < 0 || dados.ValorUnitario > ValorUnitarioMaximo || !PossuiCasasDecimaisValidas(dados.ValorUnitario))
            throw new DomainException("O valor unitário do item não pode ser negativo.");

        var observacoes = string.IsNullOrWhiteSpace(dados.Observacoes) ? null : dados.Observacoes.Trim();
        if (observacoes?.Length > ObservacoesTamanhoMaximo)
            throw new DomainException($"As observações do item devem possuir no máximo {ObservacoesTamanhoMaximo} caracteres.");

        return new OrcamentoItem
        {
            Id = Guid.NewGuid(),
            OrcamentoId = orcamentoId,
            Ordem = ordem,
            Descricao = descricao,
            Quantidade = dados.Quantidade,
            ValorUnitario = dados.ValorUnitario,
            ValorTotal = CalcularValorTotal(dados.Quantidade, dados.ValorUnitario),
            Observacoes = observacoes
        };
    }

    /// <summary>ValorTotal = Quantidade × ValorUnitario, arredondado para centavos.</summary>
    public static decimal CalcularValorTotal(decimal quantidade, decimal valorUnitario) =>
        Math.Round(quantidade * valorUnitario, CasasDecimais, MidpointRounding.AwayFromZero);

    public static bool PossuiCasasDecimaisValidas(decimal valor) =>
        decimal.Round(valor, CasasDecimais) == valor;
}

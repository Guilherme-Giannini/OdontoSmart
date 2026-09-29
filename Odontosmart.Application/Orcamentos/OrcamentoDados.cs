namespace OdontoSmart.Application.Orcamentos;

/// <summary>
/// Dados editáveis de um orçamento, informados pelo usuário no cadastro ou na edição.
/// Valores totais não fazem parte da entrada: são sempre calculados pelo domínio.
/// </summary>
public sealed record OrcamentoDados(
    Guid? PacienteId,
    DateTime? Validade,
    string? Observacoes,
    IReadOnlyList<OrcamentoItemDados> Itens);

public sealed record OrcamentoItemDados(
    string? Descricao,
    decimal? Quantidade,
    decimal? ValorUnitario,
    string? Observacoes);

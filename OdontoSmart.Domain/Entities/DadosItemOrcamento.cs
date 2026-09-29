namespace OdontoSmart.Domain.Entities;

/// <summary>
/// Dados informados para um item do orçamento. O valor total do item é sempre calculado pelo domínio.
/// </summary>
public sealed record DadosItemOrcamento(
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    string? Observacoes);

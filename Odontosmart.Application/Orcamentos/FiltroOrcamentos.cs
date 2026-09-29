using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Orcamentos;

/// <param name="Busca">Nome ou CPF do paciente.</param>
/// <param name="DataInicio">Início do período de cadastro (data local, inclusiva).</param>
/// <param name="DataFim">Fim do período de cadastro (data local, inclusiva).</param>
public sealed record FiltroOrcamentos(
    string? Busca,
    StatusOrcamento? Status,
    DateTime? DataInicio,
    DateTime? DataFim);

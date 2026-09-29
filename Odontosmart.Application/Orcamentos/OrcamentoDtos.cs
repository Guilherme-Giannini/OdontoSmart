using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Orcamentos;

public sealed record OrcamentoResumoDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    string? PacienteCpf,
    DateTime DataCadastro,
    DateTime? Validade,
    StatusOrcamento Status,
    int QuantidadeItens,
    decimal ValorTotal);

public sealed record OrcamentoDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    string? PacienteCpf,
    string? PacienteTelefone,
    string? PacienteEmail,
    DateTime DataCadastro,
    DateTime? DataAtualizacao,
    DateTime? Validade,
    StatusOrcamento Status,
    string? Observacoes,
    decimal ValorTotal,
    IReadOnlyList<OrcamentoItemDto> Itens,
    IReadOnlyList<StatusOrcamento> TransicoesPermitidas);

public sealed record OrcamentoItemDto(
    Guid Id,
    string Descricao,
    decimal Quantidade,
    decimal ValorUnitario,
    decimal ValorTotal,
    string? Observacoes);

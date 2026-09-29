using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

/// <summary>
/// Dados de exibição de um orçamento (tela de detalhes e confirmação de exclusão).
/// </summary>
public class OrcamentoDetailsViewModel
{
    public Guid Id { get; init; }
    public Guid PacienteId { get; init; }
    public string PacienteNome { get; init; } = string.Empty;
    public string PacienteCpf { get; init; } = string.Empty;
    public string PacienteTelefone { get; init; } = string.Empty;
    public string PacienteEmail { get; init; } = string.Empty;
    public string DataCadastro { get; init; } = string.Empty;
    public string DataAtualizacao { get; init; } = string.Empty;
    public string Validade { get; init; } = string.Empty;
    public StatusOrcamento Status { get; init; }
    public string Observacoes { get; init; } = string.Empty;
    public string ValorTotal { get; init; } = string.Empty;
    public IReadOnlyList<OrcamentoItemDetailsViewModel> Itens { get; init; } = [];
    public OrcamentoStatusViewModel AlteracaoStatus { get; init; } = new();

    public static OrcamentoDetailsViewModel De(OrcamentoDto orcamento) => new()
    {
        Id = orcamento.Id,
        PacienteId = orcamento.PacienteId,
        PacienteNome = orcamento.PacienteNome,
        PacienteCpf = PacienteFormatacao.Cpf(orcamento.PacienteCpf),
        PacienteTelefone = PacienteFormatacao.Telefone(orcamento.PacienteTelefone),
        PacienteEmail = orcamento.PacienteEmail ?? string.Empty,
        DataCadastro = OrcamentoFormatacao.DataHora(orcamento.DataCadastro),
        DataAtualizacao = OrcamentoFormatacao.DataHora(orcamento.DataAtualizacao),
        Validade = OrcamentoFormatacao.Data(orcamento.Validade),
        Status = orcamento.Status,
        Observacoes = orcamento.Observacoes ?? string.Empty,
        ValorTotal = OrcamentoFormatacao.Moeda(orcamento.ValorTotal),
        Itens = orcamento.Itens.Select(OrcamentoItemDetailsViewModel.De).ToList(),
        AlteracaoStatus = new OrcamentoStatusViewModel
        {
            Id = orcamento.Id,
            StatusAtual = orcamento.Status,
            TransicoesPermitidas = orcamento.TransicoesPermitidas
        }
    };
}

public class OrcamentoItemDetailsViewModel
{
    public string Descricao { get; init; } = string.Empty;
    public string Observacoes { get; init; } = string.Empty;
    public string Quantidade { get; init; } = string.Empty;
    public string ValorUnitario { get; init; } = string.Empty;
    public string ValorTotal { get; init; } = string.Empty;

    public static OrcamentoItemDetailsViewModel De(OrcamentoItemDto item) => new()
    {
        Descricao = item.Descricao,
        Observacoes = item.Observacoes ?? string.Empty,
        Quantidade = OrcamentoFormatacao.Quantidade(item.Quantidade),
        ValorUnitario = OrcamentoFormatacao.Moeda(item.ValorUnitario),
        ValorTotal = OrcamentoFormatacao.Moeda(item.ValorTotal)
    };
}

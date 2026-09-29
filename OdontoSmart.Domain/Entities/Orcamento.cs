using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Domain.Entities;

public class Orcamento
{
    public const int ObservacoesTamanhoMaximo = 2000;
    public const int QuantidadeMaximaItens = 100;

    // RN015 — únicas transições de status permitidas.
    private static readonly Dictionary<StatusOrcamento, StatusOrcamento[]> Transicoes = new()
    {
        [StatusOrcamento.Aberto] =
            [StatusOrcamento.AguardandoDecisao, StatusOrcamento.Aprovado, StatusOrcamento.Recusado, StatusOrcamento.Cancelado],
        [StatusOrcamento.AguardandoDecisao] =
            [StatusOrcamento.Aprovado, StatusOrcamento.Recusado, StatusOrcamento.Cancelado],
        [StatusOrcamento.Aprovado] = [StatusOrcamento.Cancelado],
        [StatusOrcamento.Recusado] = [StatusOrcamento.Aberto],
        [StatusOrcamento.Expirado] = [StatusOrcamento.Aberto, StatusOrcamento.Cancelado],
        [StatusOrcamento.Cancelado] = []
    };

    private readonly List<OrcamentoItem> _itens = [];

    public Guid Id { get; private set; }
    public Guid PacienteId { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    public DateTime? Validade { get; private set; }
    public StatusOrcamento Status { get; private set; }
    public string? Observacoes { get; private set; }

    /// <summary>Soma dos valores totais dos itens. Nunca informado pelo usuário.</summary>
    public decimal ValorTotal { get; private set; }

    public IReadOnlyCollection<OrcamentoItem> Itens => _itens.AsReadOnly();

    // Utilizado pelo EF Core.
    private Orcamento() { }

    public static Orcamento Criar(
        Guid pacienteId,
        DateTime? validade,
        string? observacoes,
        IEnumerable<DadosItemOrcamento> itens)
    {
        var orcamento = new Orcamento
        {
            Id = Guid.NewGuid(),
            DataCadastro = DateTime.UtcNow,
            Status = StatusOrcamento.Aberto
        };

        orcamento.DefinirDados(pacienteId, validade, observacoes);
        orcamento.DefinirItens(itens);
        return orcamento;
    }

    /// <summary>
    /// Substitui paciente, validade, observações e itens, recalculando todos os valores.
    /// </summary>
    public void Atualizar(
        Guid pacienteId,
        DateTime? validade,
        string? observacoes,
        IEnumerable<DadosItemOrcamento> itens)
    {
        DefinirDados(pacienteId, validade, observacoes);
        DefinirItens(itens);
        DataAtualizacao = DateTime.UtcNow;
    }

    public static IReadOnlyList<StatusOrcamento> TransicoesPermitidas(StatusOrcamento de) => Transicoes[de];

    public bool PodeAlterarStatusPara(StatusOrcamento novoStatus) => Transicoes[Status].Contains(novoStatus);

    public bool EstaVencido(DateTime hoje) => Validade is not null && Validade.Value.Date < hoje.Date;

    public void AlterarStatus(StatusOrcamento novoStatus, DateTime hoje)
    {
        if (!PodeAlterarStatusPara(novoStatus))
            throw new DomainException($"Transição de status não permitida: {Status} → {novoStatus}.");

        // Reabrir um orçamento vencido faria com que ele expirasse novamente de imediato.
        if (novoStatus == StatusOrcamento.Aberto && EstaVencido(hoje))
            throw new DomainException("Atualize a validade do orçamento antes de reabri-lo.");

        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// RN016 — marca como Expirado o orçamento em aberto ou aguardando decisão cuja validade já passou.
    /// Orçamentos aprovados, recusados ou cancelados não são alterados.
    /// </summary>
    /// <returns>true quando o status foi alterado.</returns>
    public bool ExpirarSeVencido(DateTime hoje)
    {
        if (Status is not (StatusOrcamento.Aberto or StatusOrcamento.AguardandoDecisao) || !EstaVencido(hoje))
            return false;

        Status = StatusOrcamento.Expirado;
        DataAtualizacao = DateTime.UtcNow;
        return true;
    }

    private void DefinirDados(Guid pacienteId, DateTime? validade, string? observacoes)
    {
        if (pacienteId == Guid.Empty)
            throw new DomainException("O orçamento deve estar associado a um paciente.");

        if (validade is not null && validade.Value.Date < DataCadastro.ToLocalTime().Date)
            throw new DomainException("A validade não pode ser anterior à data de cadastro.");

        var observacoesNormalizadas = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        if (observacoesNormalizadas?.Length > ObservacoesTamanhoMaximo)
            throw new DomainException($"As observações devem possuir no máximo {ObservacoesTamanhoMaximo} caracteres.");

        PacienteId = pacienteId;
        Validade = validade?.Date;
        Observacoes = observacoesNormalizadas;
    }

    private void DefinirItens(IEnumerable<DadosItemOrcamento> itens)
    {
        var dados = itens?.ToList() ?? [];

        if (dados.Count == 0)
            throw new DomainException("O orçamento deve possuir pelo menos um item.");

        if (dados.Count > QuantidadeMaximaItens)
            throw new DomainException($"O orçamento deve possuir no máximo {QuantidadeMaximaItens} itens.");

        // Todos os itens são validados antes de alterar o estado atual.
        var novosItens = dados.Select((item, indice) => OrcamentoItem.Criar(Id, indice + 1, item)).ToList();

        _itens.Clear();
        _itens.AddRange(novosItens);
        ValorTotal = _itens.Sum(i => i.ValorTotal);
    }
}

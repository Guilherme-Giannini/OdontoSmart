using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Orcamentos;

public interface IOrcamentoRepository
{
    /// <summary>Obtém o orçamento com seus itens, pronto para alteração.</summary>
    Task<Orcamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pesquisa orçamentos por nome/CPF do paciente, status e período de cadastro,
    /// do mais recente para o mais antigo. A consulta é executada no banco.
    /// </summary>
    Task<PaginaResultado<OrcamentoResumoDto>> PesquisarAsync(
        FiltroOrcamentos filtro, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);

    /// <summary>Orçamentos em aberto ou aguardando decisão cuja validade é anterior a <paramref name="hoje"/>.</summary>
    Task<IReadOnlyList<Orcamento>> ListarVencidosAsync(DateTime hoje, CancellationToken cancellationToken = default);

    void Adicionar(Orcamento orcamento);

    void Remover(Orcamento orcamento);

    /// <exception cref="PacienteInexistenteException">Quando o paciente associado não existe mais.</exception>
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Orcamentos;

public interface IOrcamentoService
{
    /// <summary>
    /// Lista os orçamentos de forma paginada, aplicando os filtros informados.
    /// </summary>
    Task<PaginaResultado<OrcamentoResumoDto>> PesquisarAsync(
        FiltroOrcamentos filtro, int pagina, CancellationToken cancellationToken = default);

    Task<OrcamentoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Resultado<Guid>> CriarAsync(OrcamentoDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> AtualizarAsync(Guid id, OrcamentoDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> AlterarStatusAsync(Guid id, StatusOrcamento novoStatus, CancellationToken cancellationToken = default);

    Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

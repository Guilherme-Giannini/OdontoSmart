using OdontoSmart.Application.Common;

namespace OdontoSmart.Application.Pacientes;

public interface IPacienteService
{
    /// <summary>
    /// Lista os pacientes de forma paginada. Quando <paramref name="termo"/> é informado,
    /// filtra por nome completo, CPF ou telefone.
    /// </summary>
    Task<PaginaResultado<PacienteDto>> PesquisarAsync(string? termo, int pagina, CancellationToken cancellationToken = default);

    Task<PacienteDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PacienteOpcaoDto>> ListarOpcoesAsync(CancellationToken cancellationToken = default);

    Task<Resultado<Guid>> CriarAsync(PacienteDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> AtualizarAsync(Guid id, PacienteDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

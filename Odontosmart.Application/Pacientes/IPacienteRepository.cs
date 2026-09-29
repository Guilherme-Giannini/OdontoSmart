using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Pacientes;

public interface IPacienteRepository
{
    Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pesquisa pacientes por nome, CPF ou telefone, ordenados por nome. A consulta é executada no banco.
    /// </summary>
    Task<PaginaResultado<Paciente>> PesquisarAsync(
        string? termo, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);

    Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarPacienteId, CancellationToken cancellationToken = default);

    Task<bool> PossuiOrcamentosAsync(Guid pacienteId, CancellationToken cancellationToken = default);

    /// <summary>Todos os pacientes (id, nome e CPF), ordenados por nome, para seleção em formulários.</summary>
    Task<IReadOnlyList<PacienteOpcaoDto>> ListarOpcoesAsync(CancellationToken cancellationToken = default);

    void Adicionar(Paciente paciente);

    void Remover(Paciente paciente);

    /// <exception cref="CpfDuplicadoException">Quando o CPF já pertence a outro paciente.</exception>
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

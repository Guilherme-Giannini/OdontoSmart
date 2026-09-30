using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Profissionais;

public interface IProfissionalRepository
{
    Task<Profissional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pesquisa profissionais por nome de exibição ou número do CRO, ordenados por nome.
    /// A consulta é executada no banco.
    /// </summary>
    Task<PaginaResultado<ProfissionalResumoDto>> PesquisarAsync(
        string? termo, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);

    Task<bool> ExisteCroAsync(
        string croUf, string cro, Guid? ignorarProfissionalId, CancellationToken cancellationToken = default);

    /// <summary>Indica se o usuário está vinculado a algum profissional (exceto o informado).</summary>
    Task<bool> PossuiVinculoAsync(
        Guid usuarioId, Guid? ignorarProfissionalId, CancellationToken cancellationToken = default);

    /// <summary>Ids dos usuários vinculados a algum profissional.</summary>
    Task<IReadOnlyCollection<Guid>> ListarUsuariosVinculadosAsync(CancellationToken cancellationToken = default);

    void Adicionar(Profissional profissional);

    /// <exception cref="CroDuplicadoException">Quando o CRO já pertence a outro profissional na mesma UF.</exception>
    /// <exception cref="UsuarioJaVinculadoException">Quando o usuário já está vinculado a outro profissional.</exception>
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}

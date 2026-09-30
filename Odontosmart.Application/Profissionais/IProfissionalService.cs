using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Application.Profissionais;

public interface IProfissionalService
{
    /// <summary>
    /// Lista os profissionais de forma paginada. Quando <paramref name="termo"/> é informado,
    /// filtra por nome de exibição ou número do CRO.
    /// </summary>
    Task<PaginaResultado<ProfissionalResumoDto>> PesquisarAsync(
        string? termo, int pagina, CancellationToken cancellationToken = default);

    Task<ProfissionalDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Usuários que podem ser vinculados ao profissional: ativos, de perfil Dentista e ainda não vinculados,
    /// além do usuário atualmente vinculado a <paramref name="profissionalId"/>.
    /// </summary>
    Task<IReadOnlyList<UsuarioOpcaoDto>> ListarUsuariosDisponiveisAsync(
        Guid? profissionalId, CancellationToken cancellationToken = default);

    Task<Resultado<Guid>> CriarAsync(ProfissionalDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> AtualizarAsync(Guid id, ProfissionalDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default);
}

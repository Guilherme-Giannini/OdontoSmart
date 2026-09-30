using OdontoSmart.Application.Common;

namespace OdontoSmart.Application.Usuarios;

public interface IUsuarioService
{
    /// <summary>
    /// Lista os usuários de forma paginada, filtrando por nome/e-mail e situação.
    /// </summary>
    Task<PaginaResultado<UsuarioResumoDto>> PesquisarAsync(
        FiltroUsuarios filtro, int pagina, CancellationToken cancellationToken = default);

    Task<UsuarioDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>RN011 — cria o usuário ativo e com troca de senha obrigatória no primeiro acesso.</summary>
    Task<Resultado<Guid>> CriarAsync(
        UsuarioDados dados, string? senha, string? confirmacaoSenha, CancellationToken cancellationToken = default);

    Task<Resultado> AtualizarAsync(Guid id, UsuarioDados dados, CancellationToken cancellationToken = default);

    Task<Resultado> RedefinirSenhaAsync(
        Guid id, string? novaSenha, string? confirmacaoSenha, CancellationToken cancellationToken = default);

    Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default);

    Task<ResultadoLogin> EntrarAsync(string? email, string? senha, CancellationToken cancellationToken = default);

    Task SairAsync();

    /// <summary>RN010 — troca de senha pelo próprio usuário autenticado.</summary>
    Task<Resultado> AlterarMinhaSenhaAsync(
        string? senhaAtual, string? novaSenha, string? confirmacaoSenha, CancellationToken cancellationToken = default);
}

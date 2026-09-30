using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

/// <summary>
/// Porta de gestão de usuários e autenticação. Credenciais, hash de senha, bloqueio e sessões
/// ficam a cargo da implementação (ASP.NET Core Identity); a Application aplica as regras de negócio.
/// Erros de validação usam os campos "Email", "Senha" e "SenhaAtual".
/// </summary>
public interface IGestaoUsuarios
{
    Task<bool> ExisteUsuarioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pesquisa usuários por nome ou e-mail e situação, ordenados por nome. A consulta é executada no banco.
    /// </summary>
    Task<PaginaResultado<UsuarioResumoDto>> PesquisarAsync(
        FiltroUsuarios filtro, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default);

    Task<UsuarioDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Verifica o e-mail sem diferenciar maiúsculas e minúsculas.</summary>
    Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken cancellationToken = default);

    Task<int> ContarAdministradoresAtivosAsync(CancellationToken cancellationToken = default);

    /// <summary>Usuários ativos de perfil Dentista, ordenados por nome.</summary>
    Task<IReadOnlyList<UsuarioOpcaoDto>> ListarDentistasAtivosAsync(CancellationToken cancellationToken = default);

    Task<Resultado<Guid>> CriarAsync(NovoUsuario usuario, CancellationToken cancellationToken = default);

    /// <summary>Altera nome, e-mail e perfil. A mudança de perfil invalida as sessões do usuário.</summary>
    Task<Resultado> AtualizarAsync(
        Guid id, string nomeCompleto, string email, PerfilUsuario perfil, CancellationToken cancellationToken = default);

    /// <summary>A desativação invalida as sessões do usuário.</summary>
    Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Define uma senha temporária: exige troca no próximo acesso, remove o bloqueio por tentativas
    /// e invalida as sessões do usuário.
    /// </summary>
    Task<Resultado> RedefinirSenhaAsync(Guid id, string novaSenha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Troca a senha do próprio usuário: desmarca a troca obrigatória e invalida as demais sessões,
    /// mantendo a sessão atual.
    /// </summary>
    Task<Resultado> AlterarSenhaAsync(
        Guid id, string senhaAtual, string novaSenha, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida as credenciais e cria a sessão. Usuário inexistente, senha incorreta e usuário desativado
    /// produzem o mesmo resultado. Senhas incorretas contam para o bloqueio temporário.
    /// </summary>
    Task<ResultadoLogin> EntrarAsync(string email, string senha, CancellationToken cancellationToken = default);

    /// <summary>Cria a sessão de um usuário já validado (primeiro administrador).</summary>
    Task AutenticarAsync(Guid id, CancellationToken cancellationToken = default);

    Task SairAsync();
}

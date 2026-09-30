using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

public sealed record UsuarioResumoDto(
    Guid Id,
    string NomeCompleto,
    string Email,
    PerfilUsuario Perfil,
    bool Ativo,
    bool Bloqueado,
    DateTime? UltimoAcesso);

public sealed record UsuarioDto(
    Guid Id,
    string NomeCompleto,
    string Email,
    PerfilUsuario Perfil,
    bool Ativo,
    bool Bloqueado,
    bool DeveTrocarSenha,
    DateTime DataCadastro,
    DateTime? DataAtualizacao,
    DateTime? UltimoAcesso);

/// <summary>Dados mínimos de um usuário para seleção em formulários.</summary>
public sealed record UsuarioOpcaoDto(Guid Id, string NomeCompleto, string Email);

public enum FiltroSituacaoUsuario
{
    Ativos = 1,
    Inativos = 2,
    Bloqueados = 3,
    Todos = 4
}

/// <param name="Busca">Nome ou e-mail do usuário.</param>
public sealed record FiltroUsuarios(string? Busca, FiltroSituacaoUsuario Situacao);

public enum SituacaoLogin
{
    Sucesso = 1,
    CredenciaisInvalidas = 2,
    Bloqueado = 3
}

public sealed record ResultadoLogin(SituacaoLogin Situacao, bool DeveTrocarSenha)
{
    public static ResultadoLogin CredenciaisInvalidas { get; } = new(SituacaoLogin.CredenciaisInvalidas, false);
    public static ResultadoLogin Bloqueado { get; } = new(SituacaoLogin.Bloqueado, false);
}

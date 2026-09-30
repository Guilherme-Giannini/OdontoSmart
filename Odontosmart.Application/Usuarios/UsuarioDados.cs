using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

/// <summary>
/// Dados editáveis de um usuário, informados pelo administrador no cadastro ou na edição.
/// </summary>
public sealed record UsuarioDados(string? NomeCompleto, string? Email, PerfilUsuario? Perfil);

/// <summary>Dados já validados para a criação de um usuário na porta de gestão de usuários.</summary>
public sealed record NovoUsuario(
    string NomeCompleto,
    string Email,
    PerfilUsuario Perfil,
    string Senha,
    bool DeveTrocarSenha);

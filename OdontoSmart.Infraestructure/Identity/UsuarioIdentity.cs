using Microsoft.AspNetCore.Identity;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Infraestructure.Identity;

/// <summary>
/// Usuário do sistema persistido pelo ASP.NET Core Identity. O e-mail também é o nome de usuário (login).
/// Credenciais, bloqueio e security stamp são controlados pelo Identity; não é uma entidade do Domain.
/// </summary>
public class UsuarioIdentity : IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = string.Empty;

    /// <summary>Perfil do usuário; mantido em sincronia com a role do Identity de mesmo nome.</summary>
    public PerfilUsuario Perfil { get; set; }

    public bool Ativo { get; set; }
    public bool DeveTrocarSenha { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public DateTime? UltimoAcesso { get; set; }
}

/// <summary>Perfil de acesso, gravado como role do Identity com o nome do enum <see cref="PerfilUsuario"/>.</summary>
public class PerfilIdentity : IdentityRole<Guid>;

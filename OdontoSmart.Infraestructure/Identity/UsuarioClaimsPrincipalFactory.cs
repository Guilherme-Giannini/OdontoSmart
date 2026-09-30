using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Infraestructure.Identity;

/// <summary>
/// Acrescenta à sessão o nome e a indicação de troca obrigatória de senha. O perfil já é incluído como role.
/// As claims são regeneradas a cada revalidação do security stamp.
/// </summary>
public class UsuarioClaimsPrincipalFactory(
    UserManager<UsuarioIdentity> userManager,
    RoleManager<PerfilIdentity> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<UsuarioIdentity, PerfilIdentity>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(UsuarioIdentity user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(ClaimsUsuario.NomeCompleto, user.NomeCompleto));
        identity.AddClaim(new Claim(ClaimsUsuario.DeveTrocarSenha, user.DeveTrocarSenha ? "true" : "false"));
        return identity;
    }
}

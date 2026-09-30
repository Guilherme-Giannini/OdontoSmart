using System.Security.Claims;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.Autorizacao;

/// <summary>Usuário autenticado obtido do cookie da sessão, nunca de dados do formulário.</summary>
public class UsuarioAtual(IHttpContextAccessor httpContextAccessor) : IUsuarioAtual
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User is { Identity.IsAuthenticated: true } usuario ? usuario : null;

    public Guid? Id =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? NomeCompleto => Principal?.FindFirstValue(ClaimsUsuario.NomeCompleto);

    public PerfilUsuario? Perfil =>
        Principal is { } usuario
            ? Enum.GetValues<PerfilUsuario>().Where(p => usuario.IsInRole(p.ToString())).Cast<PerfilUsuario?>().FirstOrDefault()
            : null;
}

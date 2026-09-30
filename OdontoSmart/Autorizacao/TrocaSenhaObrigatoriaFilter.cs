using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Web.Autorizacao;

/// <summary>
/// RN009 — o usuário com troca de senha obrigatória só acessa a troca de senha e o logout;
/// qualquer outra ação é redirecionada para a troca de senha.
/// </summary>
public class TrocaSenhaObrigatoriaFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;
        if (usuario.Identity?.IsAuthenticated != true || usuario.FindFirstValue(ClaimsUsuario.DeveTrocarSenha) != "true")
            return;

        if (context.ActionDescriptor.EndpointMetadata.OfType<PermitirComTrocaSenhaPendenteAttribute>().Any())
            return;

        context.Result = new RedirectToActionResult("AlterarSenha", "Conta", routeValues: null);
    }
}

/// <summary>Ações acessíveis mesmo com a troca de senha obrigatória pendente.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class PermitirComTrocaSenhaPendenteAttribute : Attribute;

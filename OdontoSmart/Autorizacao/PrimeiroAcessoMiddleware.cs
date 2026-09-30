using OdontoSmart.Application.PrimeiroAcesso;

namespace OdontoSmart.Web.Autorizacao;

/// <summary>
/// RN002 — enquanto não existir nenhum usuário, qualquer acesso é redirecionado para a criação
/// do primeiro administrador.
/// </summary>
public class PrimeiroAcessoMiddleware(RequestDelegate next)
{
    public const string Rota = "/PrimeiroAcesso";

    // Usuários nunca são excluídos: depois que o primeiro existe, a consulta ao banco não é mais necessária.
    private volatile bool _existeUsuario;

    public async Task InvokeAsync(HttpContext context, IPrimeiroAcessoService primeiroAcessoService)
    {
        if (!_existeUsuario && !RotaLiberada(context.Request.Path))
        {
            _existeUsuario = await primeiroAcessoService.ExisteUsuarioAsync(context.RequestAborted);

            if (!_existeUsuario)
            {
                context.Response.Redirect(Rota);
                return;
            }
        }

        await next(context);
    }

    private static bool RotaLiberada(PathString caminho) =>
        caminho.StartsWithSegments(Rota)
        || caminho.StartsWithSegments("/css")
        || caminho.StartsWithSegments("/js")
        || caminho.StartsWithSegments("/favicon.ico")
        || caminho.StartsWithSegments("/Home/Error");
}

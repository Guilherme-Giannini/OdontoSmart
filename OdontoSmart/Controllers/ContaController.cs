using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Web.Autorizacao;
using OdontoSmart.Web.ViewModels.Conta;

namespace OdontoSmart.Web.Controllers;

public class ContaController(IUsuarioService usuarioService) : Controller
{
    private const string MensagemSucesso = "MensagemSucesso";

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await usuarioService.EntrarAsync(model.Email, model.Senha, cancellationToken);

        switch (resultado.Situacao)
        {
            case SituacaoLogin.Bloqueado:
                ModelState.AddModelError(string.Empty,
                    "O acesso está temporariamente bloqueado por excesso de tentativas. Tente novamente mais tarde.");
                return View(model);

            case SituacaoLogin.CredenciaisInvalidas:
                // Mensagem única: não revela se o e-mail está cadastrado (RN006).
                ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
                return View(model);
        }

        if (resultado.DeveTrocarSenha)
            return RedirectToAction(nameof(AlterarSenha));

        // Somente URLs do próprio sistema (proteção contra open redirect).
        if (Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [PermitirComTrocaSenhaPendente]
    public async Task<IActionResult> Sair()
    {
        await usuarioService.SairAsync();

        TempData[MensagemSucesso] = "Você saiu do sistema.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize(Policy = Politicas.AlterarPropriaSenha)]
    [PermitirComTrocaSenhaPendente]
    public IActionResult AlterarSenha() => View(new AlterarSenhaViewModel { Obrigatoria = TrocaSenhaObrigatoria() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Politicas.AlterarPropriaSenha)]
    [PermitirComTrocaSenhaPendente]
    public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var resultado = await usuarioService.AlterarMinhaSenhaAsync(
                model.SenhaAtual, model.NovaSenha, model.ConfirmacaoNovaSenha, cancellationToken);

            if (resultado.Sucesso)
            {
                TempData[MensagemSucesso] = "Senha alterada com sucesso.";
                return RedirectToAction("Index", "Home");
            }

            AdicionarErros(resultado.Erros);
        }

        model.Obrigatoria = TrocaSenhaObrigatoria();
        return View(model);
    }

    [HttpGet]
    public IActionResult AcessoNegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private bool TrocaSenhaObrigatoria() => User.FindFirstValue(ClaimsUsuario.DeveTrocarSenha) == "true";

    private void AdicionarErros(IEnumerable<ErroValidacao> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Campo, erro.Mensagem);
    }
}

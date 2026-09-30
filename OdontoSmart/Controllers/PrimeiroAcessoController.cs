using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.PrimeiroAcesso;
using OdontoSmart.Web.ViewModels.PrimeiroAcesso;

namespace OdontoSmart.Web.Controllers;

/// <summary>
/// RN002 — criação do primeiro administrador. Depois que existe algum usuário, a tela retorna 404.
/// </summary>
[AllowAnonymous]
public class PrimeiroAcessoController(IPrimeiroAcessoService primeiroAcessoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (await primeiroAcessoService.ExisteUsuarioAsync(cancellationToken))
            return NotFound();

        return View(new PrimeiroAcessoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PrimeiroAcessoViewModel model, CancellationToken cancellationToken)
    {
        if (await primeiroAcessoService.ExisteUsuarioAsync(cancellationToken))
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await primeiroAcessoService.CriarAdministradorAsync(model.ParaDados(), cancellationToken);

        // Outra requisição criou o primeiro administrador ao mesmo tempo.
        if (resultado.NaoEncontrado)
            return NotFound();

        if (!resultado.Sucesso)
        {
            foreach (var erro in resultado.Erros)
                ModelState.AddModelError(erro.Campo, erro.Mensagem);

            return View(model);
        }

        TempData["MensagemSucesso"] = "Administrador criado com sucesso. Bem-vindo ao OdontoSmart!";
        return RedirectToAction("Index", "Home");
    }
}

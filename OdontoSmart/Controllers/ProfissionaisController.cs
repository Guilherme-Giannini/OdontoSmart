using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Web.Autorizacao;
using OdontoSmart.Web.ViewModels.Profissionais;

namespace OdontoSmart.Web.Controllers;

[Authorize(Policy = Politicas.ConsultarProfissionais)]
public class ProfissionaisController(IProfissionalService profissionalService) : Controller
{
    private const string MensagemSucesso = "MensagemSucesso";
    private const string MensagemErro = "MensagemErro";

    [HttpGet]
    public async Task<IActionResult> Index(string? busca, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await profissionalService.PesquisarAsync(busca, pagina, cancellationToken);
        return View(ProfissionalListViewModel.De(resultado, busca));
    }

    [HttpGet]
    public async Task<IActionResult> Details([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var profissional = await profissionalService.ObterPorIdAsync(id, cancellationToken);
        if (profissional is null)
            return ProfissionalNaoEncontrado();

        return View(ProfissionalDetailsViewModel.De(profissional));
    }

    [HttpGet]
    [Authorize(Policy = Politicas.GerenciarProfissionais)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = new ProfissionalCreateViewModel();
        await PreencherUsuariosAsync(model, profissionalId: null, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = Politicas.GerenciarProfissionais)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProfissionalCreateViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var resultado = await profissionalService.CriarAsync(model.ParaDados(), cancellationToken);
            if (resultado.Sucesso)
            {
                TempData[MensagemSucesso] = "Profissional cadastrado com sucesso.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }

            AdicionarErros(resultado.Erros);
        }

        await PreencherUsuariosAsync(model, profissionalId: null, cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = Politicas.GerenciarProfissionais)]
    public async Task<IActionResult> Edit([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var profissional = await profissionalService.ObterPorIdAsync(id, cancellationToken);
        if (profissional is null)
            return ProfissionalNaoEncontrado();

        var model = ProfissionalEditViewModel.De(profissional);
        await PreencherUsuariosAsync(model, id, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = Politicas.GerenciarProfissionais)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        [FromRoute] Guid id, ProfissionalEditViewModel model, CancellationToken cancellationToken)
    {
        // O profissional alterado é sempre o identificado pela rota.
        model.Id = id;

        if (ModelState.IsValid)
        {
            var resultado = await profissionalService.AtualizarAsync(id, model.ParaDados(), cancellationToken);
            if (resultado.NaoEncontrado)
                return ProfissionalNaoEncontrado();

            if (resultado.Sucesso)
            {
                TempData[MensagemSucesso] = "Profissional atualizado com sucesso.";
                return RedirectToAction(nameof(Details), new { id });
            }

            AdicionarErros(resultado.Erros);
        }

        await PreencherUsuariosAsync(model, id, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = Politicas.GerenciarProfissionais)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarAtivo([FromRoute] Guid id, bool ativo, CancellationToken cancellationToken)
    {
        var resultado = await profissionalService.AlterarAtivoAsync(id, ativo, cancellationToken);
        if (resultado.NaoEncontrado)
            return ProfissionalNaoEncontrado();

        if (resultado.Sucesso)
            TempData[MensagemSucesso] = ativo ? "Profissional reativado com sucesso." : "Profissional desativado com sucesso.";
        else
            TempData[MensagemErro] = string.Join(" ", resultado.Erros.Select(e => e.Mensagem));

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PreencherUsuariosAsync(
        ProfissionalFormViewModel model, Guid? profissionalId, CancellationToken cancellationToken) =>
        model.DefinirUsuarios(await profissionalService.ListarUsuariosDisponiveisAsync(profissionalId, cancellationToken));

    private void AdicionarErros(IEnumerable<ErroValidacao> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Campo, erro.Mensagem);
    }

    private ViewResult ProfissionalNaoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("ProfissionalNaoEncontrado");
    }
}

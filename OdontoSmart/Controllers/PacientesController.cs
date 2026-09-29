using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.Controllers;

public class PacientesController(IPacienteService pacienteService) : Controller
{
    private const string MensagemSucesso = "MensagemSucesso";
    private const string MensagemErro = "MensagemErro";

    [HttpGet]
    public async Task<IActionResult> Index(string? busca, int pagina = 1, CancellationToken cancellationToken = default)
    {
        var resultado = await pacienteService.PesquisarAsync(busca, pagina, cancellationToken);
        return View(PacienteListViewModel.De(resultado, busca));
    }

    [HttpGet]
    public async Task<IActionResult> Details([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var paciente = await pacienteService.ObterPorIdAsync(id, cancellationToken);
        if (paciente is null)
            return PacienteNaoEncontrado();

        return View(PacienteDetailsViewModel.De(paciente));
    }

    [HttpGet]
    public IActionResult Create() => View(new PacienteCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PacienteCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await pacienteService.CriarAsync(model.ParaDados(), cancellationToken);
        if (!resultado.Sucesso)
        {
            AdicionarErros(resultado.Erros);
            return View(model);
        }

        TempData[MensagemSucesso] = "Paciente cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var paciente = await pacienteService.ObterPorIdAsync(id, cancellationToken);
        if (paciente is null)
            return PacienteNaoEncontrado();

        return View(PacienteEditViewModel.De(paciente));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] Guid id, PacienteEditViewModel model, CancellationToken cancellationToken)
    {
        // O paciente alterado é sempre o identificado pela rota.
        model.Id = id;

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await pacienteService.AtualizarAsync(id, model.ParaDados(), cancellationToken);
        if (resultado.NaoEncontrado)
            return PacienteNaoEncontrado();

        if (!resultado.Sucesso)
        {
            AdicionarErros(resultado.Erros);
            return View(model);
        }

        TempData[MensagemSucesso] = "Paciente atualizado com sucesso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var paciente = await pacienteService.ObterPorIdAsync(id, cancellationToken);
        if (paciente is null)
            return PacienteNaoEncontrado();

        return View(PacienteDetailsViewModel.De(paciente));
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultado = await pacienteService.ExcluirAsync(id, cancellationToken);
        if (resultado.NaoEncontrado)
            return PacienteNaoEncontrado();

        if (!resultado.Sucesso)
        {
            TempData[MensagemErro] = string.Join(" ", resultado.Erros.Select(e => e.Mensagem));
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData[MensagemSucesso] = "Paciente excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private void AdicionarErros(IEnumerable<ErroValidacao> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Campo, erro.Mensagem);
    }

    private ViewResult PacienteNaoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("NaoEncontrado");
    }
}

using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Web.ViewModels.Orcamentos;

namespace OdontoSmart.Web.Controllers;

public class OrcamentosController(
    IOrcamentoService orcamentoService,
    IPacienteService pacienteService) : Controller
{
    private const string MensagemSucesso = "MensagemSucesso";
    private const string MensagemErro = "MensagemErro";

    [HttpGet]
    public async Task<IActionResult> Index(
        string? busca,
        StatusOrcamento? status,
        DateTime? dataInicio,
        DateTime? dataFim,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        var filtro = new FiltroOrcamentos(busca, status, dataInicio, dataFim);
        var resultado = await orcamentoService.PesquisarAsync(filtro, pagina, cancellationToken);

        return View(OrcamentoListViewModel.De(resultado, filtro));
    }

    [HttpGet]
    public async Task<IActionResult> Details([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return OrcamentoNaoEncontrado();

        return View(OrcamentoDetailsViewModel.De(orcamento));
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? pacienteId, CancellationToken cancellationToken)
    {
        var model = OrcamentoCreateViewModel.Novo(pacienteId);
        await PreencherPacientesAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrcamentoCreateViewModel model, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var resultado = await orcamentoService.CriarAsync(model.ParaDados(), cancellationToken);
            if (resultado.Sucesso)
            {
                TempData[MensagemSucesso] = "Orçamento cadastrado com sucesso.";
                return RedirectToAction(nameof(Details), new { id = resultado.Valor });
            }

            AdicionarErros(resultado.Erros);
        }

        await PreencherPacientesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return OrcamentoNaoEncontrado();

        var model = OrcamentoEditViewModel.De(orcamento);
        await PreencherPacientesAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] Guid id, OrcamentoEditViewModel model, CancellationToken cancellationToken)
    {
        // O orçamento alterado é sempre o identificado pela rota.
        var orcamento = await orcamentoService.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return OrcamentoNaoEncontrado();

        model.DefinirCabecalho(orcamento);

        if (ModelState.IsValid)
        {
            var resultado = await orcamentoService.AtualizarAsync(id, model.ParaDados(), cancellationToken);
            if (resultado.NaoEncontrado)
                return OrcamentoNaoEncontrado();

            if (resultado.Sucesso)
            {
                TempData[MensagemSucesso] = "Orçamento atualizado com sucesso.";
                return RedirectToAction(nameof(Details), new { id });
            }

            AdicionarErros(resultado.Erros);
        }

        await PreencherPacientesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarStatus(
        [FromRoute] Guid id, OrcamentoStatusViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || model.NovoStatus is null)
        {
            TempData[MensagemErro] = "Selecione um novo status válido.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var resultado = await orcamentoService.AlterarStatusAsync(id, model.NovoStatus.Value, cancellationToken);
        if (resultado.NaoEncontrado)
            return OrcamentoNaoEncontrado();

        if (resultado.Sucesso)
            TempData[MensagemSucesso] = $"Status do orçamento alterado para \"{OrcamentoFormatacao.Status(model.NovoStatus.Value)}\".";
        else
            TempData[MensagemErro] = string.Join(" ", resultado.Erros.Select(e => e.Mensagem));

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var orcamento = await orcamentoService.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return OrcamentoNaoEncontrado();

        return View(OrcamentoDetailsViewModel.De(orcamento));
    }

    [HttpPost]
    [ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultado = await orcamentoService.ExcluirAsync(id, cancellationToken);
        if (resultado.NaoEncontrado)
            return OrcamentoNaoEncontrado();

        TempData[MensagemSucesso] = "Orçamento excluído com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PreencherPacientesAsync(OrcamentoFormViewModel model, CancellationToken cancellationToken) =>
        model.DefinirPacientes(await pacienteService.ListarOpcoesAsync(cancellationToken));

    private void AdicionarErros(IEnumerable<ErroValidacao> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Campo, erro.Mensagem);
    }

    private ViewResult OrcamentoNaoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("OrcamentoNaoEncontrado");
    }
}

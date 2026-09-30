using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Web.Autorizacao;
using OdontoSmart.Web.ViewModels.Usuarios;

namespace OdontoSmart.Web.Controllers;

[Authorize(Policy = Politicas.GerenciarUsuarios)]
public class UsuariosController(IUsuarioService usuarioService, IUsuarioAtual usuarioAtual) : Controller
{
    private const string MensagemSucesso = "MensagemSucesso";
    private const string MensagemErro = "MensagemErro";

    [HttpGet]
    public async Task<IActionResult> Index(
        string? busca,
        FiltroSituacaoUsuario? situacao,
        int pagina = 1,
        CancellationToken cancellationToken = default)
    {
        // Por padrão, somente usuários ativos são exibidos.
        var filtro = new FiltroUsuarios(busca, situacao ?? FiltroSituacaoUsuario.Ativos);
        var resultado = await usuarioService.PesquisarAsync(filtro, pagina, cancellationToken);

        return View(UsuarioListViewModel.De(resultado, filtro, usuarioAtual.Id));
    }

    [HttpGet]
    public IActionResult Create() => View(new UsuarioCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var resultado = await usuarioService.CriarAsync(
            model.ParaDados(), model.Senha, model.ConfirmacaoSenha, cancellationToken);

        if (!resultado.Sucesso)
        {
            AdicionarErros(resultado.Erros);
            return View(model);
        }

        TempData[MensagemSucesso] =
            "Usuário cadastrado com sucesso. No primeiro acesso, ele deverá trocar a senha informada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return UsuarioNaoEncontrado();

        return View(UsuarioEditViewModel.De(usuario, usuarioAtual.Id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] Guid id, UsuarioEditViewModel model, CancellationToken cancellationToken)
    {
        // O usuário alterado é sempre o identificado pela rota.
        var usuario = await usuarioService.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return UsuarioNaoEncontrado();

        model.DefinirCabecalho(usuario, usuarioAtual.Id);

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await usuarioService.AtualizarAsync(id, model.ParaDados(), cancellationToken);
        if (resultado.NaoEncontrado)
            return UsuarioNaoEncontrado();

        if (!resultado.Sucesso)
        {
            AdicionarErros(resultado.Erros);
            return View(model);
        }

        TempData[MensagemSucesso] = "Usuário atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> RedefinirSenha([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return UsuarioNaoEncontrado();

        var model = new UsuarioRedefinirSenhaViewModel();
        model.DefinirUsuario(usuario);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedefinirSenha(
        [FromRoute] Guid id, UsuarioRedefinirSenhaViewModel model, CancellationToken cancellationToken)
    {
        var usuario = await usuarioService.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return UsuarioNaoEncontrado();

        model.DefinirUsuario(usuario);

        if (!ModelState.IsValid)
            return View(model);

        var resultado = await usuarioService.RedefinirSenhaAsync(
            id, model.NovaSenha, model.ConfirmacaoNovaSenha, cancellationToken);

        if (resultado.NaoEncontrado)
            return UsuarioNaoEncontrado();

        if (!resultado.Sucesso)
        {
            AdicionarErros(resultado.Erros);
            return View(model);
        }

        TempData[MensagemSucesso] =
            $"Senha de {usuario.NomeCompleto} redefinida. No próximo acesso, o usuário deverá trocá-la.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlterarAtivo([FromRoute] Guid id, bool ativo, CancellationToken cancellationToken)
    {
        var resultado = await usuarioService.AlterarAtivoAsync(id, ativo, cancellationToken);
        if (resultado.NaoEncontrado)
            return UsuarioNaoEncontrado();

        if (resultado.Sucesso)
            TempData[MensagemSucesso] = ativo ? "Usuário reativado com sucesso." : "Usuário desativado com sucesso.";
        else
            TempData[MensagemErro] = string.Join(" ", resultado.Erros.Select(e => e.Mensagem));

        return RedirectToAction(nameof(Index), new { situacao = FiltroSituacaoUsuario.Todos });
    }

    private void AdicionarErros(IEnumerable<ErroValidacao> erros)
    {
        foreach (var erro in erros)
            ModelState.AddModelError(erro.Campo, erro.Mensagem);
    }

    private ViewResult UsuarioNaoEncontrado()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        return View("UsuarioNaoEncontrado");
    }
}

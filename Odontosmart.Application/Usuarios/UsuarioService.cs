using OdontoSmart.Application.Common;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

public class UsuarioService(
    IGestaoUsuarios gestaoUsuarios,
    IProfissionalRepository profissionalRepository,
    ITransacoes transacoes,
    IUsuarioAtual usuarioAtual) : IUsuarioService
{
    public const int TamanhoPagina = 10;

    // Nomes dos campos de senha nos formulários (as senhas não fazem parte de UsuarioDados).
    public const string CampoSenha = "Senha";
    public const string CampoConfirmacaoSenha = "ConfirmacaoSenha";
    public const string CampoSenhaAtual = "SenhaAtual";
    public const string CampoNovaSenha = "NovaSenha";
    public const string CampoConfirmacaoNovaSenha = "ConfirmacaoNovaSenha";

    private static readonly ErroValidacao ErroEmailDuplicado =
        new(nameof(UsuarioDados.Email), "Já existe um usuário cadastrado com este e-mail.");

    public Task<PaginaResultado<UsuarioResumoDto>> PesquisarAsync(
        FiltroUsuarios filtro, int pagina, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(filtro.Situacao))
            filtro = filtro with { Situacao = FiltroSituacaoUsuario.Ativos };

        filtro = filtro with { Busca = filtro.Busca?.Trim() };

        return gestaoUsuarios.PesquisarAsync(filtro, Math.Max(pagina, 1), TamanhoPagina, cancellationToken);
    }

    public Task<UsuarioDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        gestaoUsuarios.ObterPorIdAsync(id, cancellationToken);

    public async Task<Resultado<Guid>> CriarAsync(
        UsuarioDados dados, string? senha, string? confirmacaoSenha, CancellationToken cancellationToken = default)
    {
        var erros = UsuarioValidador.Validar(dados);
        erros.AddRange(SenhaValidador.Validar(senha, confirmacaoSenha, CampoSenha, CampoConfirmacaoSenha));

        var email = dados.Email?.Trim();
        if (erros.All(e => e.Campo != nameof(UsuarioDados.Email))
            && await gestaoUsuarios.ExisteEmailAsync(email!, null, cancellationToken))
        {
            erros.Add(ErroEmailDuplicado);
        }

        if (erros.Count > 0)
            return Resultado<Guid>.Falha(erros);

        return await gestaoUsuarios.CriarAsync(
            new NovoUsuario(dados.NomeCompleto!.Trim(), email!, dados.Perfil!.Value, senha!, DeveTrocarSenha: true),
            cancellationToken);
    }

    public async Task<Resultado> AtualizarAsync(Guid id, UsuarioDados dados, CancellationToken cancellationToken = default)
    {
        var erros = UsuarioValidador.Validar(dados);
        if (erros.Count > 0)
            return Resultado.Falha(erros);

        // Serializa as alterações de perfil: dois administradores rebaixando um ao outro ao mesmo tempo
        // não podem deixar o sistema sem administrador (RN015).
        await using var transacao = await transacoes.IniciarExclusivaAsync(cancellationToken);

        var usuario = await gestaoUsuarios.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return Resultado.RecursoNaoEncontrado();

        var novoPerfil = dados.Perfil!.Value;
        if (novoPerfil != usuario.Perfil)
        {
            var erroPerfil = await ValidarAlteracaoPerfilAsync(usuario, cancellationToken);
            if (erroPerfil is not null)
                erros.Add(erroPerfil);
        }

        var email = dados.Email!.Trim();
        if (await gestaoUsuarios.ExisteEmailAsync(email, id, cancellationToken))
            erros.Add(ErroEmailDuplicado);

        if (erros.Count > 0)
            return Resultado.Falha(erros);

        var resultado = await gestaoUsuarios.AtualizarAsync(
            id, dados.NomeCompleto!.Trim(), email, novoPerfil, cancellationToken);
        if (!resultado.Sucesso)
            return resultado;

        await transacao.ConfirmarAsync(cancellationToken);
        return Resultado.Ok();
    }

    public async Task<Resultado> RedefinirSenhaAsync(
        Guid id, string? novaSenha, string? confirmacaoSenha, CancellationToken cancellationToken = default)
    {
        var usuario = await gestaoUsuarios.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return Resultado.RecursoNaoEncontrado();

        // RN013 — o administrador troca a própria senha pela troca de senha comum (RN010).
        if (usuario.Id == usuarioAtual.Id)
            return Resultado.Falha([new(string.Empty,
                "Para alterar a sua própria senha, utilize a opção \"Alterar senha\" do menu do usuário.")]);

        var erros = SenhaValidador.Validar(novaSenha, confirmacaoSenha, CampoNovaSenha, CampoConfirmacaoNovaSenha);
        if (erros.Count > 0)
            return Resultado.Falha(erros);

        var resultado = await gestaoUsuarios.RedefinirSenhaAsync(id, novaSenha!, cancellationToken);
        return RenomearCampoSenha(resultado, CampoNovaSenha);
    }

    public async Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default)
    {
        await using var transacao = await transacoes.IniciarExclusivaAsync(cancellationToken);

        var usuario = await gestaoUsuarios.ObterPorIdAsync(id, cancellationToken);
        if (usuario is null)
            return Resultado.RecursoNaoEncontrado();

        if (usuario.Ativo == ativo)
            return Resultado.Falha([new(string.Empty,
                ativo ? "O usuário já está ativo." : "O usuário já está inativo.")]);

        if (!ativo)
        {
            // RN015 — o sistema deve sempre possuir pelo menos um administrador ativo.
            if (usuario.Id == usuarioAtual.Id)
                return Resultado.Falha([new(string.Empty, "Você não pode desativar o seu próprio usuário.")]);

            if (usuario.Perfil == PerfilUsuario.Administrador
                && await gestaoUsuarios.ContarAdministradoresAtivosAsync(cancellationToken) <= 1)
                return Resultado.Falha([new(string.Empty,
                    "Não é possível desativar o último administrador ativo do sistema.")]);
        }

        var resultado = await gestaoUsuarios.AlterarAtivoAsync(id, ativo, cancellationToken);
        if (!resultado.Sucesso)
            return resultado;

        await transacao.ConfirmarAsync(cancellationToken);
        return Resultado.Ok();
    }

    public Task<ResultadoLogin> EntrarAsync(string? email, string? senha, CancellationToken cancellationToken = default)
    {
        // Dados ausentes recebem a mesma resposta genérica de credenciais inválidas (RN006).
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(senha))
            return Task.FromResult(ResultadoLogin.CredenciaisInvalidas);

        return gestaoUsuarios.EntrarAsync(email.Trim(), senha, cancellationToken);
    }

    public Task SairAsync() => gestaoUsuarios.SairAsync();

    public async Task<Resultado> AlterarMinhaSenhaAsync(
        string? senhaAtual, string? novaSenha, string? confirmacaoSenha, CancellationToken cancellationToken = default)
    {
        if (usuarioAtual.Id is not { } id)
            return Resultado.RecursoNaoEncontrado();

        var erros = new List<ErroValidacao>();
        if (string.IsNullOrEmpty(senhaAtual))
            erros.Add(new(CampoSenhaAtual, "Informe a senha atual."));

        erros.AddRange(SenhaValidador.Validar(novaSenha, confirmacaoSenha, CampoNovaSenha, CampoConfirmacaoNovaSenha));

        if (!string.IsNullOrEmpty(senhaAtual) && novaSenha == senhaAtual)
            erros.Add(new(CampoNovaSenha, "A nova senha deve ser diferente da senha atual."));

        if (erros.Count > 0)
            return Resultado.Falha(erros);

        var resultado = await gestaoUsuarios.AlterarSenhaAsync(id, senhaAtual!, novaSenha!, cancellationToken);
        return RenomearCampoSenha(resultado, CampoNovaSenha);
    }

    /// <summary>RN015 e RN017 — verificações feitas antes de alterar o perfil de um usuário.</summary>
    private async Task<ErroValidacao?> ValidarAlteracaoPerfilAsync(UsuarioDto usuario, CancellationToken cancellationToken)
    {
        const string campo = nameof(UsuarioDados.Perfil);

        if (usuario.Id == usuarioAtual.Id)
            return new(campo, "Você não pode alterar o seu próprio perfil.");

        if (usuario.Perfil == PerfilUsuario.Administrador
            && usuario.Ativo
            && await gestaoUsuarios.ContarAdministradoresAtivosAsync(cancellationToken) <= 1)
            return new(campo, "Não é possível alterar o perfil do último administrador ativo do sistema.");

        if (usuario.Perfil == PerfilUsuario.Dentista
            && await profissionalRepository.PossuiVinculoAsync(usuario.Id, null, cancellationToken))
            return new(campo,
                "Este usuário está vinculado a um profissional. Desfaça o vínculo no cadastro do profissional antes de alterar o perfil.");

        return null;
    }

    /// <summary>A porta reporta erros de senha no campo "Senha"; o formulário pode usar outro nome.</summary>
    private static Resultado RenomearCampoSenha(Resultado resultado, string campo) =>
        resultado.Sucesso || resultado.NaoEncontrado
            ? resultado
            : Resultado.Falha(resultado.Erros
                .Select(e => e.Campo == CampoSenha ? e with { Campo = campo } : e)
                .ToList());
}

using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Profissionais;

public class ProfissionalService(
    IProfissionalRepository repository,
    IGestaoUsuarios gestaoUsuarios,
    ITransacoes transacoes) : IProfissionalService
{
    public const int TamanhoPagina = 10;

    private static readonly ErroValidacao ErroCroDuplicado =
        new(nameof(ProfissionalDados.Cro), "Já existe um profissional cadastrado com este CRO nesta UF.");

    private static readonly ErroValidacao ErroUsuarioJaVinculado =
        new(nameof(ProfissionalDados.UsuarioId), "O usuário selecionado já está vinculado a outro profissional.");

    public Task<PaginaResultado<ProfissionalResumoDto>> PesquisarAsync(
        string? termo, int pagina, CancellationToken cancellationToken = default) =>
        repository.PesquisarAsync(termo?.Trim(), Math.Max(pagina, 1), TamanhoPagina, cancellationToken);

    public async Task<ProfissionalDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profissional = await repository.ObterPorIdAsync(id, cancellationToken);
        if (profissional is null)
            return null;

        var usuario = profissional.UsuarioId is { } usuarioId
            ? await gestaoUsuarios.ObterPorIdAsync(usuarioId, cancellationToken)
            : null;

        return new ProfissionalDto(
            profissional.Id,
            profissional.NomeExibicao,
            profissional.Cro,
            profissional.CroUf,
            profissional.Especialidade,
            profissional.Telefone,
            profissional.Email,
            profissional.UsuarioId,
            usuario?.NomeCompleto,
            usuario?.Email,
            profissional.Ativo,
            profissional.DataCadastro,
            profissional.DataAtualizacao);
    }

    public async Task<IReadOnlyList<UsuarioOpcaoDto>> ListarUsuariosDisponiveisAsync(
        Guid? profissionalId, CancellationToken cancellationToken = default)
    {
        var usuarioVinculadoId = profissionalId is { } id
            ? (await repository.ObterPorIdAsync(id, cancellationToken))?.UsuarioId
            : null;

        var vinculados = await repository.ListarUsuariosVinculadosAsync(cancellationToken);
        var dentistas = await gestaoUsuarios.ListarDentistasAtivosAsync(cancellationToken);

        var disponiveis = dentistas
            .Where(u => u.Id == usuarioVinculadoId || !vinculados.Contains(u.Id))
            .ToList();

        // O usuário já vinculado continua disponível mesmo que tenha sido desativado.
        if (usuarioVinculadoId is { } vinculadoId
            && disponiveis.All(u => u.Id != vinculadoId)
            && await gestaoUsuarios.ObterPorIdAsync(vinculadoId, cancellationToken) is { } vinculado)
        {
            disponiveis.Add(new UsuarioOpcaoDto(vinculado.Id, vinculado.NomeCompleto, vinculado.Email));
        }

        return disponiveis.OrderBy(u => u.NomeCompleto).ToList();
    }

    public async Task<Resultado<Guid>> CriarAsync(ProfissionalDados dados, CancellationToken cancellationToken = default)
    {
        // A transação exclusiva impede que o perfil do usuário vinculado seja alterado ao mesmo tempo (RN017).
        await using var transacao = await transacoes.IniciarExclusivaAsync(cancellationToken);

        var erros = await ValidarAsync(dados, profissional: null, cancellationToken);
        if (erros.Count > 0)
            return Resultado<Guid>.Falha(erros);

        var profissional = Profissional.Criar(
            dados.NomeExibicao!, dados.Cro!, dados.CroUf!, dados.Especialidade,
            dados.Telefone, dados.Email, dados.UsuarioId);

        repository.Adicionar(profissional);

        var erroPersistencia = await SalvarAsync(cancellationToken);
        if (erroPersistencia is not null)
            return Resultado<Guid>.Falha([erroPersistencia]);

        await transacao.ConfirmarAsync(cancellationToken);
        return Resultado<Guid>.Ok(profissional.Id);
    }

    public async Task<Resultado> AtualizarAsync(Guid id, ProfissionalDados dados, CancellationToken cancellationToken = default)
    {
        await using var transacao = await transacoes.IniciarExclusivaAsync(cancellationToken);

        var profissional = await repository.ObterPorIdAsync(id, cancellationToken);
        if (profissional is null)
            return Resultado.RecursoNaoEncontrado();

        var erros = await ValidarAsync(dados, profissional, cancellationToken);
        if (erros.Count > 0)
            return Resultado.Falha(erros);

        profissional.Atualizar(
            dados.NomeExibicao!, dados.Cro!, dados.CroUf!, dados.Especialidade,
            dados.Telefone, dados.Email, dados.UsuarioId);

        var erroPersistencia = await SalvarAsync(cancellationToken);
        if (erroPersistencia is not null)
            return Resultado.Falha([erroPersistencia]);

        await transacao.ConfirmarAsync(cancellationToken);
        return Resultado.Ok();
    }

    public async Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default)
    {
        var profissional = await repository.ObterPorIdAsync(id, cancellationToken);
        if (profissional is null)
            return Resultado.RecursoNaoEncontrado();

        if (profissional.Ativo == ativo)
            return Resultado.Falha([new(string.Empty,
                ativo ? "O profissional já está ativo." : "O profissional já está inativo.")]);

        if (ativo)
            profissional.Ativar();
        else
            profissional.Desativar();

        await repository.SalvarAlteracoesAsync(cancellationToken);
        return Resultado.Ok();
    }

    private async Task<List<ErroValidacao>> ValidarAsync(
        ProfissionalDados dados, Profissional? profissional, CancellationToken cancellationToken)
    {
        var erros = ProfissionalValidador.Validar(dados);

        if (erros.All(e => e.Campo is not (nameof(ProfissionalDados.Cro) or nameof(ProfissionalDados.CroUf)))
            && await repository.ExisteCroAsync(
                UnidadeFederativa.Normalizar(dados.CroUf)!, Profissional.NormalizarCro(dados.Cro)!,
                profissional?.Id, cancellationToken))
        {
            erros.Add(ErroCroDuplicado);
        }

        if (dados.UsuarioId is { } usuarioId)
        {
            var erro = await ValidarVinculoAsync(usuarioId, profissional, cancellationToken);
            if (erro is not null)
                erros.Add(erro);
        }

        return erros;
    }

    /// <summary>RN017 — somente usuários de perfil Dentista, vinculados a no máximo um profissional.</summary>
    private async Task<ErroValidacao?> ValidarVinculoAsync(
        Guid usuarioId, Profissional? profissional, CancellationToken cancellationToken)
    {
        const string campo = nameof(ProfissionalDados.UsuarioId);

        var usuario = await gestaoUsuarios.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario is null)
            return new(campo, "O usuário selecionado não existe.");

        if (usuario.Perfil != PerfilUsuario.Dentista)
            return new(campo, "Somente usuários com perfil Dentista podem ser vinculados a um profissional.");

        if (!usuario.Ativo && usuarioId != profissional?.UsuarioId)
            return new(campo, "O usuário selecionado está inativo.");

        if (await repository.PossuiVinculoAsync(usuarioId, profissional?.Id, cancellationToken))
            return ErroUsuarioJaVinculado;

        return null;
    }

    private async Task<ErroValidacao?> SalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            await repository.SalvarAlteracoesAsync(cancellationToken);
            return null;
        }
        catch (CroDuplicadoException)
        {
            return ErroCroDuplicado;
        }
        catch (UsuarioJaVinculadoException)
        {
            return ErroUsuarioJaVinculado;
        }
    }
}

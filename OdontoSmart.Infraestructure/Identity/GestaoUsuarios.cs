using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Data.Configurations;

namespace OdontoSmart.Infraestructure.Identity;

/// <summary>
/// Implementação de <see cref="IGestaoUsuarios"/> sobre o ASP.NET Core Identity.
/// Os erros do Identity são traduzidos para <see cref="ErroValidacao"/> em português.
/// </summary>
public class GestaoUsuarios(
    UserManager<UsuarioIdentity> userManager,
    SignInManager<UsuarioIdentity> signInManager,
    ApplicationDbContext context) : IGestaoUsuarios
{
    private const string CampoEmail = "Email";
    private const string CampoSenha = "Senha";
    private const string CampoSenhaAtual = "SenhaAtual";

    private static readonly ErroValidacao ErroEmailDuplicado =
        new(CampoEmail, "Já existe um usuário cadastrado com este e-mail.");

    public Task<bool> ExisteUsuarioAsync(CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(cancellationToken);

    public async Task<PaginaResultado<UsuarioResumoDto>> PesquisarAsync(
        FiltroUsuarios filtro, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padrao = Like.Contem(filtro.Busca.Trim());
            query = query.Where(u =>
                EF.Functions.ILike(u.NomeCompleto, padrao) || EF.Functions.ILike(u.Email!, padrao));
        }

        query = filtro.Situacao switch
        {
            FiltroSituacaoUsuario.Ativos => query.Where(u => u.Ativo),
            FiltroSituacaoUsuario.Inativos => query.Where(u => !u.Ativo),
            FiltroSituacaoUsuario.Bloqueados => query.Where(u => u.Ativo && u.LockoutEnd != null && u.LockoutEnd > agora),
            _ => query
        };

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(u => u.NomeCompleto)
            .ThenBy(u => u.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(u => new UsuarioResumoDto(
                u.Id,
                u.NomeCompleto,
                u.Email!,
                u.Perfil,
                u.Ativo,
                u.LockoutEnd != null && u.LockoutEnd > agora,
                u.UltimoAcesso))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<UsuarioResumoDto>(itens, total, pagina, tamanhoPagina);
    }

    public Task<UsuarioDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agora = DateTimeOffset.UtcNow;

        return context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UsuarioDto(
                u.Id,
                u.NomeCompleto,
                u.Email!,
                u.Perfil,
                u.Ativo,
                u.LockoutEnd != null && u.LockoutEnd > agora,
                u.DeveTrocarSenha,
                u.DataCadastro,
                u.DataAtualizacao,
                u.UltimoAcesso))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken cancellationToken = default)
    {
        var emailNormalizado = userManager.NormalizeEmail(email.Trim());

        return context.Users.AnyAsync(
            u => u.NormalizedEmail == emailNormalizado && (ignorarUsuarioId == null || u.Id != ignorarUsuarioId),
            cancellationToken);
    }

    public Task<int> ContarAdministradoresAtivosAsync(CancellationToken cancellationToken = default) =>
        context.Users.CountAsync(u => u.Ativo && u.Perfil == PerfilUsuario.Administrador, cancellationToken);

    public async Task<IReadOnlyList<UsuarioOpcaoDto>> ListarDentistasAtivosAsync(CancellationToken cancellationToken = default) =>
        await context.Users
            .AsNoTracking()
            .Where(u => u.Ativo && u.Perfil == PerfilUsuario.Dentista)
            .OrderBy(u => u.NomeCompleto)
            .Select(u => new UsuarioOpcaoDto(u.Id, u.NomeCompleto, u.Email!))
            .ToListAsync(cancellationToken);

    public Task<Resultado<Guid>> CriarAsync(NovoUsuario usuario, CancellationToken cancellationToken = default) =>
        EmTransacaoAsync(async () =>
        {
            var novo = new UsuarioIdentity
            {
                Id = Guid.NewGuid(),
                NomeCompleto = usuario.NomeCompleto,
                UserName = usuario.Email,
                Email = usuario.Email,
                Perfil = usuario.Perfil,
                Ativo = true,
                DeveTrocarSenha = usuario.DeveTrocarSenha,
                DataCadastro = DateTime.UtcNow
            };

            var erros = await ExecutarAsync(() => userManager.CreateAsync(novo, usuario.Senha));
            erros ??= await ExecutarAsync(() => userManager.AddToRoleAsync(novo, usuario.Perfil.ToString()));

            return erros is null ? Resultado<Guid>.Ok(novo.Id) : Resultado<Guid>.Falha(erros);
        }, cancellationToken);

    public Task<Resultado> AtualizarAsync(
        Guid id, string nomeCompleto, string email, PerfilUsuario perfil, CancellationToken cancellationToken = default) =>
        EmTransacaoAsync(async () =>
        {
            var usuario = await userManager.FindByIdAsync(id.ToString());
            if (usuario is null)
                return Resultado.RecursoNaoEncontrado();

            var perfilAnterior = usuario.Perfil;

            usuario.NomeCompleto = nomeCompleto;
            usuario.Email = email;
            usuario.UserName = email;
            usuario.Perfil = perfil;
            usuario.DataAtualizacao = DateTime.UtcNow;

            var erros = await ExecutarAsync(() => userManager.UpdateAsync(usuario));

            if (erros is null && perfil != perfilAnterior)
            {
                erros = await ExecutarAsync(() => userManager.RemoveFromRoleAsync(usuario, perfilAnterior.ToString()));
                erros ??= await ExecutarAsync(() => userManager.AddToRoleAsync(usuario, perfil.ToString()));

                // A mudança de perfil invalida as sessões abertas do usuário (RN008).
                erros ??= await ExecutarAsync(() => userManager.UpdateSecurityStampAsync(usuario));
            }

            return erros is null ? Resultado.Ok() : Resultado.Falha(erros);
        }, cancellationToken);

    public Task<Resultado> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken cancellationToken = default) =>
        EmTransacaoAsync(async () =>
        {
            var usuario = await userManager.FindByIdAsync(id.ToString());
            if (usuario is null)
                return Resultado.RecursoNaoEncontrado();

            usuario.Ativo = ativo;
            usuario.DataAtualizacao = DateTime.UtcNow;

            // A desativação invalida as sessões abertas do usuário (RN008, RN014).
            var erros = ativo
                ? await ExecutarAsync(() => userManager.UpdateAsync(usuario))
                : await ExecutarAsync(() => userManager.UpdateSecurityStampAsync(usuario));

            return erros is null ? Resultado.Ok() : Resultado.Falha(erros);
        }, cancellationToken);

    public Task<Resultado> RedefinirSenhaAsync(Guid id, string novaSenha, CancellationToken cancellationToken = default) =>
        EmTransacaoAsync(async () =>
        {
            var usuario = await userManager.FindByIdAsync(id.ToString());
            if (usuario is null)
                return Resultado.RecursoNaoEncontrado();

            // Gravados junto com a nova senha: exige troca no próximo acesso e remove o bloqueio (RN013).
            usuario.DeveTrocarSenha = true;
            usuario.LockoutEnd = null;
            usuario.AccessFailedCount = 0;
            usuario.DataAtualizacao = DateTime.UtcNow;

            // A redefinição atualiza o security stamp, invalidando as sessões abertas (RN008).
            var token = await userManager.GeneratePasswordResetTokenAsync(usuario);
            var erros = await ExecutarAsync(() => userManager.ResetPasswordAsync(usuario, token, novaSenha));

            return erros is null ? Resultado.Ok() : Resultado.Falha(erros);
        }, cancellationToken);

    public async Task<Resultado> AlterarSenhaAsync(
        Guid id, string senhaAtual, string novaSenha, CancellationToken cancellationToken = default)
    {
        var usuario = await userManager.FindByIdAsync(id.ToString());
        if (usuario is null)
            return Resultado.RecursoNaoEncontrado();

        usuario.DeveTrocarSenha = false;
        usuario.DataAtualizacao = DateTime.UtcNow;

        // A troca atualiza o security stamp: as demais sessões deixam de ser válidas (RN010).
        var erros = await ExecutarAsync(() => userManager.ChangePasswordAsync(usuario, senhaAtual, novaSenha));
        if (erros is not null)
        {
            context.ChangeTracker.Clear();
            return Resultado.Falha(erros);
        }

        // Renova o cookie da sessão atual com o novo stamp e sem a troca obrigatória.
        await signInManager.RefreshSignInAsync(usuario);
        return Resultado.Ok();
    }

    public async Task<ResultadoLogin> EntrarAsync(string email, string senha, CancellationToken cancellationToken = default)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null || !usuario.Ativo)
        {
            // Calcula um hash descartável para que o tempo de resposta não revele se o e-mail existe (RN006).
            userManager.PasswordHasher.HashPassword(new UsuarioIdentity(), senha);
            return ResultadoLogin.CredenciaisInvalidas;
        }

        // Senhas incorretas contam para o bloqueio temporário (RN007); o sucesso zera a contagem.
        var resultado = await signInManager.PasswordSignInAsync(usuario, senha, isPersistent: false, lockoutOnFailure: true);

        if (resultado.IsLockedOut)
            return ResultadoLogin.Bloqueado;

        if (!resultado.Succeeded)
            return ResultadoLogin.CredenciaisInvalidas;

        await RegistrarAcessoAsync(usuario);
        return new ResultadoLogin(SituacaoLogin.Sucesso, usuario.DeveTrocarSenha);
    }

    public async Task AutenticarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await userManager.FindByIdAsync(id.ToString())
            ?? throw new InvalidOperationException("Usuário não encontrado para autenticação.");

        await signInManager.SignInAsync(usuario, isPersistent: false);
        await RegistrarAcessoAsync(usuario);
    }

    public Task SairAsync() => signInManager.SignOutAsync();

    private async Task RegistrarAcessoAsync(UsuarioIdentity usuario)
    {
        usuario.UltimoAcesso = DateTime.UtcNow;

        // Uma falha de concorrência aqui não deve impedir o acesso: apenas o último acesso deixa de ser gravado.
        var resultado = await userManager.UpdateAsync(usuario);
        if (!resultado.Succeeded)
            context.ChangeTracker.Clear();
    }

    /// <summary>
    /// Executa uma operação em transação (ou na transação já aberta pelo chamador), confirmando somente
    /// em caso de sucesso — criação do usuário e atribuição do perfil ocorrem juntas ou não ocorrem.
    /// </summary>
    private async Task<TResultado> EmTransacaoAsync<TResultado>(
        Func<Task<TResultado>> operacao, CancellationToken cancellationToken) where TResultado : Resultado
    {
        if (context.Database.CurrentTransaction is not null)
            return await operacao();

        await using var transacao = await context.Database.BeginTransactionAsync(cancellationToken);

        var resultado = await operacao();
        if (resultado.Sucesso)
            await transacao.CommitAsync(cancellationToken);
        else
            context.ChangeTracker.Clear();

        return resultado;
    }

    /// <summary>
    /// Executa uma operação do Identity e retorna os erros traduzidos, ou null em caso de sucesso.
    /// A violação do índice único de e-mail (cadastros simultâneos) é tratada como e-mail duplicado.
    /// </summary>
    private async Task<IReadOnlyList<ErroValidacao>?> ExecutarAsync(Func<Task<IdentityResult>> operacao)
    {
        try
        {
            var resultado = await operacao();
            return resultado.Succeeded ? null : resultado.Errors.Select(Traduzir).Distinct().ToList();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: UsuarioConfiguration.IndiceEmailUnico or UsuarioConfiguration.IndiceNomeUsuarioUnico
        })
        {
            // Mantém o contexto utilizável após a falha.
            context.ChangeTracker.Clear();
            return [ErroEmailDuplicado];
        }
    }

    private ErroValidacao Traduzir(IdentityError erro) => erro.Code switch
    {
        nameof(IdentityErrorDescriber.DuplicateEmail) or nameof(IdentityErrorDescriber.DuplicateUserName) =>
            ErroEmailDuplicado,
        nameof(IdentityErrorDescriber.InvalidEmail) or nameof(IdentityErrorDescriber.InvalidUserName) =>
            new(CampoEmail, "Informe um e-mail válido."),
        nameof(IdentityErrorDescriber.PasswordMismatch) =>
            new(CampoSenhaAtual, "A senha atual está incorreta."),
        nameof(IdentityErrorDescriber.PasswordTooShort) =>
            new(CampoSenha, $"A senha deve possuir pelo menos {userManager.Options.Password.RequiredLength} caracteres."),
        nameof(IdentityErrorDescriber.PasswordRequiresDigit) =>
            new(CampoSenha, "A senha deve conter pelo menos um número."),
        nameof(IdentityErrorDescriber.PasswordRequiresLower)
            or nameof(IdentityErrorDescriber.PasswordRequiresUpper)
            or nameof(IdentityErrorDescriber.PasswordRequiresNonAlphanumeric)
            or nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) =>
            new(CampoSenha, "A senha não atende aos requisitos de segurança."),
        nameof(IdentityErrorDescriber.ConcurrencyFailure) =>
            new(string.Empty, "O usuário foi alterado por outra pessoa ao mesmo tempo. Recarregue a página e tente novamente."),
        _ => new(string.Empty, "Não foi possível concluir a operação com o usuário.")
    };
}

using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.PrimeiroAcesso;

/// <summary>Dados do primeiro administrador, informados na primeira execução do sistema.</summary>
public sealed record PrimeiroAdministradorDados(
    string? NomeCompleto,
    string? Email,
    string? Senha,
    string? ConfirmacaoSenha);

public interface IPrimeiroAcessoService
{
    /// <summary>Indica se já existe algum usuário cadastrado (RN002).</summary>
    Task<bool> ExisteUsuarioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria o primeiro administrador e o autentica. Retorna <see cref="Resultado.NaoEncontrado"/>
    /// quando já existe algum usuário.
    /// </summary>
    Task<Resultado> CriarAdministradorAsync(PrimeiroAdministradorDados dados, CancellationToken cancellationToken = default);
}

public class PrimeiroAcessoService(IGestaoUsuarios gestaoUsuarios, ITransacoes transacoes) : IPrimeiroAcessoService
{
    public Task<bool> ExisteUsuarioAsync(CancellationToken cancellationToken = default) =>
        gestaoUsuarios.ExisteUsuarioAsync(cancellationToken);

    public async Task<Resultado> CriarAdministradorAsync(
        PrimeiroAdministradorDados dados, CancellationToken cancellationToken = default)
    {
        var erros = UsuarioValidador.Validar(
            new UsuarioDados(dados.NomeCompleto, dados.Email, PerfilUsuario.Administrador));
        erros.AddRange(SenhaValidador.Validar(
            dados.Senha, dados.ConfirmacaoSenha, UsuarioService.CampoSenha, UsuarioService.CampoConfirmacaoSenha));

        if (erros.Count > 0)
            return Resultado.Falha(erros);

        Guid id;

        // Requisições simultâneas são serializadas: somente a primeira encontra o sistema sem usuários.
        await using (var transacao = await transacoes.IniciarExclusivaAsync(cancellationToken))
        {
            if (await gestaoUsuarios.ExisteUsuarioAsync(cancellationToken))
                return Resultado.RecursoNaoEncontrado();

            // O primeiro administrador definiu a própria senha: não precisa trocá-la (RN002).
            var resultado = await gestaoUsuarios.CriarAsync(
                new NovoUsuario(dados.NomeCompleto!.Trim(), dados.Email!.Trim(), PerfilUsuario.Administrador,
                    dados.Senha!, DeveTrocarSenha: false),
                cancellationToken);

            if (!resultado.Sucesso)
                return Resultado.Falha(resultado.Erros);

            await transacao.ConfirmarAsync(cancellationToken);
            id = resultado.Valor;
        }

        await gestaoUsuarios.AutenticarAsync(id, cancellationToken);
        return Resultado.Ok();
    }
}

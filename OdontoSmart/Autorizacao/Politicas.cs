using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.Autorizacao;

/// <summary>
/// Políticas de autorização nomeadas por funcionalidade. A matriz de permissões (spec de Usuários §4.1)
/// é definida somente aqui: para mudar quem pode fazer o quê, altere <see cref="Matriz"/>.
/// </summary>
public static class Politicas
{
    public const string ConsultarPacientes = nameof(ConsultarPacientes);
    public const string EditarPacientes = nameof(EditarPacientes);
    public const string ExcluirPacientes = nameof(ExcluirPacientes);

    public const string ConsultarOrcamentos = nameof(ConsultarOrcamentos);
    public const string EditarOrcamentos = nameof(EditarOrcamentos);
    public const string AlterarStatusOrcamentos = nameof(AlterarStatusOrcamentos);
    public const string ExcluirOrcamentos = nameof(ExcluirOrcamentos);

    public const string GerenciarUsuarios = nameof(GerenciarUsuarios);

    public const string ConsultarProfissionais = nameof(ConsultarProfissionais);
    public const string GerenciarProfissionais = nameof(GerenciarProfissionais);

    public const string AlterarPropriaSenha = nameof(AlterarPropriaSenha);

    private static readonly PerfilUsuario[] Todos =
        [PerfilUsuario.Administrador, PerfilUsuario.Dentista, PerfilUsuario.Recepcao];

    private static readonly PerfilUsuario[] SomenteAdministrador = [PerfilUsuario.Administrador];

    private static readonly Dictionary<string, PerfilUsuario[]> Matriz = new()
    {
        [ConsultarPacientes] = Todos,
        [EditarPacientes] = Todos,
        [ExcluirPacientes] = SomenteAdministrador,

        [ConsultarOrcamentos] = Todos,
        [EditarOrcamentos] = Todos,
        [AlterarStatusOrcamentos] = Todos,
        [ExcluirOrcamentos] = SomenteAdministrador,

        [GerenciarUsuarios] = SomenteAdministrador,

        [ConsultarProfissionais] = Todos,
        [GerenciarProfissionais] = SomenteAdministrador,

        [AlterarPropriaSenha] = Todos
    };

    public static void Registrar(AuthorizationOptions options)
    {
        // Toda rota exige usuário autenticado, salvo as marcadas com [AllowAnonymous].
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        foreach (var (politica, perfis) in Matriz)
            options.AddPolicy(politica, p => p.RequireRole(perfis.Select(perfil => perfil.ToString())));
    }

    /// <summary>Usado pelas views para ocultar menus e botões não permitidos ao perfil.</summary>
    public static async Task<bool> PermiteAsync(
        this IAuthorizationService autorizacao, ClaimsPrincipal usuario, string politica) =>
        (await autorizacao.AuthorizeAsync(usuario, politica)).Succeeded;
}

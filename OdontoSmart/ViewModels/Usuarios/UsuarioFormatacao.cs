using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Usuarios;

/// <summary>
/// Formatação de apresentação dos usuários (perfil e situação com destaque visual).
/// </summary>
public static class UsuarioFormatacao
{
    public static string Perfil(PerfilUsuario perfil) => perfil.Descricao();

    /// <summary>Classe Bootstrap que destaca visualmente cada perfil.</summary>
    public static string ClassePerfil(PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Administrador => "text-bg-dark",
        PerfilUsuario.Dentista => "text-bg-info",
        PerfilUsuario.Recepcao => "text-bg-light border",
        _ => "text-bg-light"
    };

    public static string Situacao(bool ativo, bool bloqueado) =>
        !ativo ? "Inativo" : bloqueado ? "Bloqueado" : "Ativo";

    public static string ClasseSituacao(bool ativo, bool bloqueado) =>
        !ativo ? "text-bg-secondary" : bloqueado ? "text-bg-danger" : "text-bg-success";

    public static string UltimoAcesso(DateTime? ultimoAcessoUtc) =>
        ultimoAcessoUtc is null ? "Nunca acessou" : PacienteFormatacao.DataHora(ultimoAcessoUtc);

    public static IEnumerable<SelectListItem> OpcoesPerfil =>
        Enum.GetValues<PerfilUsuario>().Select(p => new SelectListItem(Perfil(p), p.ToString()));

    public static IEnumerable<SelectListItem> OpcoesSituacao(FiltroSituacaoUsuario selecionada) =>
    [
        new("Ativos", FiltroSituacaoUsuario.Ativos.ToString(), selecionada == FiltroSituacaoUsuario.Ativos),
        new("Inativos", FiltroSituacaoUsuario.Inativos.ToString(), selecionada == FiltroSituacaoUsuario.Inativos),
        new("Bloqueados", FiltroSituacaoUsuario.Bloqueados.ToString(), selecionada == FiltroSituacaoUsuario.Bloqueados),
        new("Todos", FiltroSituacaoUsuario.Todos.ToString(), selecionada == FiltroSituacaoUsuario.Todos)
    ];
}

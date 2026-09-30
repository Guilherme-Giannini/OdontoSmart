using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Domain.Common;

namespace OdontoSmart.Web.ViewModels.Profissionais;

public static class ProfissionalFormatacao
{
    /// <summary>Ex.: "CRO-SP 12345".</summary>
    public static string Cro(string cro, string croUf) => $"CRO-{croUf} {cro}";

    public static string Situacao(bool ativo) => ativo ? "Ativo" : "Inativo";

    public static string ClasseSituacao(bool ativo) => ativo ? "text-bg-success" : "text-bg-secondary";

    public static IEnumerable<SelectListItem> OpcoesUf =>
        UnidadeFederativa.Siglas.Select(uf => new SelectListItem(uf, uf));
}

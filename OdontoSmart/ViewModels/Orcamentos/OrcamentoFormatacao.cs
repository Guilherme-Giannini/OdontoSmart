using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

/// <summary>
/// Formatação de apresentação dos orçamentos (valores monetários no formato brasileiro).
/// </summary>
public static class OrcamentoFormatacao
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

    public static string Moeda(decimal valor) => valor.ToString("C", Cultura);

    public static string Quantidade(decimal quantidade) => quantidade.ToString("0.##", Cultura);

    public static string Data(DateTime? data) => PacienteFormatacao.Data(data);

    public static string DataHora(DateTime? dataUtc) => PacienteFormatacao.DataHora(dataUtc);

    public static string Status(StatusOrcamento status) => status.Descricao();

    /// <summary>Classe Bootstrap que destaca visualmente cada status.</summary>
    public static string ClasseStatus(StatusOrcamento status) => status switch
    {
        StatusOrcamento.Aberto => "text-bg-primary",
        StatusOrcamento.AguardandoDecisao => "text-bg-warning",
        StatusOrcamento.Aprovado => "text-bg-success",
        StatusOrcamento.Recusado => "text-bg-danger",
        StatusOrcamento.Expirado => "text-bg-secondary",
        StatusOrcamento.Cancelado => "text-bg-dark",
        _ => "text-bg-light"
    };

    public static IEnumerable<SelectListItem> OpcoesStatus(IEnumerable<StatusOrcamento> status) =>
        status.Select(s => new SelectListItem(Status(s), s.ToString()));

    public static IEnumerable<SelectListItem> TodosOsStatus => OpcoesStatus(Enum.GetValues<StatusOrcamento>());
}

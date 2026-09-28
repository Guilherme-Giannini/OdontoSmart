using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Pacientes;

/// <summary>
/// Formatação de apresentação dos dados do paciente. Os valores são armazenados sem máscara.
/// </summary>
public static class PacienteFormatacao
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

    public static IEnumerable<SelectListItem> OpcoesSexo =>
        Enum.GetValues<Sexo>().Select(s => new SelectListItem(DescricaoSexo(s), s.ToString()));

    public static string Cpf(string? cpf) =>
        cpf is { Length: 11 }
            ? $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}"
            : cpf ?? string.Empty;

    public static string Telefone(string? telefone) => telefone switch
    {
        { Length: 11 } => $"({telefone[..2]}) {telefone[2..7]}-{telefone[7..]}",
        { Length: 10 } => $"({telefone[..2]}) {telefone[2..6]}-{telefone[6..]}",
        _ => telefone ?? string.Empty
    };

    public static string DescricaoSexo(Sexo? sexo) => sexo switch
    {
        Sexo.Masculino => "Masculino",
        Sexo.Feminino => "Feminino",
        Sexo.Outro => "Outro",
        _ => string.Empty
    };

    public static string Data(DateTime? data) =>
        data?.ToString("dd/MM/yyyy", Cultura) ?? string.Empty;

    /// <summary>Converte a data/hora armazenada em UTC para o horário local do servidor.</summary>
    public static string DataHora(DateTime? dataUtc) =>
        dataUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm", Cultura) ?? string.Empty;
}

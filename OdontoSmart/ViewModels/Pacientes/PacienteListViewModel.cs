using OdontoSmart.Application.Common;
using OdontoSmart.Application.Pacientes;

namespace OdontoSmart.Web.ViewModels.Pacientes;

public class PacienteListViewModel
{
    public IReadOnlyList<PacienteListItemViewModel> Pacientes { get; init; } = [];
    public string? Busca { get; init; }
    public int PaginaAtual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }

    public static PacienteListViewModel De(PaginaResultado<PacienteDto> pagina, string? busca) => new()
    {
        Pacientes = pagina.Itens.Select(PacienteListItemViewModel.De).ToList(),
        Busca = busca?.Trim(),
        PaginaAtual = pagina.Pagina,
        TotalPaginas = pagina.TotalPaginas,
        TotalRegistros = pagina.TotalRegistros
    };
}

public class PacienteListItemViewModel
{
    public Guid Id { get; init; }
    public string NomeCompleto { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string DataCadastro { get; init; } = string.Empty;

    public static PacienteListItemViewModel De(PacienteDto paciente) => new()
    {
        Id = paciente.Id,
        NomeCompleto = paciente.NomeCompleto,
        Cpf = PacienteFormatacao.Cpf(paciente.Cpf),
        Telefone = PacienteFormatacao.Telefone(paciente.Telefone),
        Email = paciente.Email ?? string.Empty,
        DataCadastro = PacienteFormatacao.Data(paciente.DataCadastro.ToLocalTime())
    };
}

using OdontoSmart.Application.Common;
using OdontoSmart.Application.Profissionais;

namespace OdontoSmart.Web.ViewModels.Profissionais;

public class ProfissionalListViewModel
{
    public IReadOnlyList<ProfissionalListItemViewModel> Profissionais { get; init; } = [];
    public string? Busca { get; init; }
    public int PaginaAtual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }

    public static ProfissionalListViewModel De(PaginaResultado<ProfissionalResumoDto> pagina, string? busca) => new()
    {
        Profissionais = pagina.Itens.Select(ProfissionalListItemViewModel.De).ToList(),
        Busca = busca?.Trim(),
        PaginaAtual = pagina.Pagina,
        TotalPaginas = pagina.TotalPaginas,
        TotalRegistros = pagina.TotalRegistros
    };
}

public class ProfissionalListItemViewModel
{
    public Guid Id { get; init; }
    public string NomeExibicao { get; init; } = string.Empty;
    public string Cro { get; init; } = string.Empty;
    public string Especialidade { get; init; } = string.Empty;
    public string UsuarioVinculado { get; init; } = string.Empty;
    public bool Ativo { get; init; }

    public static ProfissionalListItemViewModel De(ProfissionalResumoDto profissional) => new()
    {
        Id = profissional.Id,
        NomeExibicao = profissional.NomeExibicao,
        Cro = ProfissionalFormatacao.Cro(profissional.Cro, profissional.CroUf),
        Especialidade = profissional.Especialidade ?? string.Empty,
        UsuarioVinculado = profissional.UsuarioNome ?? string.Empty,
        Ativo = profissional.Ativo
    };
}

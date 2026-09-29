using System.Globalization;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

public class OrcamentoListViewModel
{
    public IReadOnlyList<OrcamentoListItemViewModel> Orcamentos { get; init; } = [];

    // Filtros aplicados.
    public string? Busca { get; init; }
    public StatusOrcamento? Status { get; init; }
    public DateTime? DataInicio { get; init; }
    public DateTime? DataFim { get; init; }

    public int PaginaAtual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }

    public bool PossuiFiltros =>
        !string.IsNullOrEmpty(Busca) || Status is not null || DataInicio is not null || DataFim is not null;

    /// <summary>Parâmetros de rota que preservam os filtros ao navegar entre páginas.</summary>
    public Dictionary<string, string> RotaDaPagina(int pagina)
    {
        var rota = new Dictionary<string, string> { ["pagina"] = pagina.ToString(CultureInfo.InvariantCulture) };

        if (!string.IsNullOrEmpty(Busca))
            rota["busca"] = Busca;
        if (Status is not null)
            rota["status"] = Status.Value.ToString();
        if (DataInicio is not null)
            rota["dataInicio"] = DataInicio.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (DataFim is not null)
            rota["dataFim"] = DataFim.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        return rota;
    }

    public static OrcamentoListViewModel De(PaginaResultado<OrcamentoResumoDto> pagina, FiltroOrcamentos filtro) => new()
    {
        Orcamentos = pagina.Itens.Select(OrcamentoListItemViewModel.De).ToList(),
        Busca = filtro.Busca?.Trim(),
        Status = filtro.Status,
        DataInicio = filtro.DataInicio,
        DataFim = filtro.DataFim,
        PaginaAtual = pagina.Pagina,
        TotalPaginas = pagina.TotalPaginas,
        TotalRegistros = pagina.TotalRegistros
    };
}

public class OrcamentoListItemViewModel
{
    public Guid Id { get; init; }
    public string PacienteNome { get; init; } = string.Empty;
    public string PacienteCpf { get; init; } = string.Empty;
    public string DataCadastro { get; init; } = string.Empty;
    public string Validade { get; init; } = string.Empty;
    public StatusOrcamento Status { get; init; }
    public int QuantidadeItens { get; init; }
    public string ValorTotal { get; init; } = string.Empty;

    public static OrcamentoListItemViewModel De(OrcamentoResumoDto orcamento) => new()
    {
        Id = orcamento.Id,
        PacienteNome = orcamento.PacienteNome,
        PacienteCpf = PacienteFormatacao.Cpf(orcamento.PacienteCpf),
        DataCadastro = OrcamentoFormatacao.Data(orcamento.DataCadastro.ToLocalTime()),
        Validade = OrcamentoFormatacao.Data(orcamento.Validade),
        Status = orcamento.Status,
        QuantidadeItens = orcamento.QuantidadeItens,
        ValorTotal = OrcamentoFormatacao.Moeda(orcamento.ValorTotal)
    };
}

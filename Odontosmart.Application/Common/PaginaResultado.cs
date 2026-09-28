namespace OdontoSmart.Application.Common;

public sealed record PaginaResultado<T>(IReadOnlyList<T> Itens, int TotalRegistros, int Pagina, int TamanhoPagina)
{
    public int TotalPaginas => TotalRegistros == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
}

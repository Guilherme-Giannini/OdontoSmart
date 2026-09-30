namespace OdontoSmart.Domain.Common;

public static class UnidadeFederativa
{
    public const int Tamanho = 2;

    /// <summary>Siglas das 27 unidades federativas brasileiras, em ordem alfabética.</summary>
    public static IReadOnlyList<string> Siglas { get; } =
    [
        "AC", "AL", "AM", "AP", "BA", "CE", "DF", "ES", "GO", "MA", "MG", "MS", "MT", "PA",
        "PB", "PE", "PI", "PR", "RJ", "RN", "RO", "RR", "RS", "SC", "SE", "SP", "TO"
    ];

    /// <summary>
    /// Retorna a sigla em maiúsculas e sem espaços, ou null quando não informada.
    /// </summary>
    public static string? Normalizar(string? uf) =>
        string.IsNullOrWhiteSpace(uf) ? null : uf.Trim().ToUpperInvariant();

    public static bool EhValida(string? uf) => Normalizar(uf) is { } sigla && Siglas.Contains(sigla);
}

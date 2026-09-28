namespace OdontoSmart.Domain.Common;

public static class Digitos
{
    /// <summary>
    /// Remove qualquer caractere que não seja dígito (máscaras, espaços, pontuação).
    /// </summary>
    public static string Extrair(string? valor) =>
        string.IsNullOrEmpty(valor) ? string.Empty : new string(valor.Where(char.IsAsciiDigit).ToArray());
}

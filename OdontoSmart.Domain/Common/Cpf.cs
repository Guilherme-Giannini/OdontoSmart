namespace OdontoSmart.Domain.Common;

public static class Cpf
{
    public const int Tamanho = 11;

    /// <summary>
    /// Retorna o CPF somente com dígitos, ou null quando não informado.
    /// </summary>
    public static string? Normalizar(string? cpf)
    {
        var digitos = Digitos.Extrair(cpf);
        return digitos.Length == 0 ? null : digitos;
    }

    /// <summary>
    /// Valida os dígitos verificadores do CPF. Aceita o valor com ou sem máscara.
    /// </summary>
    public static bool EhValido(string? cpf)
    {
        var digitos = Digitos.Extrair(cpf);

        if (digitos.Length != Tamanho || digitos.Distinct().Count() == 1)
            return false;

        return digitos[9] - '0' == CalcularDigito(digitos, 9)
            && digitos[10] - '0' == CalcularDigito(digitos, 10);
    }

    private static int CalcularDigito(string digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += (digitos[i] - '0') * (quantidade + 1 - i);

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}

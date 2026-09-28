using System.Net.Mail;

namespace OdontoSmart.Domain.Common;

public static class Email
{
    public const int TamanhoMaximo = 254;

    public static bool EhValido(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > TamanhoMaximo)
            return false;

        var valor = email.Trim();

        // MailAddress aceita formatos como "Nome <a@b.com>"; exige-se apenas o endereço,
        // com domínio contendo ao menos um ponto.
        return MailAddress.TryCreate(valor, out var endereco)
            && endereco.Address == valor
            && endereco.Host.Contains('.');
    }
}

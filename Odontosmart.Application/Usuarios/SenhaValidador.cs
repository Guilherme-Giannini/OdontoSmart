using OdontoSmart.Application.Common;

namespace OdontoSmart.Application.Usuarios;

public static class SenhaValidador
{
    public const int TamanhoMinimo = 8;
    public const int TamanhoMaximo = 128;

    /// <summary>
    /// RN005 — de 8 a 128 caracteres, com pelo menos uma letra e um número; a confirmação deve ser igual.
    /// </summary>
    public static List<ErroValidacao> Validar(
        string? senha, string? confirmacao, string campoSenha, string campoConfirmacao)
    {
        var erros = new List<ErroValidacao>();

        if (string.IsNullOrEmpty(senha))
        {
            erros.Add(new(campoSenha, "Informe a senha."));
            return erros;
        }

        if (senha.Length is < TamanhoMinimo or > TamanhoMaximo)
            erros.Add(new(campoSenha, $"A senha deve possuir entre {TamanhoMinimo} e {TamanhoMaximo} caracteres."));
        else if (!senha.Any(char.IsLetter) || !senha.Any(char.IsDigit))
            erros.Add(new(campoSenha, "A senha deve conter pelo menos uma letra e um número."));

        if (confirmacao != senha)
            erros.Add(new(campoConfirmacao, "A confirmação não confere com a senha informada."));

        return erros;
    }
}

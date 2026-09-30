using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Common;

namespace OdontoSmart.Application.Usuarios;

public static class UsuarioValidador
{
    public const int NomeCompletoTamanhoMinimo = 3;
    public const int NomeCompletoTamanhoMaximo = 200;

    public static List<ErroValidacao> Validar(UsuarioDados dados)
    {
        var erros = new List<ErroValidacao>();

        var nome = dados.NomeCompleto?.Trim() ?? string.Empty;
        if (nome.Length == 0)
            erros.Add(new(nameof(UsuarioDados.NomeCompleto), "Informe o nome completo do usuário."));
        else if (nome.Length is < NomeCompletoTamanhoMinimo or > NomeCompletoTamanhoMaximo)
            erros.Add(new(nameof(UsuarioDados.NomeCompleto),
                $"O nome completo deve possuir entre {NomeCompletoTamanhoMinimo} e {NomeCompletoTamanhoMaximo} caracteres."));

        if (string.IsNullOrWhiteSpace(dados.Email))
            erros.Add(new(nameof(UsuarioDados.Email), "Informe o e-mail do usuário."));
        else if (!Email.EhValido(dados.Email))
            erros.Add(new(nameof(UsuarioDados.Email), "Informe um e-mail válido."));

        if (dados.Perfil is null)
            erros.Add(new(nameof(UsuarioDados.Perfil), "Selecione o perfil do usuário."));
        else if (!Enum.IsDefined(dados.Perfil.Value))
            erros.Add(new(nameof(UsuarioDados.Perfil), "Perfil inválido."));

        return erros;
    }
}

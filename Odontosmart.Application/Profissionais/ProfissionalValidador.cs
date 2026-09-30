using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Profissionais;

public static class ProfissionalValidador
{
    public static List<ErroValidacao> Validar(ProfissionalDados dados)
    {
        var erros = new List<ErroValidacao>();

        var nome = dados.NomeExibicao?.Trim() ?? string.Empty;
        if (nome.Length == 0)
            erros.Add(new(nameof(ProfissionalDados.NomeExibicao), "Informe o nome de exibição do profissional."));
        else if (nome.Length is < Profissional.NomeExibicaoTamanhoMinimo or > Profissional.NomeExibicaoTamanhoMaximo)
            erros.Add(new(nameof(ProfissionalDados.NomeExibicao),
                $"O nome de exibição deve possuir entre {Profissional.NomeExibicaoTamanhoMinimo} e {Profissional.NomeExibicaoTamanhoMaximo} caracteres."));

        if (string.IsNullOrWhiteSpace(dados.Cro))
            erros.Add(new(nameof(ProfissionalDados.Cro), "Informe o número do CRO."));
        else if (!Profissional.CroEhValido(dados.Cro))
            erros.Add(new(nameof(ProfissionalDados.Cro),
                $"O CRO deve conter somente dígitos (de 1 a {Profissional.CroTamanhoMaximo})."));

        if (string.IsNullOrWhiteSpace(dados.CroUf))
            erros.Add(new(nameof(ProfissionalDados.CroUf), "Selecione a UF do CRO."));
        else if (!UnidadeFederativa.EhValida(dados.CroUf))
            erros.Add(new(nameof(ProfissionalDados.CroUf), "UF do CRO inválida."));

        if (dados.Especialidade?.Trim().Length > Profissional.EspecialidadeTamanhoMaximo)
            erros.Add(new(nameof(ProfissionalDados.Especialidade),
                $"A especialidade deve possuir no máximo {Profissional.EspecialidadeTamanhoMaximo} caracteres."));

        if (!string.IsNullOrWhiteSpace(dados.Telefone))
        {
            var telefone = Digitos.Extrair(dados.Telefone);
            if (telefone.Length is < Profissional.TelefoneTamanhoMinimo or > Profissional.TelefoneTamanhoMaximo)
                erros.Add(new(nameof(ProfissionalDados.Telefone), "Informe um telefone válido com DDD (10 ou 11 dígitos)."));
        }

        if (!string.IsNullOrWhiteSpace(dados.Email) && !Email.EhValido(dados.Email))
            erros.Add(new(nameof(ProfissionalDados.Email), "Informe um e-mail válido."));

        return erros;
    }
}

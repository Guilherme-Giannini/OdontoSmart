using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Pacientes;

public static class PacienteValidador
{
    public static List<ErroValidacao> Validar(PacienteDados dados)
    {
        var erros = new List<ErroValidacao>();

        var nome = dados.NomeCompleto?.Trim() ?? string.Empty;
        if (nome.Length == 0)
            erros.Add(new(nameof(PacienteDados.NomeCompleto), "Informe o nome completo do paciente."));
        else if (nome.Length is < Paciente.NomeCompletoTamanhoMinimo or > Paciente.NomeCompletoTamanhoMaximo)
            erros.Add(new(nameof(PacienteDados.NomeCompleto),
                $"O nome completo deve possuir entre {Paciente.NomeCompletoTamanhoMinimo} e {Paciente.NomeCompletoTamanhoMaximo} caracteres."));

        if (!string.IsNullOrWhiteSpace(dados.Cpf) && !Cpf.EhValido(dados.Cpf))
            erros.Add(new(nameof(PacienteDados.Cpf), "CPF inválido."));

        if (dados.DataNascimento is not null && dados.DataNascimento.Value.Date > DateTime.Today)
            erros.Add(new(nameof(PacienteDados.DataNascimento), "A data de nascimento não pode ser uma data futura."));

        if (dados.Sexo is not null && !Enum.IsDefined(dados.Sexo.Value))
            erros.Add(new(nameof(PacienteDados.Sexo), "Sexo inválido."));

        if (!string.IsNullOrWhiteSpace(dados.Telefone))
        {
            var telefone = Digitos.Extrair(dados.Telefone);
            if (telefone.Length is < Paciente.TelefoneTamanhoMinimo or > Paciente.TelefoneTamanhoMaximo)
                erros.Add(new(nameof(PacienteDados.Telefone), "Informe um telefone válido com DDD (10 ou 11 dígitos)."));
        }

        if (!string.IsNullOrWhiteSpace(dados.Email) && !Email.EhValido(dados.Email))
            erros.Add(new(nameof(PacienteDados.Email), "Informe um e-mail válido."));

        if (dados.Observacoes?.Trim().Length > Paciente.ObservacoesTamanhoMaximo)
            erros.Add(new(nameof(PacienteDados.Observacoes),
                $"As observações devem possuir no máximo {Paciente.ObservacoesTamanhoMaximo} caracteres."));

        return erros;
    }
}

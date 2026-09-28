using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Domain.Entities;

public class Paciente
{
    public const int NomeCompletoTamanhoMinimo = 3;
    public const int NomeCompletoTamanhoMaximo = 200;
    public const int TelefoneTamanhoMinimo = 10;
    public const int TelefoneTamanhoMaximo = 11;
    public const int ObservacoesTamanhoMaximo = 2000;

    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;

    /// <summary>CPF somente com dígitos.</summary>
    public string? Cpf { get; private set; }

    public DateTime? DataNascimento { get; private set; }
    public Sexo? Sexo { get; private set; }

    /// <summary>Telefone somente com dígitos (DDD + número).</summary>
    public string? Telefone { get; private set; }

    public string? Email { get; private set; }
    public string? Observacoes { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    // Utilizado pelo EF Core.
    private Paciente() { }

    public static Paciente Criar(
        string nomeCompleto,
        string? cpf,
        DateTime? dataNascimento,
        Sexo? sexo,
        string? telefone,
        string? email,
        string? observacoes)
    {
        var paciente = new Paciente
        {
            Id = Guid.NewGuid(),
            DataCadastro = DateTime.UtcNow
        };

        paciente.DefinirDados(nomeCompleto, cpf, dataNascimento, sexo, telefone, email, observacoes);
        return paciente;
    }

    public void Atualizar(
        string nomeCompleto,
        string? cpf,
        DateTime? dataNascimento,
        Sexo? sexo,
        string? telefone,
        string? email,
        string? observacoes)
    {
        DefinirDados(nomeCompleto, cpf, dataNascimento, sexo, telefone, email, observacoes);
        DataAtualizacao = DateTime.UtcNow;
    }

    private void DefinirDados(
        string nomeCompleto,
        string? cpf,
        DateTime? dataNascimento,
        Sexo? sexo,
        string? telefone,
        string? email,
        string? observacoes)
    {
        var nome = nomeCompleto?.Trim() ?? string.Empty;
        if (nome.Length is < NomeCompletoTamanhoMinimo or > NomeCompletoTamanhoMaximo)
            throw new DomainException(
                $"O nome completo deve possuir entre {NomeCompletoTamanhoMinimo} e {NomeCompletoTamanhoMaximo} caracteres.");

        var cpfNormalizado = Common.Cpf.Normalizar(cpf);
        if (cpfNormalizado is not null && !Common.Cpf.EhValido(cpfNormalizado))
            throw new DomainException("CPF inválido.");

        if (dataNascimento is not null && dataNascimento.Value.Date > DateTime.Today)
            throw new DomainException("A data de nascimento não pode ser uma data futura.");

        if (sexo is not null && !Enum.IsDefined(sexo.Value))
            throw new DomainException("Sexo inválido.");

        var telefoneNormalizado = Digitos.Extrair(telefone);
        if (telefoneNormalizado.Length > 0 &&
            telefoneNormalizado.Length is < TelefoneTamanhoMinimo or > TelefoneTamanhoMaximo)
            throw new DomainException("Telefone inválido.");

        var emailNormalizado = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        if (emailNormalizado is not null && !Common.Email.EhValido(emailNormalizado))
            throw new DomainException("E-mail inválido.");

        var observacoesNormalizadas = string.IsNullOrWhiteSpace(observacoes) ? null : observacoes.Trim();
        if (observacoesNormalizadas?.Length > ObservacoesTamanhoMaximo)
            throw new DomainException(
                $"As observações devem possuir no máximo {ObservacoesTamanhoMaximo} caracteres.");

        NomeCompleto = nome;
        Cpf = cpfNormalizado;
        DataNascimento = dataNascimento?.Date;
        Sexo = sexo;
        Telefone = telefoneNormalizado.Length == 0 ? null : telefoneNormalizado;
        Email = emailNormalizado;
        Observacoes = observacoesNormalizadas;
    }
}

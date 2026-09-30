using OdontoSmart.Domain.Common;

namespace OdontoSmart.Domain.Entities;

/// <summary>
/// Profissional (dentista) que atua na clínica. Pode existir sem acesso ao sistema;
/// quando possui acesso, é vinculado a um usuário de perfil Dentista.
/// </summary>
public class Profissional
{
    public const int NomeExibicaoTamanhoMinimo = 3;
    public const int NomeExibicaoTamanhoMaximo = 200;
    public const int CroTamanhoMaximo = 10;
    public const int EspecialidadeTamanhoMaximo = 100;
    public const int TelefoneTamanhoMinimo = 10;
    public const int TelefoneTamanhoMaximo = 11;

    public Guid Id { get; private set; }
    public string NomeExibicao { get; private set; } = string.Empty;

    /// <summary>Número do CRO, somente dígitos.</summary>
    public string Cro { get; private set; } = string.Empty;

    /// <summary>Sigla da UF do CRO, em maiúsculas.</summary>
    public string CroUf { get; private set; } = string.Empty;

    public string? Especialidade { get; private set; }

    /// <summary>Telefone somente com dígitos (DDD + número).</summary>
    public string? Telefone { get; private set; }

    public string? Email { get; private set; }
    public Guid? UsuarioId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }

    // Utilizado pelo EF Core.
    private Profissional() { }

    public static Profissional Criar(
        string nomeExibicao,
        string cro,
        string croUf,
        string? especialidade,
        string? telefone,
        string? email,
        Guid? usuarioId)
    {
        var profissional = new Profissional
        {
            Id = Guid.NewGuid(),
            Ativo = true,
            DataCadastro = DateTime.UtcNow
        };

        profissional.DefinirDados(nomeExibicao, cro, croUf, especialidade, telefone, email, usuarioId);
        return profissional;
    }

    public void Atualizar(
        string nomeExibicao,
        string cro,
        string croUf,
        string? especialidade,
        string? telefone,
        string? email,
        Guid? usuarioId)
    {
        DefinirDados(nomeExibicao, cro, croUf, especialidade, telefone, email, usuarioId);
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        if (Ativo)
            throw new DomainException("O profissional já está ativo.");

        Ativo = true;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        if (!Ativo)
            throw new DomainException("O profissional já está inativo.");

        Ativo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public static string? NormalizarCro(string? cro) => string.IsNullOrWhiteSpace(cro) ? null : cro.Trim();

    public static bool CroEhValido(string? cro) =>
        NormalizarCro(cro) is { Length: > 0 and <= CroTamanhoMaximo } valor && valor.All(char.IsAsciiDigit);

    private void DefinirDados(
        string nomeExibicao,
        string cro,
        string croUf,
        string? especialidade,
        string? telefone,
        string? email,
        Guid? usuarioId)
    {
        var nome = nomeExibicao?.Trim() ?? string.Empty;
        if (nome.Length is < NomeExibicaoTamanhoMinimo or > NomeExibicaoTamanhoMaximo)
            throw new DomainException(
                $"O nome de exibição deve possuir entre {NomeExibicaoTamanhoMinimo} e {NomeExibicaoTamanhoMaximo} caracteres.");

        if (!CroEhValido(cro))
            throw new DomainException($"O CRO deve conter somente dígitos (de 1 a {CroTamanhoMaximo}).");

        if (!UnidadeFederativa.EhValida(croUf))
            throw new DomainException("UF do CRO inválida.");

        var especialidadeNormalizada = string.IsNullOrWhiteSpace(especialidade) ? null : especialidade.Trim();
        if (especialidadeNormalizada?.Length > EspecialidadeTamanhoMaximo)
            throw new DomainException(
                $"A especialidade deve possuir no máximo {EspecialidadeTamanhoMaximo} caracteres.");

        var telefoneNormalizado = Digitos.Extrair(telefone);
        if (telefoneNormalizado.Length > 0 &&
            telefoneNormalizado.Length is < TelefoneTamanhoMinimo or > TelefoneTamanhoMaximo)
            throw new DomainException("Telefone inválido.");

        var emailNormalizado = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        if (emailNormalizado is not null && !Common.Email.EhValido(emailNormalizado))
            throw new DomainException("E-mail inválido.");

        if (usuarioId == Guid.Empty)
            throw new DomainException("Usuário vinculado inválido.");

        NomeExibicao = nome;
        Cro = NormalizarCro(cro)!;
        CroUf = UnidadeFederativa.Normalizar(croUf)!;
        Especialidade = especialidadeNormalizada;
        Telefone = telefoneNormalizado.Length == 0 ? null : telefoneNormalizado;
        Email = emailNormalizado;
        UsuarioId = usuarioId;
    }
}

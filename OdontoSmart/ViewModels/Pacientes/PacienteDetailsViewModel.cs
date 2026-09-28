using OdontoSmart.Application.Pacientes;

namespace OdontoSmart.Web.ViewModels.Pacientes;

/// <summary>
/// Dados de exibição de um paciente (tela de detalhes e confirmação de exclusão).
/// </summary>
public class PacienteDetailsViewModel
{
    public Guid Id { get; init; }
    public string NomeCompleto { get; init; } = string.Empty;
    public string Cpf { get; init; } = string.Empty;
    public string DataNascimento { get; init; } = string.Empty;
    public string Sexo { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Observacoes { get; init; } = string.Empty;
    public string DataCadastro { get; init; } = string.Empty;
    public string DataAtualizacao { get; init; } = string.Empty;

    public static PacienteDetailsViewModel De(PacienteDto paciente) => new()
    {
        Id = paciente.Id,
        NomeCompleto = paciente.NomeCompleto,
        Cpf = PacienteFormatacao.Cpf(paciente.Cpf),
        DataNascimento = PacienteFormatacao.Data(paciente.DataNascimento),
        Sexo = PacienteFormatacao.DescricaoSexo(paciente.Sexo),
        Telefone = PacienteFormatacao.Telefone(paciente.Telefone),
        Email = paciente.Email ?? string.Empty,
        Observacoes = paciente.Observacoes ?? string.Empty,
        DataCadastro = PacienteFormatacao.DataHora(paciente.DataCadastro),
        DataAtualizacao = PacienteFormatacao.DataHora(paciente.DataAtualizacao)
    };
}

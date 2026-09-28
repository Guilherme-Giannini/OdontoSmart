using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Pacientes;

/// <summary>
/// Dados editáveis de um paciente, informados pelo usuário no cadastro ou na edição.
/// </summary>
public sealed record PacienteDados(
    string NomeCompleto,
    string? Cpf,
    DateTime? DataNascimento,
    Sexo? Sexo,
    string? Telefone,
    string? Email,
    string? Observacoes);

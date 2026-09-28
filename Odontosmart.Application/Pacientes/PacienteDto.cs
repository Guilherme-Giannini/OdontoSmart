using OdontoSmart.Domain.Entities;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Pacientes;

public sealed record PacienteDto(
    Guid Id,
    string NomeCompleto,
    string? Cpf,
    DateTime? DataNascimento,
    Sexo? Sexo,
    string? Telefone,
    string? Email,
    string? Observacoes,
    DateTime DataCadastro,
    DateTime? DataAtualizacao)
{
    public static PacienteDto De(Paciente paciente) => new(
        paciente.Id,
        paciente.NomeCompleto,
        paciente.Cpf,
        paciente.DataNascimento,
        paciente.Sexo,
        paciente.Telefone,
        paciente.Email,
        paciente.Observacoes,
        paciente.DataCadastro,
        paciente.DataAtualizacao);
}

namespace OdontoSmart.Application.Pacientes;

/// <summary>Dados mínimos de um paciente para seleção em formulários.</summary>
public sealed record PacienteOpcaoDto(Guid Id, string NomeCompleto, string? Cpf);

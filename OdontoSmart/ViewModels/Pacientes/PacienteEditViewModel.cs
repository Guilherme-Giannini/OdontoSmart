using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Application.Pacientes;

namespace OdontoSmart.Web.ViewModels.Pacientes;

public class PacienteEditViewModel : PacienteFormViewModel
{
    /// <summary>Definido pelo controller a partir da rota; nunca lido do formulário.</summary>
    [BindNever]
    public Guid Id { get; set; }

    public static PacienteEditViewModel De(PacienteDto paciente) => new()
    {
        Id = paciente.Id,
        NomeCompleto = paciente.NomeCompleto,
        Cpf = PacienteFormatacao.Cpf(paciente.Cpf),
        DataNascimento = paciente.DataNascimento,
        Sexo = paciente.Sexo,
        Telefone = PacienteFormatacao.Telefone(paciente.Telefone),
        Email = paciente.Email,
        Observacoes = paciente.Observacoes
    };
}

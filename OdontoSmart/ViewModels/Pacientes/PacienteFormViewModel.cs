using System.ComponentModel.DataAnnotations;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Pacientes;

/// <summary>
/// Campos editáveis do formulário de paciente, compartilhados pelo cadastro e pela edição.
/// As anotações servem à validação no navegador; as regras de negócio são validadas na camada Application.
/// </summary>
public abstract class PacienteFormViewModel
{
    [Display(Name = "Nome completo")]
    [Required(ErrorMessage = "Informe o nome completo do paciente.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome completo deve possuir entre {2} e {1} caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Display(Name = "CPF")]
    [StringLength(14, ErrorMessage = "CPF inválido.")]
    public string? Cpf { get; set; }

    [Display(Name = "Data de nascimento")]
    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

    [Display(Name = "Sexo")]
    public Sexo? Sexo { get; set; }

    [Display(Name = "Telefone")]
    [StringLength(20, ErrorMessage = "Informe um telefone válido com DDD (10 ou 11 dígitos).")]
    public string? Telefone { get; set; }

    [Display(Name = "E-mail")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve possuir no máximo {1} caracteres.")]
    public string? Email { get; set; }

    [Display(Name = "Observações")]
    [StringLength(2000, ErrorMessage = "As observações devem possuir no máximo {1} caracteres.")]
    public string? Observacoes { get; set; }

    public PacienteDados ParaDados() =>
        new(NomeCompleto, Cpf, DataNascimento, Sexo, Telefone, Email, Observacoes);
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Web.ViewModels.Profissionais;

/// <summary>
/// Campos editáveis do formulário de profissional, compartilhados pelo cadastro e pela edição.
/// As anotações servem à validação no navegador; as regras de negócio são validadas na camada Application.
/// </summary>
public abstract class ProfissionalFormViewModel
{
    [Display(Name = "Nome de exibição")]
    [Required(ErrorMessage = "Informe o nome de exibição do profissional.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome de exibição deve possuir entre {2} e {1} caracteres.")]
    public string? NomeExibicao { get; set; }

    [Display(Name = "CRO")]
    [Required(ErrorMessage = "Informe o número do CRO.")]
    [RegularExpression(@"^\s*\d{1,10}\s*$", ErrorMessage = "O CRO deve conter somente dígitos (de 1 a 10).")]
    public string? Cro { get; set; }

    [Display(Name = "UF do CRO")]
    [Required(ErrorMessage = "Selecione a UF do CRO.")]
    public string? CroUf { get; set; }

    [Display(Name = "Especialidade")]
    [StringLength(100, ErrorMessage = "A especialidade deve possuir no máximo {1} caracteres.")]
    public string? Especialidade { get; set; }

    [Display(Name = "Telefone")]
    [StringLength(20, ErrorMessage = "Informe um telefone válido com DDD (10 ou 11 dígitos).")]
    public string? Telefone { get; set; }

    [Display(Name = "E-mail")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve possuir no máximo {1} caracteres.")]
    public string? Email { get; set; }

    [Display(Name = "Usuário vinculado")]
    public Guid? UsuarioId { get; set; }

    [BindNever]
    public IEnumerable<SelectListItem> Usuarios { get; set; } = [];

    public void DefinirUsuarios(IEnumerable<UsuarioOpcaoDto> usuarios) =>
        Usuarios = usuarios.Select(u => new SelectListItem($"{u.NomeCompleto} ({u.Email})", u.Id.ToString()));

    public ProfissionalDados ParaDados() =>
        new(NomeExibicao, Cro, CroUf, Especialidade, Telefone, Email, UsuarioId);
}

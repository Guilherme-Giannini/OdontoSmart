using System.ComponentModel.DataAnnotations;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Usuarios;

/// <summary>
/// Campos editáveis do formulário de usuário, compartilhados pelo cadastro e pela edição.
/// As anotações servem à validação no navegador; as regras de negócio são validadas na camada Application.
/// </summary>
public abstract class UsuarioFormViewModel
{
    [Display(Name = "Nome completo")]
    [Required(ErrorMessage = "Informe o nome completo do usuário.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome completo deve possuir entre {2} e {1} caracteres.")]
    public string? NomeCompleto { get; set; }

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail do usuário.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve possuir no máximo {1} caracteres.")]
    public string? Email { get; set; }

    [Display(Name = "Perfil")]
    [Required(ErrorMessage = "Selecione o perfil do usuário.")]
    public PerfilUsuario? Perfil { get; set; }

    public UsuarioDados ParaDados() => new(NomeCompleto, Email, Perfil);
}

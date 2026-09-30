using System.ComponentModel.DataAnnotations;
using OdontoSmart.Application.PrimeiroAcesso;

namespace OdontoSmart.Web.ViewModels.PrimeiroAcesso;

public class PrimeiroAcessoViewModel
{
    [Display(Name = "Nome completo")]
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "O nome completo deve possuir entre {2} e {1} caracteres.")]
    public string? NomeCompleto { get; set; }

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve possuir no máximo {1} caracteres.")]
    public string? Email { get; set; }

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve possuir entre {2} e {1} caracteres.")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    [Display(Name = "Confirmação da senha")]
    [Required(ErrorMessage = "Confirme a senha.")]
    [Compare(nameof(Senha), ErrorMessage = "A confirmação não confere com a senha informada.")]
    [DataType(DataType.Password)]
    public string? ConfirmacaoSenha { get; set; }

    public PrimeiroAdministradorDados ParaDados() => new(NomeCompleto, Email, Senha, ConfirmacaoSenha);
}

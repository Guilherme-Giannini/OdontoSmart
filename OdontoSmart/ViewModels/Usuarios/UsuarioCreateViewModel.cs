using System.ComponentModel.DataAnnotations;

namespace OdontoSmart.Web.ViewModels.Usuarios;

public class UsuarioCreateViewModel : UsuarioFormViewModel
{
    /// <summary>Senha inicial; o usuário deverá trocá-la no primeiro acesso.</summary>
    [Display(Name = "Senha inicial")]
    [Required(ErrorMessage = "Informe a senha inicial.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve possuir entre {2} e {1} caracteres.")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    [Display(Name = "Confirmação da senha")]
    [Required(ErrorMessage = "Confirme a senha inicial.")]
    [Compare(nameof(Senha), ErrorMessage = "A confirmação não confere com a senha informada.")]
    [DataType(DataType.Password)]
    public string? ConfirmacaoSenha { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace OdontoSmart.Web.ViewModels.Conta;

public class LoginViewModel
{
    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail.")]
    public string? Email { get; set; }

    /// <summary>Nunca é reenviada ao navegador: campos de senha são renderizados vazios.</summary>
    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string? Senha { get; set; }

    /// <summary>Rota de origem; só é utilizada se for uma URL local (proteção contra open redirect).</summary>
    public string? ReturnUrl { get; set; }
}

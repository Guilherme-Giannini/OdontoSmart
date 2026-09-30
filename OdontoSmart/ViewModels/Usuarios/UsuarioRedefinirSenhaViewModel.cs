using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Web.ViewModels.Usuarios;

public class UsuarioRedefinirSenhaViewModel
{
    // Somente exibição: identificam o usuário cuja senha será redefinida.

    [BindNever]
    public Guid Id { get; set; }

    [BindNever]
    public string NomeCompleto { get; set; } = string.Empty;

    [BindNever]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Nova senha temporária")]
    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve possuir entre {2} e {1} caracteres.")]
    [DataType(DataType.Password)]
    public string? NovaSenha { get; set; }

    [Display(Name = "Confirmação da senha")]
    [Required(ErrorMessage = "Confirme a nova senha.")]
    [Compare(nameof(NovaSenha), ErrorMessage = "A confirmação não confere com a senha informada.")]
    [DataType(DataType.Password)]
    public string? ConfirmacaoNovaSenha { get; set; }

    public void DefinirUsuario(UsuarioDto usuario)
    {
        Id = usuario.Id;
        NomeCompleto = usuario.NomeCompleto;
        Email = usuario.Email;
    }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OdontoSmart.Web.ViewModels.Conta;

/// <summary>
/// Troca de senha pelo próprio usuário. As anotações servem à validação no navegador;
/// a regra de senha (RN005/RN010) é validada na camada Application.
/// </summary>
public class AlterarSenhaViewModel
{
    [Display(Name = "Senha atual")]
    [Required(ErrorMessage = "Informe a senha atual.")]
    [DataType(DataType.Password)]
    public string? SenhaAtual { get; set; }

    [Display(Name = "Nova senha")]
    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve possuir entre {2} e {1} caracteres.")]
    [DataType(DataType.Password)]
    public string? NovaSenha { get; set; }

    [Display(Name = "Confirmação da nova senha")]
    [Required(ErrorMessage = "Confirme a nova senha.")]
    [Compare(nameof(NovaSenha), ErrorMessage = "A confirmação não confere com a senha informada.")]
    [DataType(DataType.Password)]
    public string? ConfirmacaoNovaSenha { get; set; }

    /// <summary>Somente exibição: indica que a troca é obrigatória (primeiro acesso ou senha redefinida).</summary>
    [BindNever]
    public bool Obrigatoria { get; set; }
}

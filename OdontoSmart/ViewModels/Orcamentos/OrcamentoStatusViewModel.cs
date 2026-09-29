using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

/// <summary>
/// Formulário de alteração de status. Apenas <see cref="NovoStatus"/> é recebido do navegador;
/// a validade da transição é verificada no servidor.
/// </summary>
public class OrcamentoStatusViewModel
{
    [BindNever]
    public Guid Id { get; set; }

    [BindNever]
    public StatusOrcamento StatusAtual { get; set; }

    [Display(Name = "Novo status")]
    [Required(ErrorMessage = "Selecione o novo status.")]
    public StatusOrcamento? NovoStatus { get; set; }

    [BindNever]
    public IReadOnlyList<StatusOrcamento> TransicoesPermitidas { get; set; } = [];
}

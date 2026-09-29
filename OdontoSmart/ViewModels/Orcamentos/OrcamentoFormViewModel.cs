using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

/// <summary>
/// Campos editáveis do formulário de orçamento, compartilhados pelo cadastro e pela edição.
/// As anotações servem à validação no navegador; as regras de negócio são validadas na camada Application.
/// </summary>
public abstract class OrcamentoFormViewModel
{
    [Display(Name = "Paciente")]
    [Required(ErrorMessage = "Selecione o paciente do orçamento.")]
    public Guid? PacienteId { get; set; }

    [Display(Name = "Validade")]
    [DataType(DataType.Date)]
    public DateTime? Validade { get; set; }

    [Display(Name = "Observações")]
    [StringLength(2000, ErrorMessage = "As observações devem possuir no máximo {1} caracteres.")]
    public string? Observacoes { get; set; }

    public List<OrcamentoItemViewModel> Itens { get; set; } = [];

    [BindNever]
    public IEnumerable<SelectListItem> Pacientes { get; set; } = [];

    /// <summary>Somente leitura: soma exibida no formulário, recalculada no servidor ao salvar.</summary>
    public decimal ValorTotal => Itens.Sum(i => i.ValorTotal);

    public void DefinirPacientes(IEnumerable<PacienteOpcaoDto> pacientes) =>
        Pacientes = pacientes.Select(p => new SelectListItem(
            string.IsNullOrEmpty(p.Cpf) ? p.NomeCompleto : $"{p.NomeCompleto} — CPF {PacienteFormatacao.Cpf(p.Cpf)}",
            p.Id.ToString()));

    public OrcamentoDados ParaDados() =>
        new(PacienteId, Validade, Observacoes, Itens.Select(i => i.ParaDados()).ToList());
}

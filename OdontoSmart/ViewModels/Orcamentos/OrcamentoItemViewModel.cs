using System.ComponentModel.DataAnnotations;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

/// <summary>
/// Item do formulário de orçamento. O valor total não é recebido do navegador:
/// é apenas exibido, e o valor persistido é recalculado pelo domínio.
/// </summary>
public class OrcamentoItemViewModel
{
    [Display(Name = "Descrição")]
    [Required(ErrorMessage = "Informe a descrição do item.")]
    [StringLength(500, ErrorMessage = "A descrição deve possuir no máximo {1} caracteres.")]
    public string? Descricao { get; set; }

    [Display(Name = "Quantidade")]
    [Required(ErrorMessage = "Informe a quantidade.")]
    [Range(typeof(decimal), "0.01", "10000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "A quantidade deve ser maior que zero.")]
    public decimal? Quantidade { get; set; }

    [Display(Name = "Valor unitário")]
    [Required(ErrorMessage = "Informe o valor unitário.")]
    [Range(typeof(decimal), "0", "1000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "O valor unitário não pode ser negativo.")]
    public decimal? ValorUnitario { get; set; }

    [Display(Name = "Observações do item")]
    [StringLength(500, ErrorMessage = "As observações do item devem possuir no máximo {1} caracteres.")]
    public string? Observacoes { get; set; }

    /// <summary>Somente leitura: calculado para exibição, nunca vinculado a partir do formulário.</summary>
    public decimal ValorTotal => Quantidade is > 0 && ValorUnitario is >= 0
        ? OrcamentoItem.CalcularValorTotal(Quantidade.Value, ValorUnitario.Value)
        : 0m;

    public OrcamentoItemDados ParaDados() => new(Descricao, Quantidade, ValorUnitario, Observacoes);

    public static OrcamentoItemViewModel De(OrcamentoItemDto item) => new()
    {
        Descricao = item.Descricao,
        Quantidade = item.Quantidade,
        ValorUnitario = item.ValorUnitario,
        Observacoes = item.Observacoes
    };
}

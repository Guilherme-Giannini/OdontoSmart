using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Orcamentos;

public class OrcamentoEditViewModel : OrcamentoFormViewModel
{
    // Somente exibição: definidos pelo controller a partir do orçamento armazenado, nunca lidos do formulário.

    [BindNever]
    public Guid Id { get; set; }

    [BindNever]
    public DateTime DataCadastro { get; set; }

    [BindNever]
    public StatusOrcamento Status { get; set; }

    public void DefinirCabecalho(OrcamentoDto orcamento)
    {
        Id = orcamento.Id;
        DataCadastro = orcamento.DataCadastro;
        Status = orcamento.Status;
    }

    public static OrcamentoEditViewModel De(OrcamentoDto orcamento)
    {
        var model = new OrcamentoEditViewModel
        {
            PacienteId = orcamento.PacienteId,
            Validade = orcamento.Validade,
            Observacoes = orcamento.Observacoes,
            Itens = orcamento.Itens.Select(OrcamentoItemViewModel.De).ToList()
        };

        model.DefinirCabecalho(orcamento);
        return model;
    }
}

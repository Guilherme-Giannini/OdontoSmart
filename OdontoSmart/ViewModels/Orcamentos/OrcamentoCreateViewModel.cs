namespace OdontoSmart.Web.ViewModels.Orcamentos;

public class OrcamentoCreateViewModel : OrcamentoFormViewModel
{
    /// <summary>Formulário inicial com uma linha de item em branco.</summary>
    public static OrcamentoCreateViewModel Novo(Guid? pacienteId = null) => new()
    {
        PacienteId = pacienteId,
        Itens = [new OrcamentoItemViewModel { Quantidade = 1 }]
    };
}

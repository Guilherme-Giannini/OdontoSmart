using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Orcamentos;

public static class StatusOrcamentoExtensions
{
    public static string Descricao(this StatusOrcamento status) => status switch
    {
        StatusOrcamento.Aberto => "Aberto",
        StatusOrcamento.AguardandoDecisao => "Aguardando decisão",
        StatusOrcamento.Aprovado => "Aprovado",
        StatusOrcamento.Recusado => "Recusado",
        StatusOrcamento.Expirado => "Expirado",
        StatusOrcamento.Cancelado => "Cancelado",
        _ => status.ToString()
    };
}

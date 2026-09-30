using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OdontoSmart.Application.Common;

namespace OdontoSmart.Infraestructure.Data;

public class Transacoes(ApplicationDbContext context) : ITransacoes
{
    // Chave arbitrária e fixa do advisory lock que serializa as operações de controle de acesso.
    private const long ChaveControleAcesso = 7_281_604_531;

    public async Task<ITransacaoExclusiva> IniciarExclusivaAsync(CancellationToken cancellationToken = default)
    {
        var transacao = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Liberado automaticamente pelo PostgreSQL ao confirmar ou desfazer a transação.
            await context.Database.ExecuteSqlAsync(
                $"SELECT pg_advisory_xact_lock({ChaveControleAcesso})", cancellationToken);
        }
        catch
        {
            await transacao.DisposeAsync();
            throw;
        }

        return new TransacaoExclusiva(context, transacao);
    }

    private sealed class TransacaoExclusiva(ApplicationDbContext context, IDbContextTransaction transacao)
        : ITransacaoExclusiva
    {
        private bool _confirmada;

        public async Task ConfirmarAsync(CancellationToken cancellationToken = default)
        {
            await transacao.CommitAsync(cancellationToken);
            _confirmada = true;
        }

        public async ValueTask DisposeAsync()
        {
            await transacao.DisposeAsync();

            // Alterações desfeitas no banco não podem continuar pendentes no contexto.
            if (!_confirmada)
                context.ChangeTracker.Clear();
        }
    }
}

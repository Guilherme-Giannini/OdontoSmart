namespace OdontoSmart.Application.Common;

public interface ITransacoes
{
    /// <summary>
    /// Inicia uma transação que serializa as operações de controle de acesso (usuários, perfis e
    /// vínculos de profissionais): enquanto ela estiver aberta, outra transação exclusiva aguarda.
    /// Garante que verificações como "último administrador ativo" não sejam burladas por requisições simultâneas.
    /// Sem <see cref="ITransacaoExclusiva.ConfirmarAsync"/>, as alterações são desfeitas ao descartar.
    /// </summary>
    Task<ITransacaoExclusiva> IniciarExclusivaAsync(CancellationToken cancellationToken = default);
}

public interface ITransacaoExclusiva : IAsyncDisposable
{
    Task ConfirmarAsync(CancellationToken cancellationToken = default);
}

using OdontoSmart.Application.Common;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Orcamentos;

public class OrcamentoService(
    IOrcamentoRepository orcamentoRepository,
    IPacienteRepository pacienteRepository) : IOrcamentoService
{
    public const int TamanhoPagina = 10;

    private static readonly ErroValidacao ErroPacienteInexistente =
        new(nameof(OrcamentoDados.PacienteId), "O paciente selecionado não existe.");

    public async Task<PaginaResultado<OrcamentoResumoDto>> PesquisarAsync(
        FiltroOrcamentos filtro, int pagina, CancellationToken cancellationToken = default)
    {
        await ExpirarVencidosAsync(cancellationToken);

        // Período informado invertido é interpretado no sentido correto.
        if (filtro.DataInicio > filtro.DataFim)
            filtro = filtro with { DataInicio = filtro.DataFim, DataFim = filtro.DataInicio };

        filtro = filtro with { Busca = filtro.Busca?.Trim() };

        return await orcamentoRepository.PesquisarAsync(filtro, Math.Max(pagina, 1), TamanhoPagina, cancellationToken);
    }

    public async Task<OrcamentoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orcamento = await orcamentoRepository.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return null;

        var hoje = DateTime.Today;
        if (orcamento.ExpirarSeVencido(hoje))
            await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        var paciente = await pacienteRepository.ObterPorIdAsync(orcamento.PacienteId, cancellationToken);

        return new OrcamentoDto(
            orcamento.Id,
            orcamento.PacienteId,
            paciente?.NomeCompleto ?? string.Empty,
            paciente?.Cpf,
            paciente?.Telefone,
            paciente?.Email,
            orcamento.DataCadastro,
            orcamento.DataAtualizacao,
            orcamento.Validade,
            orcamento.Status,
            orcamento.Observacoes,
            orcamento.ValorTotal,
            orcamento.Itens
                .OrderBy(i => i.Ordem)
                .Select(i => new OrcamentoItemDto(i.Id, i.Descricao, i.Quantidade, i.ValorUnitario, i.ValorTotal, i.Observacoes))
                .ToList(),
            Orcamento.TransicoesPermitidas(orcamento.Status)
                .Where(s => s != StatusOrcamento.Aberto || !orcamento.EstaVencido(hoje))
                .ToList());
    }

    public async Task<Resultado<Guid>> CriarAsync(OrcamentoDados dados, CancellationToken cancellationToken = default)
    {
        var erros = await ValidarAsync(dados, DateTime.Today, cancellationToken);
        if (erros.Count > 0)
            return Resultado<Guid>.Falha(erros);

        var orcamento = Orcamento.Criar(dados.PacienteId!.Value, dados.Validade, dados.Observacoes, ParaDominio(dados.Itens));

        orcamentoRepository.Adicionar(orcamento);

        try
        {
            await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);
        }
        catch (PacienteInexistenteException)
        {
            return Resultado<Guid>.Falha([ErroPacienteInexistente]);
        }

        return Resultado<Guid>.Ok(orcamento.Id);
    }

    public async Task<Resultado> AtualizarAsync(Guid id, OrcamentoDados dados, CancellationToken cancellationToken = default)
    {
        var orcamento = await orcamentoRepository.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return Resultado.RecursoNaoEncontrado();

        var erros = await ValidarAsync(dados, orcamento.DataCadastro.ToLocalTime(), cancellationToken);
        if (erros.Count > 0)
            return Resultado.Falha(erros);

        orcamento.Atualizar(dados.PacienteId!.Value, dados.Validade, dados.Observacoes, ParaDominio(dados.Itens));

        try
        {
            await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);
        }
        catch (PacienteInexistenteException)
        {
            return Resultado.Falha([ErroPacienteInexistente]);
        }

        return Resultado.Ok();
    }

    public async Task<Resultado> AlterarStatusAsync(
        Guid id, StatusOrcamento novoStatus, CancellationToken cancellationToken = default)
    {
        var orcamento = await orcamentoRepository.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return Resultado.RecursoNaoEncontrado();

        var hoje = DateTime.Today;

        // A transição é avaliada sobre o status real: um orçamento vencido é expirado antes.
        var expirou = orcamento.ExpirarSeVencido(hoje);

        var erro = ValidarTransicao(orcamento, novoStatus, hoje);
        if (erro is not null)
        {
            if (expirou)
                await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);

            return Resultado.Falha([erro]);
        }

        orcamento.AlterarStatus(novoStatus, hoje);
        await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        return Resultado.Ok();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var orcamento = await orcamentoRepository.ObterPorIdAsync(id, cancellationToken);
        if (orcamento is null)
            return Resultado.RecursoNaoEncontrado();

        // Os itens carregados com o orçamento são removidos na mesma transação (exclusão em cascata).
        orcamentoRepository.Remover(orcamento);
        await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);

        return Resultado.Ok();
    }

    private static ErroValidacao? ValidarTransicao(Orcamento orcamento, StatusOrcamento novoStatus, DateTime hoje)
    {
        const string campo = "NovoStatus";

        if (!Enum.IsDefined(novoStatus))
            return new(campo, "Status inválido.");

        if (!orcamento.PodeAlterarStatusPara(novoStatus))
            return new(campo,
                $"Não é permitido alterar o status de \"{orcamento.Status.Descricao()}\" para \"{novoStatus.Descricao()}\".");

        if (novoStatus == StatusOrcamento.Aberto && orcamento.EstaVencido(hoje))
            return new(campo, "A validade deste orçamento já passou. Atualize a validade antes de reabri-lo.");

        return null;
    }

    private async Task ExpirarVencidosAsync(CancellationToken cancellationToken)
    {
        var hoje = DateTime.Today;
        var vencidos = await orcamentoRepository.ListarVencidosAsync(hoje, cancellationToken);

        var alterou = false;
        foreach (var orcamento in vencidos)
            alterou |= orcamento.ExpirarSeVencido(hoje);

        if (alterou)
            await orcamentoRepository.SalvarAlteracoesAsync(cancellationToken);
    }

    private async Task<List<ErroValidacao>> ValidarAsync(
        OrcamentoDados dados, DateTime dataCadastro, CancellationToken cancellationToken)
    {
        var erros = OrcamentoValidador.Validar(dados, dataCadastro);

        if (dados.PacienteId is { } pacienteId
            && pacienteId != Guid.Empty
            && await pacienteRepository.ObterPorIdAsync(pacienteId, cancellationToken) is null)
        {
            erros.Add(ErroPacienteInexistente);
        }

        return erros;
    }

    private static IEnumerable<DadosItemOrcamento> ParaDominio(IEnumerable<OrcamentoItemDados> itens) =>
        itens.Select(i => new DadosItemOrcamento(i.Descricao!, i.Quantidade!.Value, i.ValorUnitario!.Value, i.Observacoes));
}

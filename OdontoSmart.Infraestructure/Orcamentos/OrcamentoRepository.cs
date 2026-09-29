using Microsoft.EntityFrameworkCore;
using Npgsql;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Orcamentos;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Domain.Enums;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Data.Configurations;

namespace OdontoSmart.Infraestructure.Orcamentos;

public class OrcamentoRepository(ApplicationDbContext context) : IOrcamentoRepository
{
    public Task<Orcamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Orcamentos
            .Include(o => o.Itens)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<PaginaResultado<OrcamentoResumoDto>> PesquisarAsync(
        FiltroOrcamentos filtro, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var query =
            from orcamento in context.Orcamentos.AsNoTracking()
            join paciente in context.Pacientes.AsNoTracking() on orcamento.PacienteId equals paciente.Id
            select new { Orcamento = orcamento, Paciente = paciente };

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var padraoNome = Like.Contem(filtro.Busca.Trim());
            var digitos = Digitos.Extrair(filtro.Busca);

            if (digitos.Length > 0)
            {
                // O CPF é armazenado somente com dígitos: a busca ignora a máscara digitada.
                var padraoCpf = $"%{digitos}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.Paciente.NomeCompleto, padraoNome) ||
                    (x.Paciente.Cpf != null && EF.Functions.Like(x.Paciente.Cpf, padraoCpf)));
            }
            else
            {
                query = query.Where(x => EF.Functions.ILike(x.Paciente.NomeCompleto, padraoNome));
            }
        }

        if (filtro.Status is { } status)
            query = query.Where(x => x.Orcamento.Status == status);

        // O período é informado em datas locais; DataCadastro é armazenada em UTC.
        if (filtro.DataInicio is { } dataInicio)
        {
            var inicioUtc = ParaUtc(dataInicio.Date);
            query = query.Where(x => x.Orcamento.DataCadastro >= inicioUtc);
        }

        if (filtro.DataFim is { } dataFim)
        {
            var fimExclusivoUtc = ParaUtc(dataFim.Date.AddDays(1));
            query = query.Where(x => x.Orcamento.DataCadastro < fimExclusivoUtc);
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderByDescending(x => x.Orcamento.DataCadastro)
            .ThenBy(x => x.Orcamento.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new OrcamentoResumoDto(
                x.Orcamento.Id,
                x.Paciente.Id,
                x.Paciente.NomeCompleto,
                x.Paciente.Cpf,
                x.Orcamento.DataCadastro,
                x.Orcamento.Validade,
                x.Orcamento.Status,
                x.Orcamento.Itens.Count,
                x.Orcamento.ValorTotal))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<OrcamentoResumoDto>(itens, total, pagina, tamanhoPagina);
    }

    public async Task<IReadOnlyList<Orcamento>> ListarVencidosAsync(DateTime hoje, CancellationToken cancellationToken = default)
    {
        var data = hoje.Date;

        return await context.Orcamentos
            .Where(o => (o.Status == StatusOrcamento.Aberto || o.Status == StatusOrcamento.AguardandoDecisao)
                && o.Validade != null
                && o.Validade < data)
            .ToListAsync(cancellationToken);
    }

    public void Adicionar(Orcamento orcamento) => context.Orcamentos.Add(orcamento);

    public void Remover(Orcamento orcamento) => context.Orcamentos.Remove(orcamento);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ForeignKeyViolation,
            ConstraintName: OrcamentoConfiguration.ChaveEstrangeiraPaciente
        })
        {
            // Mantém o contexto utilizável após a falha.
            context.ChangeTracker.Clear();
            throw new PacienteInexistenteException(ex);
        }
    }

    private static DateTime ParaUtc(DateTime dataLocal) =>
        DateTime.SpecifyKind(dataLocal, DateTimeKind.Local).ToUniversalTime();
}

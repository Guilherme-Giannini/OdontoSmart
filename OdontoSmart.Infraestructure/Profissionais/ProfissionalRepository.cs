using Microsoft.EntityFrameworkCore;
using Npgsql;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Data.Configurations;

namespace OdontoSmart.Infraestructure.Profissionais;

public class ProfissionalRepository(ApplicationDbContext context) : IProfissionalRepository
{
    public Task<Profissional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Profissionais.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PaginaResultado<ProfissionalResumoDto>> PesquisarAsync(
        string? termo, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var query =
            from profissional in context.Profissionais.AsNoTracking()
            join usuario in context.Users.AsNoTracking() on profissional.UsuarioId equals usuario.Id into usuarios
            from usuario in usuarios.DefaultIfEmpty()
            select new { Profissional = profissional, UsuarioNome = usuario != null ? usuario.NomeCompleto : null };

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var padraoNome = Like.Contem(termo.Trim());
            var digitos = Digitos.Extrair(termo);

            if (digitos.Length > 0)
            {
                // O CRO é armazenado somente com dígitos.
                var padraoCro = $"%{digitos}%";
                query = query.Where(x =>
                    EF.Functions.ILike(x.Profissional.NomeExibicao, padraoNome) ||
                    EF.Functions.Like(x.Profissional.Cro, padraoCro));
            }
            else
            {
                query = query.Where(x => EF.Functions.ILike(x.Profissional.NomeExibicao, padraoNome));
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(x => x.Profissional.NomeExibicao)
            .ThenBy(x => x.Profissional.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new ProfissionalResumoDto(
                x.Profissional.Id,
                x.Profissional.NomeExibicao,
                x.Profissional.Cro,
                x.Profissional.CroUf,
                x.Profissional.Especialidade,
                x.UsuarioNome,
                x.Profissional.Ativo))
            .ToListAsync(cancellationToken);

        return new PaginaResultado<ProfissionalResumoDto>(itens, total, pagina, tamanhoPagina);
    }

    public Task<bool> ExisteCroAsync(
        string croUf, string cro, Guid? ignorarProfissionalId, CancellationToken cancellationToken = default) =>
        context.Profissionais.AnyAsync(
            p => p.CroUf == croUf && p.Cro == cro && (ignorarProfissionalId == null || p.Id != ignorarProfissionalId),
            cancellationToken);

    public Task<bool> PossuiVinculoAsync(
        Guid usuarioId, Guid? ignorarProfissionalId, CancellationToken cancellationToken = default) =>
        context.Profissionais.AnyAsync(
            p => p.UsuarioId == usuarioId && (ignorarProfissionalId == null || p.Id != ignorarProfissionalId),
            cancellationToken);

    public async Task<IReadOnlyCollection<Guid>> ListarUsuariosVinculadosAsync(CancellationToken cancellationToken = default) =>
        await context.Profissionais
            .Where(p => p.UsuarioId != null)
            .Select(p => p.UsuarioId!.Value)
            .ToHashSetAsync(cancellationToken);

    public void Adicionar(Profissional profissional) => context.Profissionais.Add(profissional);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ProfissionalConfiguration.IndiceCroUnico or ProfissionalConfiguration.IndiceUsuarioUnico
        } violacao)
        {
            // Mantém o contexto utilizável após a falha.
            context.ChangeTracker.Clear();

            if (violacao.ConstraintName == ProfissionalConfiguration.IndiceCroUnico)
                throw new CroDuplicadoException(ex);

            throw new UsuarioJaVinculadoException(ex);
        }
    }
}

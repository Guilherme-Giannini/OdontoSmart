using Microsoft.EntityFrameworkCore;
using Npgsql;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Pacientes;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;
using OdontoSmart.Infraestructure.Data;
using OdontoSmart.Infraestructure.Data.Configurations;

namespace OdontoSmart.Infraestructure.Pacientes;

public class PacienteRepository(ApplicationDbContext context) : IPacienteRepository
{
    public Task<Paciente?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Pacientes.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PaginaResultado<Paciente>> PesquisarAsync(
        string? termo, int pagina, int tamanhoPagina, CancellationToken cancellationToken = default)
    {
        var query = context.Pacientes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var padraoNome = $"%{EscaparLike(termo.Trim())}%";
            var digitos = Digitos.Extrair(termo);

            if (digitos.Length > 0)
            {
                // CPF e telefone são armazenados somente com dígitos: a busca ignora a máscara digitada.
                var padraoDigitos = $"%{digitos}%";
                query = query.Where(p =>
                    EF.Functions.ILike(p.NomeCompleto, padraoNome) ||
                    (p.Cpf != null && EF.Functions.Like(p.Cpf, padraoDigitos)) ||
                    (p.Telefone != null && EF.Functions.Like(p.Telefone, padraoDigitos)));
            }
            else
            {
                query = query.Where(p => EF.Functions.ILike(p.NomeCompleto, padraoNome));
            }
        }

        var total = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(p => p.NomeCompleto)
            .ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return new PaginaResultado<Paciente>(itens, total, pagina, tamanhoPagina);
    }

    public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarPacienteId, CancellationToken cancellationToken = default) =>
        context.Pacientes.AnyAsync(
            p => p.Cpf == cpf && (ignorarPacienteId == null || p.Id != ignorarPacienteId),
            cancellationToken);

    public void Adicionar(Paciente paciente) => context.Pacientes.Add(paciente);

    public void Remover(Paciente paciente) => context.Pacientes.Remove(paciente);

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: PacienteConfiguration.IndiceCpfUnico
        })
        {
            // Mantém o contexto utilizável após a falha.
            context.ChangeTracker.Clear();
            throw new CpfDuplicadoException(ex);
        }
    }

    // Escapa os curingas do LIKE para que o termo digitado seja tratado literalmente
    // (o PostgreSQL usa "\" como caractere de escape padrão).
    private static string EscaparLike(string valor) =>
        valor.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
}

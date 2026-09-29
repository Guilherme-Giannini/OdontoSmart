using OdontoSmart.Application.Common;
using OdontoSmart.Domain.Common;
using OdontoSmart.Domain.Entities;

namespace OdontoSmart.Application.Pacientes;

public class PacienteService(IPacienteRepository repository) : IPacienteService
{
    public const int TamanhoPagina = 10;

    private static readonly ErroValidacao ErroCpfDuplicado =
        new(nameof(PacienteDados.Cpf), "Já existe um paciente cadastrado com este CPF.");

    public async Task<PaginaResultado<PacienteDto>> PesquisarAsync(
        string? termo, int pagina, CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(pagina, 1);

        var resultado = await repository.PesquisarAsync(termo?.Trim(), pagina, TamanhoPagina, cancellationToken);

        return new PaginaResultado<PacienteDto>(
            resultado.Itens.Select(PacienteDto.De).ToList(),
            resultado.TotalRegistros,
            resultado.Pagina,
            resultado.TamanhoPagina);
    }

    public async Task<PacienteDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paciente = await repository.ObterPorIdAsync(id, cancellationToken);
        return paciente is null ? null : PacienteDto.De(paciente);
    }

    public Task<IReadOnlyList<PacienteOpcaoDto>> ListarOpcoesAsync(CancellationToken cancellationToken = default) =>
        repository.ListarOpcoesAsync(cancellationToken);

    public async Task<Resultado<Guid>> CriarAsync(PacienteDados dados, CancellationToken cancellationToken = default)
    {
        var erros = await ValidarAsync(dados, pacienteId: null, cancellationToken);
        if (erros.Count > 0)
            return Resultado<Guid>.Falha(erros);

        var paciente = Paciente.Criar(
            dados.NomeCompleto, dados.Cpf, dados.DataNascimento, dados.Sexo,
            dados.Telefone, dados.Email, dados.Observacoes);

        repository.Adicionar(paciente);

        try
        {
            await repository.SalvarAlteracoesAsync(cancellationToken);
        }
        catch (CpfDuplicadoException)
        {
            return Resultado<Guid>.Falha([ErroCpfDuplicado]);
        }

        return Resultado<Guid>.Ok(paciente.Id);
    }

    public async Task<Resultado> AtualizarAsync(Guid id, PacienteDados dados, CancellationToken cancellationToken = default)
    {
        var paciente = await repository.ObterPorIdAsync(id, cancellationToken);
        if (paciente is null)
            return Resultado.RecursoNaoEncontrado();

        var erros = await ValidarAsync(dados, id, cancellationToken);
        if (erros.Count > 0)
            return Resultado.Falha(erros);

        paciente.Atualizar(
            dados.NomeCompleto, dados.Cpf, dados.DataNascimento, dados.Sexo,
            dados.Telefone, dados.Email, dados.Observacoes);

        try
        {
            await repository.SalvarAlteracoesAsync(cancellationToken);
        }
        catch (CpfDuplicadoException)
        {
            return Resultado.Falha([ErroCpfDuplicado]);
        }

        return Resultado.Ok();
    }

    public async Task<Resultado> ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var paciente = await repository.ObterPorIdAsync(id, cancellationToken);
        if (paciente is null)
            return Resultado.RecursoNaoEncontrado();

        if (await repository.PossuiOrcamentosAsync(id, cancellationToken))
            return Resultado.Falha([new(string.Empty,
                "Não é possível excluir o paciente porque existem orçamentos associados a ele.")]);

        repository.Remover(paciente);
        await repository.SalvarAlteracoesAsync(cancellationToken);

        return Resultado.Ok();
    }

    private async Task<List<ErroValidacao>> ValidarAsync(
        PacienteDados dados, Guid? pacienteId, CancellationToken cancellationToken)
    {
        var erros = PacienteValidador.Validar(dados);

        var cpf = Cpf.Normalizar(dados.Cpf);
        if (cpf is not null
            && erros.All(e => e.Campo != nameof(PacienteDados.Cpf))
            && await repository.ExisteCpfAsync(cpf, pacienteId, cancellationToken))
        {
            erros.Add(ErroCpfDuplicado);
        }

        return erros;
    }
}

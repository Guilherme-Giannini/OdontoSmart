namespace OdontoSmart.Application.Profissionais;

/// <summary>
/// Dados editáveis de um profissional, informados pelo administrador no cadastro ou na edição.
/// </summary>
public sealed record ProfissionalDados(
    string? NomeExibicao,
    string? Cro,
    string? CroUf,
    string? Especialidade,
    string? Telefone,
    string? Email,
    Guid? UsuarioId);

public sealed record ProfissionalResumoDto(
    Guid Id,
    string NomeExibicao,
    string Cro,
    string CroUf,
    string? Especialidade,
    string? UsuarioNome,
    bool Ativo);

public sealed record ProfissionalDto(
    Guid Id,
    string NomeExibicao,
    string Cro,
    string CroUf,
    string? Especialidade,
    string? Telefone,
    string? Email,
    Guid? UsuarioId,
    string? UsuarioNome,
    string? UsuarioEmail,
    bool Ativo,
    DateTime DataCadastro,
    DateTime? DataAtualizacao);

using OdontoSmart.Application.Profissionais;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Profissionais;

/// <summary>Dados de exibição de um profissional.</summary>
public class ProfissionalDetailsViewModel
{
    public Guid Id { get; init; }
    public string NomeExibicao { get; init; } = string.Empty;
    public string Cro { get; init; } = string.Empty;
    public string Especialidade { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string UsuarioVinculado { get; init; } = string.Empty;
    public bool Ativo { get; init; }
    public string DataCadastro { get; init; } = string.Empty;
    public string DataAtualizacao { get; init; } = string.Empty;

    public static ProfissionalDetailsViewModel De(ProfissionalDto profissional) => new()
    {
        Id = profissional.Id,
        NomeExibicao = profissional.NomeExibicao,
        Cro = ProfissionalFormatacao.Cro(profissional.Cro, profissional.CroUf),
        Especialidade = profissional.Especialidade ?? string.Empty,
        Telefone = PacienteFormatacao.Telefone(profissional.Telefone),
        Email = profissional.Email ?? string.Empty,
        UsuarioVinculado = profissional.UsuarioNome is null
            ? string.Empty
            : $"{profissional.UsuarioNome} ({profissional.UsuarioEmail})",
        Ativo = profissional.Ativo,
        DataCadastro = PacienteFormatacao.DataHora(profissional.DataCadastro),
        DataAtualizacao = PacienteFormatacao.DataHora(profissional.DataAtualizacao)
    };
}

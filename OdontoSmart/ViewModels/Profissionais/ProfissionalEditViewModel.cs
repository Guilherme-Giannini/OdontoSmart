using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Application.Profissionais;
using OdontoSmart.Web.ViewModels.Pacientes;

namespace OdontoSmart.Web.ViewModels.Profissionais;

public class ProfissionalEditViewModel : ProfissionalFormViewModel
{
    /// <summary>Definido pelo controller a partir da rota; nunca lido do formulário.</summary>
    [BindNever]
    public Guid Id { get; set; }

    public static ProfissionalEditViewModel De(ProfissionalDto profissional) => new()
    {
        Id = profissional.Id,
        NomeExibicao = profissional.NomeExibicao,
        Cro = profissional.Cro,
        CroUf = profissional.CroUf,
        Especialidade = profissional.Especialidade,
        Telefone = PacienteFormatacao.Telefone(profissional.Telefone),
        Email = profissional.Email,
        UsuarioId = profissional.UsuarioId
    };
}

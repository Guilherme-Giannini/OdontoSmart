using Microsoft.AspNetCore.Mvc.ModelBinding;
using OdontoSmart.Application.Usuarios;

namespace OdontoSmart.Web.ViewModels.Usuarios;

public class UsuarioEditViewModel : UsuarioFormViewModel
{
    // Somente exibição: definidos pelo controller a partir do usuário armazenado, nunca lidos do formulário.

    [BindNever]
    public Guid Id { get; set; }

    [BindNever]
    public bool EhUsuarioAtual { get; set; }

    public void DefinirCabecalho(UsuarioDto usuario, Guid? usuarioAtualId)
    {
        Id = usuario.Id;
        EhUsuarioAtual = usuario.Id == usuarioAtualId;
    }

    public static UsuarioEditViewModel De(UsuarioDto usuario, Guid? usuarioAtualId)
    {
        var model = new UsuarioEditViewModel
        {
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Perfil = usuario.Perfil
        };

        model.DefinirCabecalho(usuario, usuarioAtualId);
        return model;
    }
}

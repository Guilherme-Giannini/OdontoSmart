using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

public static class PerfilUsuarioExtensions
{
    public static string Descricao(this PerfilUsuario perfil) => perfil switch
    {
        PerfilUsuario.Administrador => "Administrador",
        PerfilUsuario.Dentista => "Dentista",
        PerfilUsuario.Recepcao => "Recepção",
        _ => perfil.ToString()
    };
}

using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Application.Usuarios;

/// <summary>Usuário autenticado na requisição atual. Nunca é obtido de dados do formulário.</summary>
public interface IUsuarioAtual
{
    /// <summary>Null quando não há usuário autenticado.</summary>
    Guid? Id { get; }

    string? NomeCompleto { get; }

    PerfilUsuario? Perfil { get; }
}

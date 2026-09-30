namespace OdontoSmart.Application.Usuarios;

/// <summary>
/// Tipos de claim adicionais da sessão, gravados pela infraestrutura e lidos pela camada Web.
/// O perfil é gravado como role e o Id como NameIdentifier (padrão do Identity).
/// </summary>
public static class ClaimsUsuario
{
    public const string NomeCompleto = "odontosmart:nome_completo";
    public const string DeveTrocarSenha = "odontosmart:deve_trocar_senha";
}

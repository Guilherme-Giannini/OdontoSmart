namespace OdontoSmart.Application.Profissionais;

/// <summary>
/// Lançada pela persistência quando a restrição de unicidade de (UF, CRO) é violada
/// (por exemplo, dois cadastros simultâneos com o mesmo CRO).
/// </summary>
public class CroDuplicadoException(Exception? innerException = null)
    : Exception("Já existe um profissional cadastrado com este CRO nesta UF.", innerException);

/// <summary>
/// Lançada pela persistência quando a restrição de unicidade do usuário vinculado é violada.
/// </summary>
public class UsuarioJaVinculadoException(Exception? innerException = null)
    : Exception("O usuário já está vinculado a outro profissional.", innerException);
